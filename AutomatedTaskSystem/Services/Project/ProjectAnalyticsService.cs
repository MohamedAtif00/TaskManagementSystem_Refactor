using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.Projects;
using AutomatedTaskSystem.Dtos.SprintDtos;
using AutomatedTaskSystem.Helper;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.TaskStatus;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.Sprint;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using TaskModel = AutomatedTaskSystem.Models.Task;

namespace AutomatedTaskSystem.Services.Project;

public class ProjectAnalyticsService : IProjectAnalyticsService
{
    private readonly DataContext _context;

    public ProjectAnalyticsService(DataContext context)
    {
        _context = context;
    }

    private async Task<DbConnection> GetOpenConnectionAsync()
    {
        var connection = _context.Database.GetDbConnection();
        await connection.EnsureOpenAsync();
        return connection;
    }

    private static (DateTime? startDate, DateTime? endDate) GetDateRangeFromTimePeriod(TimePeriodFilter? timePeriod)
    {
        if (timePeriod == null || timePeriod == TimePeriodFilter.AllTime)
            return (null, null);

        var now = DateTime.Now;
        var today = now.Date;

        return timePeriod switch
        {
            TimePeriodFilter.Today => (today, today.AddDays(1).AddTicks(-1)),
            TimePeriodFilter.LastWeek => (today.AddDays(-7), now),
            TimePeriodFilter.LastMonth => (today.AddMonths(-1), now),
            _ => (null, null)
        };
    }

    private static bool IsOldLearningObjectiveName(string? name) =>
        !string.IsNullOrWhiteSpace(name) && name.Contains("old", StringComparison.OrdinalIgnoreCase);

    public async Task<ResponseService<ProjectOverviewAnalyticsDto>> GetProjectOverviewAsync(int projectId, TimePeriodFilter? timePeriod = null)
    {
        var response = new ResponseService<ProjectOverviewAnalyticsDto>();

        try
        {
            var (startDate, endDate) = GetDateRangeFromTimePeriod(timePeriod);

            var projectExists = await _context.Projects
                .AsNoTracking()
                .AnyAsync(p => p.Id == projectId);

            if (!projectExists)
            {
                response.Error = true;
                response.Message = "Project not found.";
                return response;
            }

            var connection = await GetOpenConnectionAsync();

            const string aggregateSql = """
SELECT
    NumberOfUnits = (
        SELECT COUNT(1)
        FROM Units u
        WHERE u.ProjectId = @projectId AND u.Archived = 0
    ),
    NumberOfLessons = (
        SELECT COUNT(1)
        FROM Lessons l
        INNER JOIN Units u ON u.Id = l.UnitId
        WHERE u.ProjectId = @projectId AND u.Archived = 0 AND l.Archived = 0
    ),
    NumberOfLearningObjectives = (
        SELECT COUNT(1)
        FROM LearningObjectives lo
        INNER JOIN Lessons l ON l.Id = lo.LessonId
        INNER JOIN Units u ON u.Id = l.UnitId
        WHERE u.ProjectId = @projectId AND u.Archived = 0 AND l.Archived = 0 AND lo.Archived = 0
          AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%'
    ),
    TotalExpectedTasks = (
        SELECT COUNT(1)
        FROM LearningObjectives lo
        INNER JOIN Lessons l ON l.Id = lo.LessonId
        INNER JOIN Units u ON u.Id = l.UnitId
        INNER JOIN Nodes n ON n.SchemaId = lo.SchemaId
        INNER JOIN Steps s ON s.NodeId = n.Id
        WHERE u.ProjectId = @projectId
          AND u.Archived = 0 AND l.Archived = 0 AND lo.Archived = 0
          AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%'
          AND n.Archived = 0 AND s.Archived = 0
    );
""";

            var aggregates = await connection.QuerySingleAsync<ProjectOverviewAggregatesRow>(
                aggregateSql,
                new { projectId }
            );

            const string taskSummarySql = """
                        SELECT
                            Completed = ISNULL(SUM(CASE WHEN t.Status = @done THEN 1 ELSE 0 END), 0),
                            Active = ISNULL(SUM(CASE WHEN t.Status IN (@todo, @doing) THEN 1 ELSE 0 END), 0),
                            [Rollback] = ISNULL(SUM(CASE WHEN t.IsRollback = 1 THEN 1 ELSE 0 END), 0),
                            Flagged = ISNULL(SUM(CASE WHEN t.Flagged = 1 THEN 1 ELSE 0 END), 0)
                        FROM Tasks t
                        INNER JOIN LearningObjectives lo ON lo.Id = t.LearningObjectiveId
                        INNER JOIN Lessons l ON l.Id = lo.LessonId
                        INNER JOIN Units u ON u.Id = l.UnitId
                        WHERE u.ProjectId = @projectId
                          AND u.Archived = 0 AND l.Archived = 0 AND lo.Archived = 0
                          AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%'
                          AND t.Archived = 0;
                        """;

            var taskSummaryRow = await connection.QuerySingleAsync<ProjectTaskSummaryRow>(
                taskSummarySql,
                new
                {
                    projectId,
                    done = (int)TaskStatusEnum.Done,
                    todo = (int)TaskStatusEnum.ToDo,
                    doing = (int)TaskStatusEnum.Doing
                }
            );

            var totalExpectedTasks = aggregates.TotalExpectedTasks;
            var completedCount = taskSummaryRow.Completed;
            var activeCount = taskSummaryRow.Active;
            var notStartedCount = aggregates.TotalExpectedTasks - completedCount - activeCount;

            var taskSummary = new TaskSummaryDto
            {
                Active = activeCount,
                Completed = completedCount,
                Rollback = taskSummaryRow.Rollback,
                Flagged = taskSummaryRow.Flagged,
                NotStarted = Math.Max(0, notStartedCount),
                Total = aggregates.TotalExpectedTasks
            };

            List<string> preferredGroupSteps = new()
            {
                "ID",
                "id reviewer",
                "SME",
                "Proofreading",
                "VO",
                "GD",
                "Motion",
                "Animation",
                "developers",
                "Native developer",
                "data allocation",
                "tester",
                "qc"
            };

            const string tagsSql = """
                        SELECT
                            t.GroupId AS GroupId,
                            g.Name AS [Label],
                            g.ColorCode AS ColorCode,
                            COUNT(1) AS [Value]
                        FROM Tasks t
                        INNER JOIN Groups g ON g.Id = t.GroupId
                        INNER JOIN LearningObjectives lo ON lo.Id = t.LearningObjectiveId
                        INNER JOIN Lessons l ON l.Id = lo.LessonId
                        INNER JOIN Units u ON u.Id = l.UnitId
                        WHERE u.ProjectId = @projectId
                          AND u.Archived = 0 AND l.Archived = 0 AND lo.Archived = 0
                          AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%'
                          AND t.Archived = 0
                          AND t.Status IN (@backlog, @todo, @doing)
                          AND (@startDate IS NULL OR (t.CreatedAt >= @startDate AND t.CreatedAt <= @endDate))
                        GROUP BY t.GroupId, g.Name, g.ColorCode;
                        """;

            var tagRows = (await connection.QueryAsync<ProjectOverviewTagRow>(
                    tagsSql,
                    new
                    {
                        projectId,
                        backlog = (int)TaskStatusEnum.Backlog,
                        todo = (int)TaskStatusEnum.ToDo,
                        doing = (int)TaskStatusEnum.Doing,
                        startDate,
                        endDate
                    }
                ))
                .ToList();

            var tagData = tagRows
                .Where(r => r.Value > 0)
                .Select(r => new TagDataDto
                {
                    GroupId = r.GroupId,
                    Label = r.Label ?? "",
                    Value = r.Value,
                    Color = string.IsNullOrEmpty(r.ColorCode) ? "#6b7280" : r.ColorCode,
                    IsFilled = false
                })
                .OrderBy(tag =>
                {
                    var index = preferredGroupSteps.FindIndex(s => s.Equals(tag.Label, StringComparison.OrdinalIgnoreCase));
                    return index == -1 ? short.MaxValue : index;
                })
                .ToList();

            const string loSummarySql = """
WITH ProjectLOs AS (
    SELECT lo.Id, lo.SchemaId
    FROM LearningObjectives lo
    INNER JOIN Lessons l ON l.Id = lo.LessonId
    INNER JOIN Units u ON u.Id = l.UnitId
    WHERE u.ProjectId = @projectId
      AND u.Archived = 0 AND l.Archived = 0 AND lo.Archived = 0
      AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%'
),
ExpectedSteps AS (
    SELECT pl.Id AS LearningObjectiveId, COUNT(1) AS ExpectedCount
    FROM ProjectLOs pl
    INNER JOIN Nodes n ON n.SchemaId = pl.SchemaId AND n.Archived = 0
    INNER JOIN Steps s ON s.NodeId = n.Id AND s.Archived = 0
    GROUP BY pl.Id
),
TaskAgg AS (
    SELECT t.LearningObjectiveId,
           TotalTasks = COUNT(1),
           NonBacklogTasks = SUM(CASE WHEN t.Status <> @backlog THEN 1 ELSE 0 END),
           NotDoneTasks = SUM(CASE WHEN t.Status <> @done THEN 1 ELSE 0 END)
    FROM Tasks t
    INNER JOIN ProjectLOs pl ON pl.Id = t.LearningObjectiveId
    WHERE t.Archived = 0
    GROUP BY t.LearningObjectiveId
)
SELECT
    Total = (SELECT COUNT(1) FROM ProjectLOs),
    Completed = ISNULL(SUM(CASE WHEN ISNULL(ta.TotalTasks, 0) > 0 AND ISNULL(ta.NotDoneTasks, 0) = 0 THEN 1 ELSE 0 END), 0),
    NotStarted = ISNULL(SUM(CASE
        WHEN ISNULL(ta.TotalTasks, 0) = 0 AND ISNULL(es.ExpectedCount, 0) > 0 THEN 1
        WHEN ISNULL(ta.TotalTasks, 0) > 0 AND ISNULL(ta.NonBacklogTasks, 0) = 0 THEN 1
        ELSE 0
    END), 0)
FROM ProjectLOs pl
LEFT JOIN ExpectedSteps es ON es.LearningObjectiveId = pl.Id
LEFT JOIN TaskAgg ta ON ta.LearningObjectiveId = pl.Id;
""";

            var loSummaryRow = await connection.QuerySingleAsync<ProjectOverviewLoSummaryRow>(
                loSummarySql,
                new
                {
                    projectId,
                    backlog = (int)TaskStatusEnum.Backlog,
                    done = (int)TaskStatusEnum.Done
                }
            );

            var totalLOs = loSummaryRow.Total;
            var completedLOs = loSummaryRow.Completed;
            var notStartedLOs = loSummaryRow.NotStarted;
            var inProcessLOs = totalLOs - completedLOs - notStartedLOs;

            var loSummary = new LearningObjectiveSummaryDto
            {
                Completed = completedLOs,
                NotStarted = notStartedLOs,
                InProcess = inProcessLOs,
                Total = totalLOs
            };

            var progressPercent = totalExpectedTasks > 0
                ? Math.Min(100, (int)Math.Round((double)completedCount / totalExpectedTasks * 100))
                : 0;

            response.Data = new ProjectOverviewAnalyticsDto
            {
                NumberOfUnits = aggregates.NumberOfUnits,
                NumberOfLessons = aggregates.NumberOfLessons,
                NumberOfLearningObjectives = aggregates.NumberOfLearningObjectives,
                ProjectProgress = new ProjectProgressDto
                {
                    ProgressPercent = progressPercent,
                    TotalExpectedTasks = aggregates.TotalExpectedTasks,
                    CompletedTasks = completedCount,
                    ActiveTasks = activeCount
                },
                LearningActivitiesSummary = new ProjectLearningActivitiesSummaryDto
                {
                    LoSummary = loSummary,
                    Tags = tagData
                },
                TasksSummary = taskSummary
            };

            response.Message = "Project overview analytics retrieved successfully.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetProjectOverviewAsync: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            response.Error = true;
            response.Message = $"An unexpected error occurred: {ex.Message}";
        }

        return response;
    }

    private sealed record ProjectOverviewAggregatesRow(
        int NumberOfUnits,
        int NumberOfLessons,
        int NumberOfLearningObjectives,
        int TotalExpectedTasks
    );

    private sealed record ProjectTaskSummaryRow(
        int Completed,
        int Active,
        int Rollback,
        int Flagged
    );

    private sealed record ProjectOverviewTagRow(
        int GroupId,
        string? Label,
        string? ColorCode,
        int Value
    );

    private sealed record ProjectOverviewLoSummaryRow(
        int Total,
        int Completed,
        int NotStarted
    );

    public async Task<ResponseService<ProjectLearningObjectivesProgressDto>> GetProjectLearningObjectivesProgressAsync(int projectId, TimePeriodFilter? timePeriod = null, int? groupId = null)
    {
        var response = new ResponseService<ProjectLearningObjectivesProgressDto>();

        try
        {
            var (startDate, endDate) = GetDateRangeFromTimePeriod(timePeriod);
            var projectExists = await _context.Projects
                .AsNoTracking()
                .AnyAsync(p => p.Id == projectId);

            if (!projectExists)
            {
                response.Error = true;
                response.Message = "Project not found.";
                return response;
            }

            var connection = await GetOpenConnectionAsync();

            const string loProgressSql = """
                    WITH ProjectLOs AS (
                        SELECT lo.Id, lo.Name, lo.SchemaId
                        FROM LearningObjectives lo
                        INNER JOIN Lessons l ON l.Id = lo.LessonId AND l.Archived = 0
                        INNER JOIN Units u ON u.Id = l.UnitId AND u.Archived = 0
                        WHERE u.ProjectId = @projectId
                          AND lo.Archived = 0
                                  AND LOWER(lo.Name) NOT LIKE '%old%'
                    ),
                    Expected AS (
                        SELECT
                            pl.Id AS LearningObjectiveId,
                            TotalExpectedTasks = COUNT(1),
                            TotalExpectedDuration = ISNULL(SUM(tb.Duration), 0)
                        FROM ProjectLOs pl
                        INNER JOIN Nodes n ON n.SchemaId = pl.SchemaId AND n.Archived = 0
                        INNER JOIN Steps s ON s.NodeId = n.Id AND s.Archived = 0
                        LEFT JOIN TaskBank tb ON tb.Id = s.TaskBankId
                        GROUP BY pl.Id
                    ),
                    FilteredTasks AS (
                        SELECT t.Id, t.LearningObjectiveId, t.Status
                        FROM Tasks t
                        INNER JOIN ProjectLOs pl ON pl.Id = t.LearningObjectiveId
                        WHERE t.Archived = 0
                          AND (@groupId IS NULL OR t.GroupId = @groupId)
                          AND (@startDate IS NULL OR (t.CreatedAt >= @startDate AND t.CreatedAt <= @endDate))
                    ),
                    Completed AS (
                        SELECT ft.LearningObjectiveId, CompletedTasks = COUNT(1)
                        FROM FilteredTasks ft
                        WHERE ft.Status = @done
                        GROUP BY ft.LearningObjectiveId
                    ),
                    Actual AS (
                        SELECT ft.LearningObjectiveId,
                               TotalActualMinutes = ISNULL(SUM(CAST(twt.Duration AS float)) / 60000.0, 0)
                        FROM FilteredTasks ft
                        INNER JOIN TaskWorkTimes twt ON twt.TaskId = ft.Id
                        GROUP BY ft.LearningObjectiveId
                    )
                    SELECT
                        pl.Id,
                        pl.Name,
                        TotalExpectedTasks = ISNULL(e.TotalExpectedTasks, 0),
                        TotalExpectedDuration = ISNULL(e.TotalExpectedDuration, 0),
                        CompletedTasks = ISNULL(c.CompletedTasks, 0),
                        TotalActualMinutes = ISNULL(a.TotalActualMinutes, 0)
                    FROM ProjectLOs pl
                    LEFT JOIN Expected e ON e.LearningObjectiveId = pl.Id
                    LEFT JOIN Completed c ON c.LearningObjectiveId = pl.Id
                    LEFT JOIN Actual a ON a.LearningObjectiveId = pl.Id
                    WHERE (@groupId IS NULL OR EXISTS (
                        SELECT 1 FROM Tasks t
                        WHERE t.LearningObjectiveId = pl.Id AND t.Archived = 0 AND t.GroupId = @groupId
                    ))
                    ORDER BY pl.Id;
                    """;

            var loRows = (await connection.QueryAsync<ProjectLoProgressRow>(
                loProgressSql,
                new
                {
                    projectId,
                    groupId,
                    startDate,
                    endDate,
                    done = (int)TaskStatusEnum.Done
                }
            )).ToList();

            const string phasesSql = """
SELECT
    lo.Id AS LearningObjectiveId,
    g.Name AS GroupName,
    g.ColorCode AS ColorCode
FROM LearningObjectives lo
INNER JOIN Lessons l ON l.Id = lo.LessonId AND l.Archived = 0
INNER JOIN Units u ON u.Id = l.UnitId AND u.Archived = 0
INNER JOIN Tasks t ON t.LearningObjectiveId = lo.Id AND t.Archived = 0
INNER JOIN Groups g ON g.Id = t.GroupId
WHERE u.ProjectId = @projectId
  AND lo.Archived = 0
  AND LOWER(lo.Name) NOT LIKE '%old%'
  AND t.Status IN (@backlog, @todo, @doing)
  AND (@groupId IS NULL OR t.GroupId = @groupId);
""";

            var phaseRows = (await connection.QueryAsync<ProjectLoPhaseRow>(
                phasesSql,
                new
                {
                    projectId,
                    groupId,
                    backlog = (int)TaskStatusEnum.Backlog,
                    todo = (int)TaskStatusEnum.ToDo,
                    doing = (int)TaskStatusEnum.Doing
                }
            )).ToList();

            var phasesByLoId = phaseRows
                .GroupBy(r => r.LearningObjectiveId)
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .Where(x => !string.IsNullOrWhiteSpace(x.GroupName))
                        .DistinctBy(x => x.GroupName)
                        .Select(x => new CurrentPhaseDto
                        {
                            GroupName = x.GroupName!,
                            ColorCode = string.IsNullOrEmpty(x.ColorCode) ? "#6b7280" : x.ColorCode
                        })
                        .ToList()
                );

            var data = loRows.Select(r =>
            {
                var completionPercentage = r.TotalExpectedTasks > 0
                    ? Math.Min(100, (int)Math.Round((double)r.CompletedTasks / r.TotalExpectedTasks * 100))
                    : 0;

                var status =
                    r.TotalExpectedDuration > 0 && r.TotalActualMinutes > r.TotalExpectedDuration
                        ? "Delayed"
                        : completionPercentage >= 75
                            ? "On Track"
                            : completionPercentage >= 50
                                ? "At Risk"
                                : "Delayed";

                return new LearningObjectiveProgressDto
                {
                    Id = r.Id,
                    Name = r.Name ?? "",
                    Value = completionPercentage,
                    Status = status,
                    CurrentPhases = phasesByLoId.GetValueOrDefault(r.Id) ?? new List<CurrentPhaseDto>()
                };
            }).ToList();

            if (!groupId.HasValue && data.Count > 10)
                data = data.Where(x => x.Value > 0).ToList();

            response.Data = new ProjectLearningObjectivesProgressDto { Data = data };
            response.Message = "Project learning objectives progress retrieved successfully.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetProjectLearningObjectivesProgressAsync: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            response.Error = true;
            response.Message = $"An unexpected error occurred: {ex.Message}";
        }

        return response;
    }

    private sealed record ProjectLoProgressRow(
        int Id,
        string? Name,
        int TotalExpectedTasks,
        int TotalExpectedDuration,
        int CompletedTasks,
        double TotalActualMinutes
    );

    private sealed record ProjectLoPhaseRow(
        int LearningObjectiveId,
        string? GroupName,
        string? ColorCode
    );

    public async Task<ResponseService<ProjectLearningObjectivesTableDto>> GetProjectLearningObjectivesTableAsync(int projectId)
    {
        var response = new ResponseService<ProjectLearningObjectivesTableDto>();

        try
        {
            var project = await _context.Projects
                .AsSplitQuery()
                .Include(p => p.Units.Where(u => !u.Archived))
                    .ThenInclude(u => u.Lessons.Where(l => !l.Archived))
                        .ThenInclude(l => l.LearningObjectives.Where(lo => !lo.Archived))
                            .ThenInclude(lo => lo.Tasks.Where(t => !t.Archived))
                                .ThenInclude(t => t.Group)
                .Include(p => p.Units.Where(u => !u.Archived))
                    .ThenInclude(u => u.Lessons.Where(l => !l.Archived))
                        .ThenInclude(l => l.LearningObjectives.Where(lo => !lo.Archived))
                            .ThenInclude(lo => lo.Schema)
                                .ThenInclude(schema => schema.Nodes)
                                    .ThenInclude(node => node.Steps)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                response.Error = true;
                response.Message = "Project not found.";
                return response;
            }

            var allLOs = project.Units
                .Where(u => !u.Archived)
                .SelectMany(u => u.Lessons.Where(l => !l.Archived))
                .SelectMany(l => l.LearningObjectives.Where(lo => lo != null && !lo.Archived && !IsOldLearningObjectiveName(lo.Name)))
                .ToList();

            var tableData = allLOs.Select(lo =>
            {
                var tasks = (lo.Tasks ?? new List<TaskModel>()).Where(t => !t.Archived).ToList();

                var totalExpectedTasks = lo.Schema?.Nodes?
                    .Where(node => !node.Archived)
                    .SelectMany(node => node.Steps ?? new List<Step>())
                    .Count(step => !step.Archived) ?? 0;

                var completedTasks = tasks.Count(t => t.Status == TaskStatusEnum.Done);
                var activeTasks = tasks.Count(t => t.Status == TaskStatusEnum.ToDo || t.Status == TaskStatusEnum.Doing);

                var progress = totalExpectedTasks > 0
                    ? (int)Math.Round((double)completedTasks / totalExpectedTasks * 100)
                    : 0;

                string status;
                if (progress >= 75) status = "On Track";
                else if (progress >= 50) status = "At Risk";
                else status = "Delayed";

                var currentPhases = tasks
                    .Where(t => t.Status == TaskStatusEnum.ToDo || t.Status == TaskStatusEnum.Doing || t.Status == TaskStatusEnum.Backlog)
                    .Where(t => t.Group != null)
                    .Select(t => new CurrentPhaseDto
                    {
                        GroupName = t.Group!.Name,
                        ColorCode = string.IsNullOrEmpty(t.Group.ColorCode) ? "#6b7280" : t.Group.ColorCode
                    })
                    .DistinctBy(p => p.GroupName)
                    .ToList();

                var subject = (lo.Name ?? "") switch
                {
                    var s when s.Contains("mth", StringComparison.OrdinalIgnoreCase) => "math",
                    var s when s.Contains("sci", StringComparison.OrdinalIgnoreCase) => "science",
                    var s when s.Contains("eng", StringComparison.OrdinalIgnoreCase) => "english",
                    var s when s.Contains("ara", StringComparison.OrdinalIgnoreCase) => "arabic",
                    var s when s.Contains("soc", StringComparison.OrdinalIgnoreCase) => "social study",
                    var s when s.Contains("mul", StringComparison.OrdinalIgnoreCase) => "multimedia",
                    var s when s.Contains("rel", StringComparison.OrdinalIgnoreCase) => "religion",
                    _ => "unknown"
                };

                var startDate = lo.StartedAt?.ToString("d/M/yyyy") ?? "";
                var endDate = lo.DoneAt?.ToString("d/M/yyyy") ?? "In Process";

                return new LearningObjectiveTableRowDto
                {
                    Id = lo.Id,
                    Name = lo.Name,
                    Subject = subject,
                    StartDate = startDate,
                    EndDate = endDate,
                    ActiveTasks = activeTasks,
                    CurrentPhases = currentPhases,
                    Status = status,
                    Progress = progress
                };
            }).ToList();

            response.Data = new ProjectLearningObjectivesTableDto
            {
                ProjectName = project.Name,
                Data = tableData
            };

            response.Message = "Project learning objectives table data retrieved successfully.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetProjectLearningObjectivesTableAsync: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            response.Error = true;
            response.Message = $"An unexpected error occurred: {ex.Message}";
        }

        return response;
    }
}

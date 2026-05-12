using System.Linq.Expressions;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.SprintDtos;
using AutomatedTaskSystem.Helper;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.TaskStatus;
using AutomatedTaskSystem.Services.ResponseService;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace AutomatedTaskSystem.Services.Sprint
{
    public class SprintAnalyticsService : ISprintAnalyticsService
    {
        private readonly DataContext _dataContext;

        public SprintAnalyticsService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        private async Task<DbConnection> GetOpenConnectionAsync()
        {
            var connection = _dataContext.Database.GetDbConnection();
            await connection.EnsureOpenAsync();
            return connection;
        }

        /// <summary>
        /// Calculate the date range based on the time period filter
        /// </summary>
        /// <param name="timePeriod">The time period filter</param>
        /// <returns>A tuple containing the start date and end date for filtering</returns>
        private static (DateTime? startDate, DateTime? endDate) GetDateRangeFromTimePeriod(TimePeriodFilter? timePeriod)
        {
            if (timePeriod == null || timePeriod == TimePeriodFilter.AllTime)
            {
                return (null, null); // No date filtering
            }

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

        public async Task<ResponseService<SprintOverviewDto>> GetSprintOverviewAsync(int sprintId, TimePeriodFilter? timePeriod = null)
        {
            var response = new ResponseService<SprintOverviewDto>();

            try
            {
                var (startDate, endDate) = GetDateRangeFromTimePeriod(timePeriod);

                var sprintExists = await _dataContext.Sprints
                    .AsNoTracking()
                    .AnyAsync(s => s.Id == sprintId);

                if (!sprintExists)
                {
                    response.Error = true;
                    response.Message = "Sprint not found.";
                    return response;
                }

                var connection = await GetOpenConnectionAsync();

                const string expectedTasksSql = """
                    SELECT
                        TotalExpectedTasks = COUNT(1)
                    FROM SprintLearningObjectives slo
                    INNER JOIN LearningObjectives lo ON lo.Id = slo.LearningObjectiveId AND lo.Archived = 0
                    INNER JOIN Nodes n ON n.SchemaId = lo.SchemaId AND n.Archived = 0
                    INNER JOIN Steps st ON st.NodeId = n.Id AND st.Archived = 0
                    WHERE slo.SprintId = @sprintId
                      AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%';
                    """;

                var totalExpectedTasks = await connection.QuerySingleAsync<int>(
                    expectedTasksSql,
                    new { sprintId }
                );

                const string taskSummarySql = """
                        SELECT
                            Completed = SUM(CASE WHEN t.Status = @done THEN 1 ELSE 0 END),
                            Active = SUM(CASE WHEN t.Status IN (@todo, @doing) THEN 1 ELSE 0 END),
                            RollbackCount = SUM(CASE WHEN t.IsRollback = 1 THEN 1 ELSE 0 END),
                            Flagged = SUM(CASE WHEN t.Flagged = 1 THEN 1 ELSE 0 END)
                        FROM Tasks t
                        INNER JOIN LearningObjectives lo ON lo.Id = t.LearningObjectiveId AND lo.Archived = 0
                        INNER JOIN SprintLearningObjectives slo ON slo.LearningObjectiveId = lo.Id AND slo.SprintId = @sprintId
                        WHERE t.Archived = 0
                          AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%';
                        """;

                var taskSummaryRow = await connection.QuerySingleAsync<SprintTaskSummaryRow>(
                    taskSummarySql,
                    new
                    {
                        sprintId,
                        done = (int)TaskStatusEnum.Done,
                        todo = (int)TaskStatusEnum.ToDo,
                        doing = (int)TaskStatusEnum.Doing
                    }
                );

                var completedCount = taskSummaryRow.Completed;
                var activeCount = taskSummaryRow.Active;
                var notStartedCount = totalExpectedTasks - completedCount - activeCount;

                // Calculate task summary (UNFILTERED - always shows complete task status distribution)
                // Time period filter does NOT affect task summary pie charts
                var taskSummary = new TaskSummaryDto
                {
                    Active = activeCount,
                    Completed = completedCount,
                    Rollback = taskSummaryRow.RollbackCount,
                    Flagged = taskSummaryRow.Flagged,
                    NotStarted = Math.Max(0, notStartedCount),
                    Total = totalExpectedTasks
                };

                // Calculate tag distribution (FILTERED by time period)
                // Only count tasks with status Backlog, ToDo, or Doing (exclude Done and Rollback)
                List<string> preferredGroupSteps = new List<string>
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
                    INNER JOIN LearningObjectives lo ON lo.Id = t.LearningObjectiveId AND lo.Archived = 0
                    INNER JOIN SprintLearningObjectives slo ON slo.LearningObjectiveId = lo.Id AND slo.SprintId = @sprintId
                    INNER JOIN Groups g ON g.Id = t.GroupId
                    WHERE t.Archived = 0
                      AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%'
                      AND t.Status IN (@backlog, @todo, @doing)
                      AND (@startDate IS NULL OR (t.CreatedAt >= @startDate AND t.CreatedAt <= @endDate))
                    GROUP BY t.GroupId, g.Name, g.ColorCode;
                    """;

                var tagRows = (await connection.QueryAsync<SprintTagRow>(
                        tagsSql,
                        new
                        {
                            sprintId,
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
                    
                // Calculate learning objectives summary with mutually exclusive categorization
                // All LOs must be categorized: Completed + NotStarted + InProcess = Total
                // NOTE: LO Summary is UNFILTERED - always shows complete LO status distribution regardless of time period
                // (Similar to Task Summary - time period filter does NOT affect this chart)

                const string loSummarySql = """
                WITH SprintLOs AS (
                    SELECT lo.Id, lo.SchemaId
                    FROM SprintLearningObjectives slo
                    INNER JOIN LearningObjectives lo ON lo.Id = slo.LearningObjectiveId
                    WHERE slo.SprintId = @sprintId
                      AND lo.Archived = 0
                      AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%'
                ),
                ExpectedSteps AS (
                    SELECT sl.Id AS LearningObjectiveId, COUNT(1) AS ExpectedCount
                    FROM SprintLOs sl
                    INNER JOIN Nodes n ON n.SchemaId = sl.SchemaId AND n.Archived = 0
                    INNER JOIN Steps s ON s.NodeId = n.Id AND s.Archived = 0
                    GROUP BY sl.Id
                ),
                TaskAgg AS (
                    SELECT t.LearningObjectiveId,
                           TotalTasks = COUNT(1),
                           NonBacklogTasks = SUM(CASE WHEN t.Status <> @backlog THEN 1 ELSE 0 END),
                           NotDoneTasks = SUM(CASE WHEN t.Status <> @done THEN 1 ELSE 0 END)
                    FROM Tasks t
                    INNER JOIN SprintLOs sl ON sl.Id = t.LearningObjectiveId
                    WHERE t.Archived = 0
                    GROUP BY t.LearningObjectiveId
                )
                SELECT
                    Total = (SELECT COUNT(1) FROM SprintLOs),
                    Completed = SUM(CASE WHEN ISNULL(ta.TotalTasks, 0) > 0 AND ISNULL(ta.NotDoneTasks, 0) = 0 THEN 1 ELSE 0 END),
                    NotStarted = SUM(CASE
                        WHEN ISNULL(ta.TotalTasks, 0) = 0 AND ISNULL(es.ExpectedCount, 0) > 0 THEN 1
                        WHEN ISNULL(ta.TotalTasks, 0) > 0 AND ISNULL(ta.NonBacklogTasks, 0) = 0 THEN 1
                        ELSE 0
                    END)
                FROM SprintLOs sl
                LEFT JOIN ExpectedSteps es ON es.LearningObjectiveId = sl.Id
                LEFT JOIN TaskAgg ta ON ta.LearningObjectiveId = sl.Id;
                """;

                var loSummaryRow = await connection.QuerySingleAsync<SprintLoSummaryRow>(
                    loSummarySql,
                    new
                    {
                        sprintId,
                        backlog = (int)TaskStatusEnum.Backlog,
                        done = (int)TaskStatusEnum.Done
                    }
                );

                var totalLOs = loSummaryRow.Total;
                var completedLOs = loSummaryRow.Completed;
                var notStartedLOs = loSummaryRow.NotStarted;

                // In Process: Everything else (LOs with mixed statuses, or any ToDo/Doing tasks)
                // This ensures completedLOs + notStartedLOs + inProcessLOs = totalLOs
                var inProcessLOs = totalLOs - completedLOs - notStartedLOs;

                var loSummary = new LearningObjectiveSummaryDto
                {
                    Completed = completedLOs,
                    NotStarted = notStartedLOs,
                    InProcess = inProcessLOs,
                    Total = totalLOs
                };

                response.Data = new SprintOverviewDto
                {
                    Tags = tagData,
                    TaskSummary = taskSummary,
                    LoSummary = loSummary,
                    SprintSummary = taskSummary // Same as task summary for now
                };

                response.Message = "Sprint overview retrieved successfully.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSprintOverviewAsync: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                response.Error = true;
                response.Message = $"An unexpected error occurred: {ex.Message}";
            }

            return response;
        }

        public async Task<ResponseService<SprintLearningObjectivesProgressDto>> GetSprintLearningObjectivesProgressAsync(int sprintId, TimePeriodFilter? timePeriod = null,int? groupId = null)
        {
            var response = new ResponseService<SprintLearningObjectivesProgressDto>();

            try
            {
                // Calculate date range based on time period for filtering tasks
                var (startDate, endDate) = GetDateRangeFromTimePeriod(timePeriod);

                // Verify sprint exists and include Schema -> Nodes -> Steps -> TaskBank for calculating expected tasks and duration
                var query = _dataContext.Sprints
                        .Include(s => s.SprintLearningObjectives
                            .Where(slo => slo.LearningObjective != null && !slo.LearningObjective.Archived))
                            .ThenInclude(slo => slo.LearningObjective)
                                .ThenInclude(lo => lo.Tasks.Where(t => !t.Archived))
                                    .ThenInclude(t => t.Group)
                        .Include(s => s.SprintLearningObjectives)
                            .ThenInclude(slo => slo.LearningObjective)
                                .ThenInclude(lo => lo.Schema)
                                    .ThenInclude(schema => schema.Nodes)
                                        .ThenInclude(node => node.Steps)
                                            .ThenInclude(step => step.TaskBank)
                        .AsQueryable();

                var sprint = await query.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == sprintId);

                if (sprint == null)
                {
                    response.Error = true;
                    response.Message = "Sprint not found.";
                    return response;
                }

                // Helper function to filter tasks by groupId
                Func<IEnumerable<Models.Task>, IEnumerable<Models.Task>> filterTasksByGroup = (tasks) =>
                {
                    if (groupId.HasValue)
                    {
                        return tasks.Where(t => t.GroupId == groupId.Value);
                    }
                    return tasks;
                };

                // Get all task IDs belonging to LOs in this sprint for TaskWorkTime calculation
                // Apply groupId filter to task IDs as well
                var allTaskIds = sprint.SprintLearningObjectives
                    .Where(slo => slo.LearningObjective != null)
                    .SelectMany(slo => slo.LearningObjective!.Tasks ?? new List<Models.Task>())
                    .Where(t => !t.Archived)
                    .Where(t => !groupId.HasValue || t.GroupId == groupId.Value)
                    .Select(t => t.Id)
                    .ToList();

                // Fetch TaskWorkTimes for all tasks and group by TaskId
                var taskWorkTimes = await _dataContext.TaskWorkTimes
                    .Where(twt => allTaskIds.Contains(twt.TaskId))
                    .ToListAsync();

                // Create a dictionary of task ID -> total actual duration in minutes from TaskWorkTime records (stored in milliseconds)
                var taskActualDurations = taskWorkTimes
                    .GroupBy(twt => twt.TaskId)
                    .ToDictionary(g => g.Key, g => g.Sum(twt => TimeSpan.FromMilliseconds(twt.Duration).TotalMinutes));

                // Helper function to filter tasks by date range
                Func<IEnumerable<Models.Task>, IEnumerable<Models.Task>> filterTasksByDate = (tasks) =>
                {
                    var filtered = tasks.Where(t => !t.Archived); 
                    if (startDate.HasValue && endDate.HasValue)
                    {
                        filtered = filtered.Where(t => t.CreatedAt >= startDate.Value && t.CreatedAt <= endDate.Value);
                    }
                    return filtered;
                };

                // Filter learning objectives: only include LOs that have at least one task matching the groupId filter
                var filteredSprintLOs = sprint.SprintLearningObjectives
                    .Where(slo => slo.LearningObjective != null && !slo.LearningObjective.Archived)
                    .Where(slo => !IsOldLearningObjectiveName(slo.LearningObjective!.Name))
                    .Where(slo => !groupId.HasValue ||
                        (slo.LearningObjective!.Tasks != null &&
                         slo.LearningObjective.Tasks.Any(t => !t.Archived && t.GroupId == groupId.Value)))
                    .ToList();

                // Calculate progress for each learning objective
                var progressQuery = filteredSprintLOs
                    .Select(slo =>
                    {
                        var lo = slo.LearningObjective!;
                        // Get all non-archived tasks, then filter by groupId if specified
                        var allTasks = lo.Tasks?.Where(t => !t.Archived).ToList() ?? new List<Models.Task>();
                        var groupFilteredTasks = filterTasksByGroup(allTasks).ToList();

                        // Apply time period filtering to the group-filtered tasks
                        var filteredTasks = filterTasksByDate(groupFilteredTasks).ToList();

                        // Get all steps from schema (excluding archived)
                        var steps = lo.Schema?.Nodes?
                            .Where(node => !node.Archived)
                            .SelectMany(node => node.Steps ?? new List<Models.Step>())
                            .Where(step => !step.Archived)
                            .ToList() ?? new List<Models.Step>();

                        // Calculate total expected tasks from schema steps count
                        var totalExpectedTasks = steps.Count;

                        // Calculate total expected duration from TaskBank.Duration for all schema steps
                        var totalExpectedDuration = steps
                            .Where(step => step.TaskBank != null)
                            .Sum(step => step.TaskBank.Duration);

                        // Calculate total actual time spent from TaskWorkTime.Duration for all tasks in this LO
                        var totalActualTimeSpent = filteredTasks
                            .Where(t => taskActualDurations.ContainsKey(t.Id))
                            .Sum(t => taskActualDurations[t.Id]);

                        // Count completed tasks (status = Done) from filtered tasks
                        var completedTasks = filteredTasks.Count(t => t.Status == TaskStatusEnum.Done);

                        // Calculate completion percentage based on expected tasks from schema
                        var completionPercentage = totalExpectedTasks > 0
                            ? Math.Min(100, (int)Math.Round((double)completedTasks / totalExpectedTasks * 100))
                            : 0;

                        // Determine status based on time-based delay detection first, then completion percentage
                        string status;

                        // Time-based delay detection: if total logged time exceeds expected duration, mark as Delayed
                        if (totalExpectedDuration > 0 && totalActualTimeSpent > totalExpectedDuration)
                        {
                            status = "Delayed";
                        }
                        // Otherwise, use completion percentage-based status
                        else if (completionPercentage >= 75)
                        {
                            status = "On Track";
                        }
                        else if (completionPercentage >= 50)
                        {
                            status = "At Risk";
                        }
                        else
                        {
                            status = "Delayed";
                        }

                        // Get current phases (unique groups of active tasks)
                        // Use groupFilteredTasks to be consistent with the group filtering
                        var currentPhases = groupFilteredTasks
                            .Where(t => t.Status == TaskStatusEnum.Backlog || t.Status == TaskStatusEnum.ToDo || t.Status == TaskStatusEnum.Doing)
                            .Where(t => t.Group != null)
                            .Select(t => new CurrentPhaseDto
                            {
                                GroupName = t.Group!.Name,
                                ColorCode = string.IsNullOrEmpty(t.Group.ColorCode) ? "#6b7280" : t.Group.ColorCode
                            })
                            .DistinctBy(p => p.GroupName)
                            .ToList();

                        return new LearningObjectiveProgressDto
                        {
                            Id  = lo.Id,
                            Name = lo.Name,
                            Value = completionPercentage,
                            Status = status,
                            CurrentPhases = currentPhases
                        };
                    });

                // When no specific group is selected and there are many LOs, hide 0%-progress items.
                // When a group filter is applied, always return all matching LOs (including 0%),
                // so that clicking on a tag never results in an unexpected empty list.
                if (!groupId.HasValue && filteredSprintLOs.Count > 10)
                {
                    progressQuery = progressQuery.Where(lo => lo.Value > 0);
                }

                var progressData = progressQuery.ToList();

                response.Data = new SprintLearningObjectivesProgressDto
                {
                    Data = progressData
                };

                response.Message = "Learning objectives progress retrieved successfully.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSprintLearningObjectivesProgressAsync: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                response.Error = true;
                response.Message = $"An unexpected error occurred: {ex.Message}";
            }

            return response;
        }

        public async Task<ResponseService<SprintLearningObjectivesTableDto>> GetSprintLearningObjectivesTableAsync(int sprintId)
        {
            var response = new ResponseService<SprintLearningObjectivesTableDto>();

            try
            {
                var connection = await GetOpenConnectionAsync();

                const string sprintNameSql = """
                    SELECT Name
                    FROM Sprints
                    WHERE Id = @sprintId;
                    """;

                var sprintName = await connection.QuerySingleOrDefaultAsync<string>(
                    sprintNameSql,
                    new { sprintId }
                );

                if (string.IsNullOrEmpty(sprintName))
                {
                    response.Error = true;
                    response.Message = "Sprint not found.";
                    return response;
                }

                const string tableRowsSql = """
            WITH SprintLOs AS (
                SELECT lo.Id, lo.Name, lo.StartedAt, lo.DoneAt, lo.SchemaId
                FROM SprintLearningObjectives slo
                INNER JOIN LearningObjectives lo ON lo.Id = slo.LearningObjectiveId
                WHERE slo.SprintId = @sprintId
                  AND lo.Archived = 0
                  AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%'
            ),
            ExpectedSteps AS (
                SELECT sl.Id AS LearningObjectiveId, COUNT(1) AS TotalExpectedTasks
                FROM SprintLOs sl
                INNER JOIN Nodes n ON n.SchemaId = sl.SchemaId AND n.Archived = 0
                INNER JOIN Steps s ON s.NodeId = n.Id AND s.Archived = 0
                GROUP BY sl.Id
            ),
            TaskAgg AS (
                SELECT t.LearningObjectiveId,
                       CompletedTasks = SUM(CASE WHEN t.Status = @done THEN 1 ELSE 0 END),
                       ActiveTasks = SUM(CASE WHEN t.Status IN (@todo, @doing) THEN 1 ELSE 0 END)
                FROM Tasks t
                INNER JOIN SprintLOs sl ON sl.Id = t.LearningObjectiveId
                WHERE t.Archived = 0
                GROUP BY t.LearningObjectiveId
            )
            SELECT
                sl.Id,
                sl.Name,
                sl.StartedAt,
                sl.DoneAt,
                TotalExpectedTasks = ISNULL(es.TotalExpectedTasks, 0),
                CompletedTasks = ISNULL(ta.CompletedTasks, 0),
                ActiveTasks = ISNULL(ta.ActiveTasks, 0)
            FROM SprintLOs sl
            LEFT JOIN ExpectedSteps es ON es.LearningObjectiveId = sl.Id
            LEFT JOIN TaskAgg ta ON ta.LearningObjectiveId = sl.Id
            ORDER BY sl.Id;
            """;

                var loRows = (await connection.QueryAsync<SprintTableLoRow>(
                    tableRowsSql,
                    new
                    {
                        sprintId,
                        done = (int)TaskStatusEnum.Done,
                        todo = (int)TaskStatusEnum.ToDo,
                        doing = (int)TaskStatusEnum.Doing
                    }
                )).ToList();

                const string phasesSql = """
                    SELECT
                        lo.Id AS LearningObjectiveId,
                        g.Name AS GroupName,
                        g.ColorCode AS ColorCode
                    FROM SprintLearningObjectives slo
                    INNER JOIN LearningObjectives lo ON lo.Id = slo.LearningObjectiveId AND lo.Archived = 0
                    INNER JOIN Tasks t ON t.LearningObjectiveId = lo.Id AND t.Archived = 0
                    INNER JOIN Groups g ON g.Id = t.GroupId
                    WHERE slo.SprintId = @sprintId
                      AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%'
                      AND t.Status IN (@backlog, @todo, @doing);
                    """;

                var phaseRows = (await connection.QueryAsync<SprintTablePhaseRow>(
                    phasesSql,
                    new
                    {
                        sprintId,
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

                var tableData = loRows.Select(r =>
                {
                    var progress = r.TotalExpectedTasks > 0
                        ? (int)Math.Round((double)r.CompletedTasks / r.TotalExpectedTasks * 100)
                        : 0;

                    var status = progress >= 75 ? "On Track"
                        : progress >= 50 ? "At Risk"
                        : "Delayed";

                    var subject = (r.Name ?? "") switch
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

                    return new LearningObjectiveTableRowDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Subject = subject,
                        StartDate = r.StartedAt?.ToString("d/M/yyyy") ?? "",
                        EndDate = r.DoneAt?.ToString("d/M/yyyy") ?? "In Process",
                        ActiveTasks = r.ActiveTasks,
                        CurrentPhases = phasesByLoId.GetValueOrDefault(r.Id) ?? new List<CurrentPhaseDto>(),
                        Status = status,
                        Progress = progress
                    };
                }).ToList();

                response.Data = new SprintLearningObjectivesTableDto
                {
                    SprintName = sprintName,
                    Data = tableData
                };

                response.Message = "Learning objectives table data retrieved successfully.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSprintLearningObjectivesTableAsync: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                response.Error = true;
                response.Message = $"An unexpected error occurred: {ex.Message}";
            }

            return response;
        }

        private sealed record SprintTaskSummaryRow(
            int Completed,
            int Active,
            int RollbackCount,
            int Flagged
        );

        private sealed record SprintTagRow(
            int GroupId,
            string? Label,
            string? ColorCode,
            int Value
        );

        private sealed record SprintLoSummaryRow(
            int Total,
            int Completed,
            int NotStarted
        );

        private sealed record SprintTableLoRow(
            int Id,
            string Name,
            DateTime? StartedAt,
            DateTime? DoneAt,
            int TotalExpectedTasks,
            int CompletedTasks,
            int ActiveTasks
        );

        private sealed record SprintTablePhaseRow(
            int LearningObjectiveId,
            string? GroupName,
            string? ColorCode
        );
    }
}


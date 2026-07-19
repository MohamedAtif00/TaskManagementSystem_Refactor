using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.DailyReport;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.TaskActivityType;
using AutomatedTaskSystem.Models.Enums.TaskPriority;
using AutomatedTaskSystem.Models.Enums.TaskStatus;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Services.AuthService;
using AutomatedTaskSystem.Services.ResponseService;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AutomatedTaskSystem.Services.DailyReport;

public class DailyReportService : IDailyReportService
{
    private const int DefaultPageSize = 5;
    private const int MaxPageSize = 500;
    private const int DefaultLookbackDays = 7;

    private static readonly string[] StaticSemesters = ["Term 1", "Term 2"];
    private static readonly string[] StaticPriorities = ["High", "Medium", "Low"];
    private static readonly string[] StaticGrades =
        ["Kg1", "Kg2", "Grade 1", "Grade 2", "Grade 3", "Grade 4", "Grade 5", "Grade 6", "Grade 7"];
    private static readonly string[] StaticSubjects =
        ["Arabic", "English", "Math (A)", "Science (A)", "Social", "ICT (A)", "MUL (A)", "Religion", "Other"];

    private readonly DataContext _context;
    private readonly IAuthService _authService;
    private readonly string _connectionString;

    public DailyReportService(DataContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
        _connectionString = context.Database.GetConnectionString()
            ?? throw new InvalidOperationException("Database connection string is not configured.");
    }

    public async Task<ActionResult<ResponseService<GetDailyReportDashboardDto>>> GetDashboardAsync(
        DailyReportFilterDto filter)
    {
        var user = await _authService.GetAuthedUser();
        if (user is null)
            return Unauthorized<GetDailyReportDashboardDto>("Invalid Auth");

        var normalized = NormalizeFilter(filter);
        var page = normalized.Page;
        var pageSize = normalized.PageSize;

        var rowsTask = QueryPagedRowsTwoPhaseAsync(normalized, user, page, pageSize);
        var summaryTask = QuerySummaryBatchAsync(normalized, user);
        var chartsTask = QueryProblemTypesDirectAsync(normalized, user);
        var lookupsTask = QueryLightLookupsAsync(normalized, user);

        await System.Threading.Tasks.Task.WhenAll(rowsTask, summaryTask, chartsTask, lookupsTask);

        var (rows, totalCount) = await rowsTask;
        var summary = await summaryTask;
        var problemTypes = await chartsTask;
        var lookups = await lookupsTask;

        return new ResponseService<GetDailyReportDashboardDto>
        {
            Error = false,
            Message = "Daily report dashboard",
            Data = new GetDailyReportDashboardDto
            {
                Rows = new GetDailyReportPagedDto
                {
                    Rows = rows,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                },
                Summary = summary,
                Charts = new GetDailyReportChartDto
                {
                    Approved = summary.Approved,
                    Hold = summary.Hold,
                    Rollback = summary.Rollback,
                    ProblemTypes = problemTypes
                },
                Lookups = lookups
            }
        };
    }

    public async Task<ActionResult<ResponseService<GetDailyReportPagedDto>>> GetRowsAsync(DailyReportFilterDto filter)
    {
        var user = await _authService.GetAuthedUser();
        if (user is null)
            return Unauthorized<GetDailyReportPagedDto>("Invalid Auth");

        var normalized = NormalizeFilter(filter);
        var (rows, totalCount) = await QueryPagedRowsTwoPhaseAsync(
            normalized, user, normalized.Page, normalized.PageSize);

        return new ResponseService<GetDailyReportPagedDto>
        {
            Error = false,
            Message = "Daily report rows",
            Data = new GetDailyReportPagedDto
            {
                Rows = rows,
                TotalCount = totalCount,
                Page = normalized.Page,
                PageSize = normalized.PageSize
            }
        };
    }

    public async Task<ActionResult<ResponseService<GetDailyReportSummaryDto>>> GetSummaryAsync(DailyReportFilterDto filter)
    {
        var user = await _authService.GetAuthedUser();
        if (user is null)
            return Unauthorized<GetDailyReportSummaryDto>("Invalid Auth");

        var summary = await QuerySummaryBatchAsync(NormalizeFilter(filter), user);
        return new ResponseService<GetDailyReportSummaryDto>
        {
            Error = false,
            Message = "Daily report summary",
            Data = summary
        };
    }

    public async Task<ActionResult<ResponseService<GetDailyReportChartDto>>> GetChartsAsync(DailyReportFilterDto filter)
    {
        var user = await _authService.GetAuthedUser();
        if (user is null)
            return Unauthorized<GetDailyReportChartDto>("Invalid Auth");

        var normalized = NormalizeFilter(filter);
        var summary = await QuerySummaryBatchAsync(normalized, user);
        var problemTypes = await QueryProblemTypesDirectAsync(normalized, user);

        return new ResponseService<GetDailyReportChartDto>
        {
            Error = false,
            Message = "Daily report charts",
            Data = new GetDailyReportChartDto
            {
                Approved = summary.Approved,
                Hold = summary.Hold,
                Rollback = summary.Rollback,
                ProblemTypes = problemTypes
            }
        };
    }

    public async Task<ActionResult<ResponseService<GetDailyReportLookupsDto>>> GetLookupsAsync()
    {
        var user = await _authService.GetAuthedUser();
        if (user is null)
            return Unauthorized<GetDailyReportLookupsDto>("Invalid Auth");

        var filter = new DailyReportFilterDto
        {
            From = DateTime.Today.AddDays(-DefaultLookbackDays),
            To = DateTime.Today
        };
        var lookups = await QueryLightLookupsAsync(filter, user);

        return new ResponseService<GetDailyReportLookupsDto>
        {
            Error = false,
            Message = "Daily report lookups",
            Data = lookups
        };
    }

    public async Task<ActionResult<ResponseService<string>>> UpdateNotesAsync(int taskId, UpdateDailyReportNotesDto dto)
    {
        var user = await _authService.GetAuthedUser();
        if (user is null)
            return Unauthorized<string>("Invalid Auth");

        var taskExists = await _context.Tasks.AnyAsync(t => t.Id == taskId && !t.Archived);
        if (!taskExists)
            return NotFound<string>("Task not found");

        var existing = await _context.DailyReportNoteOverrides.FirstOrDefaultAsync(o => o.TaskId == taskId);
        if (existing is null)
        {
            _context.DailyReportNoteOverrides.Add(new DailyReportNoteOverride
            {
                TaskId = taskId,
                Notes = dto.Notes ?? "",
                UpdatedByUserId = user.Id,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.Notes = dto.Notes ?? "";
            existing.UpdatedByUserId = user.Id;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return new ResponseService<string>
        {
            Error = false,
            Message = "Notes updated",
            Data = dto.Notes ?? ""
        };
    }

    /// <summary>
    /// Phase A: light CTE count + TaskIds. Phase B: enrich only those TaskIds with rollback/notes.
    /// </summary>
    private async Task<(List<GetDailyReportRowDto> Rows, int TotalCount)> QueryPagedRowsTwoPhaseAsync(
        DailyReportFilterDto filter,
        User user,
        int page,
        int pageSize)
    {
        var (cteSql, filteredSql, parameters) = BuildQueryParts(filter, user);
        parameters.Add("offset", (page - 1) * pageSize);
        parameters.Add("pageSize", pageSize);

        var phaseASql = $"""
            {cteSql},
            Filtered AS ({filteredSql})
            {DailyReportSql.CountAndPagedTaskIds}
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        int totalCount;
        List<int> taskIds;
        await using (var multi = await connection.QueryMultipleAsync(phaseASql, parameters, commandTimeout: 60))
        {
            totalCount = await multi.ReadSingleAsync<int>();
            taskIds = (await multi.ReadAsync<int>()).ToList();
        }

        if (taskIds.Count == 0)
            return ([], totalCount);

        var enrichParams = BuildParameters(filter, user);
        enrichParams.Add("taskIds", taskIds);

        var enriched = (await connection.QueryAsync<GetDailyReportRowDto>(
            DailyReportSql.EnrichRowsSql, enrichParams, commandTimeout: 60)).ToList();

        var byId = enriched.ToDictionary(r => r.TaskId);
        var ordered = taskIds
            .Where(byId.ContainsKey)
            .Select(id => byId[id])
            .ToList();

        return (ordered, totalCount);
    }

    private async Task<GetDailyReportSummaryDto> QuerySummaryBatchAsync(DailyReportFilterDto filter, User user)
    {
        var (cteSql, filteredSql, parameters) = BuildQueryParts(filter, user);
        var sql = $"""
            {cteSql},
            Filtered AS ({filteredSql})
            {DailyReportSql.SummaryAndTopTeams}
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var multi = await connection.QueryMultipleAsync(sql, parameters, commandTimeout: 60);
        var summaryRow = await multi.ReadSingleAsync<SummaryRow>();
        var topTeams = (await multi.ReadAsync<TeamCountRow>()).ToList();

        return new GetDailyReportSummaryDto
        {
            Total = summaryRow.Total,
            Approved = summaryRow.Approved,
            Hold = summaryRow.Hold,
            Rollback = summaryRow.Rollback,
            ActiveTeams = summaryRow.ActiveTeams,
            TopTeams = topTeams
                .Select(t => new DailyReportTeamCountDto { Team = t.Team, Count = t.Count })
                .ToList()
        };
    }

    private async Task<List<DailyReportProblemCountDto>> QueryProblemTypesDirectAsync(
        DailyReportFilterDto filter,
        User user)
    {
        var parameters = BuildParameters(filter, user);
        AddDateParameters(filter, parameters);
        var roleFilter = BuildRoleFilterSql(user, parameters);
        var sql = DailyReportSql.ProblemTypesDirect.Replace("{ROLE_FILTER}", roleFilter);

        var rows = await RunQueryAsync<ProblemCountRow>(sql, parameters);
        return rows
            .Select(p => new DailyReportProblemCountDto { ProblemType = p.ProblemType, Count = p.Count })
            .ToList();
    }

    private async Task<GetDailyReportLookupsDto> QueryLightLookupsAsync(DailyReportFilterDto filter, User user)
    {
        var parameters = BuildParameters(filter, user);
        AddDateParameters(filter, parameters);
        var roleFilter = BuildRoleFilterSql(user, parameters);

        var teamsSql = DailyReportSql.LightLookupsTeams.Replace("{ROLE_FILTER}", roleFilter);
        var tasksSql = DailyReportSql.LightLookupsTaskNames.Replace("{ROLE_FILTER}", roleFilter);

        var batchSql = teamsSql + ";\n" + tasksSql;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var multi = await connection.QueryMultipleAsync(batchSql, parameters, commandTimeout: 60);
        var teams = (await multi.ReadAsync<LookupValueRow>()).ToList();
        var taskNames = (await multi.ReadAsync<LookupValueRow>()).ToList();

        return new GetDailyReportLookupsDto
        {
            Teams = teams.Select(r => r.Value).ToList(),
            Semesters = StaticSemesters.ToList(),
            Subjects = StaticSubjects.ToList(),
            Grades = StaticGrades.ToList(),
            TaskNames = taskNames.Select(r => r.Value).ToList(),
            ProblemTypes = [],
            Priorities = StaticPriorities.ToList()
        };
    }

    private (string BaseCte, string FilteredSql, DynamicParameters Parameters) BuildQueryParts(
        DailyReportFilterDto filter,
        User user)
    {
        var parameters = BuildParameters(filter, user);
        AddDateParameters(filter, parameters);
        var roleFilter = BuildRoleFilterSql(user, parameters);
        var dateFilterBase = """
            AND COALESCE(ad.ReportDate, t.CreatedAt) >= @fromDate
            AND COALESCE(ad.ReportDate, t.CreatedAt) < DATEADD(DAY, 1, @toDate)
            """;

        var baseCte = DailyReportSql.LightBaseCte
            .Replace("{ROLE_FILTER}", roleFilter)
            .Replace("{DATE_FILTER_BASE}", dateFilterBase);

        var problemFilter = string.IsNullOrWhiteSpace(filter.ProblemType)
            ? ""
            : DailyReportSql.ProblemTypeExistsFilter;

        var filteredRows = DailyReportSql.FilteredRows
            .Replace("{TEAM_FILTER}", string.IsNullOrWhiteSpace(filter.Team) ? "" : "AND Team = @team")
            .Replace("{SEMESTER_FILTER}", string.IsNullOrWhiteSpace(filter.Semester) ? "" : "AND Semester = @semester")
            .Replace("{SUBJECT_FILTER}", string.IsNullOrWhiteSpace(filter.Subject) ? "" : "AND Subjects = @subject")
            .Replace("{GRADE_FILTER}", string.IsNullOrWhiteSpace(filter.Grade) ? "" : "AND Grade = @grade")
            .Replace("{TASK_FILTER}", string.IsNullOrWhiteSpace(filter.TaskName) ? "" : "AND TaskName = @taskName")
            .Replace("{STATUS_FILTER}", string.IsNullOrWhiteSpace(filter.Status) ? "" : "AND [Status] = @status")
            .Replace("{PROBLEM_FILTER}", problemFilter)
            .Replace("{PRIORITY_FILTER}", string.IsNullOrWhiteSpace(filter.Priority) ? "" : "AND [Priority] = @priority");

        return (baseCte, filteredRows, parameters);
    }

    private static DailyReportFilterDto NormalizeFilter(DailyReportFilterDto filter) =>
        new()
        {
            From = filter.From,
            To = filter.To,
            Team = filter.Team,
            Semester = filter.Semester,
            Subject = filter.Subject,
            Grade = filter.Grade,
            TaskName = filter.TaskName,
            Status = filter.Status,
            ProblemType = filter.ProblemType,
            Priority = filter.Priority,
            Page = Math.Max(1, filter.Page),
            PageSize = Math.Clamp(filter.PageSize <= 0 ? DefaultPageSize : filter.PageSize, 1, MaxPageSize)
        };

    private static void AddDateParameters(DailyReportFilterDto filter, DynamicParameters parameters)
    {
        parameters.Add("fromDate", filter.From?.Date ?? DateTime.Today.AddDays(-DefaultLookbackDays));
        parameters.Add("toDate", filter.To?.Date ?? DateTime.Today);
    }

    private static string BuildRoleFilterSql(User user, DynamicParameters parameters)
    {
        switch (user.Role)
        {
            case UserRoleEnum.ProjectManger:
            case UserRoleEnum.Owner:
                return "";
            case UserRoleEnum.SectionHead:
                parameters.Add("sectionHeadId", user.Id);
                return """
                    AND t.GroupId IN (
                        SELECT sg.GroupId FROM SectionGroups sg
                        INNER JOIN Sections sec ON sec.Id = sg.SectionId
                        WHERE sec.HeadId = @sectionHeadId AND sec.Archived = 0
                    )
                    """;
            case UserRoleEnum.TeamLeader:
                parameters.Add("leaderGroupId", user.GroupId);
                return "AND t.GroupId = @leaderGroupId";
            case UserRoleEnum.Member:
                parameters.Add("memberUserId", user.Id);
                parameters.Add("memberGroupId", user.GroupId);
                return "AND (t.UserId = @memberUserId OR t.GroupId = @memberGroupId)";
            default:
                return "AND 1 = 0";
        }
    }

    private static DynamicParameters BuildParameters(DailyReportFilterDto filter, User user)
    {
        var parameters = new DynamicParameters();
        parameters.Add("statusDone", (int)TaskActivityTypeEnum.Status_Done);
        parameters.Add("statusRollback", (int)TaskActivityTypeEnum.Status_Rollback);
        parameters.Add("rollbackActivity", (int)TaskActivityTypeEnum.Rollback);
        parameters.Add("taskDone", (int)TaskStatusEnum.Done);
        parameters.Add("taskRollback", (int)TaskStatusEnum.Rollback);
        parameters.Add("priorityHigh", (int)TaskPriorityEnum.High);
        parameters.Add("priorityMedium", (int)TaskPriorityEnum.Medium);
        parameters.Add("priorityLow", (int)TaskPriorityEnum.Low);

        if (!string.IsNullOrWhiteSpace(filter.Team)) parameters.Add("team", filter.Team);
        if (!string.IsNullOrWhiteSpace(filter.Semester)) parameters.Add("semester", filter.Semester);
        if (!string.IsNullOrWhiteSpace(filter.Subject)) parameters.Add("subject", filter.Subject);
        if (!string.IsNullOrWhiteSpace(filter.Grade)) parameters.Add("grade", filter.Grade);
        if (!string.IsNullOrWhiteSpace(filter.TaskName)) parameters.Add("taskName", filter.TaskName);
        if (!string.IsNullOrWhiteSpace(filter.Status)) parameters.Add("status", filter.Status);
        if (!string.IsNullOrWhiteSpace(filter.ProblemType)) parameters.Add("problemType", filter.ProblemType);
        if (!string.IsNullOrWhiteSpace(filter.Priority)) parameters.Add("priority", filter.Priority);

        return parameters;
    }

    private async Task<List<T>> RunQueryAsync<T>(string sql, DynamicParameters parameters)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var result = await connection.QueryAsync<T>(sql, parameters, commandTimeout: 60);
        return result.ToList();
    }

    private static ActionResult<ResponseService<T>> Unauthorized<T>(string message) =>
        new UnauthorizedObjectResult(new BaseResponseService { Error = true, Message = message });

    private static ActionResult<ResponseService<T>> NotFound<T>(string message) =>
        new NotFoundObjectResult(new BaseResponseService { Error = true, Message = message });

    private sealed class SummaryRow
    {
        public int Total { get; set; }
        public int Approved { get; set; }
        public int Hold { get; set; }
        public int Rollback { get; set; }
        public int ActiveTeams { get; set; }
    }

    private sealed class TeamCountRow
    {
        public string Team { get; set; } = "";
        public int Count { get; set; }
    }

    private sealed class ProblemCountRow
    {
        public string ProblemType { get; set; } = "";
        public int Count { get; set; }
    }

    private sealed class LookupValueRow
    {
        public string Value { get; set; } = "";
    }
}

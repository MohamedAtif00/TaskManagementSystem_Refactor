using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.TaskLogger;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.TaskActivityType;
using AutomatedTaskSystem.Models.Enums.TaskStatus;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Services.AuthService;
using AutomatedTaskSystem.Services.ResponseService;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AutomatedTaskSystem.Services.TaskLogger;

public class TaskLoggerService : ITaskLoggerService
{
    private const int DefaultPageSize = 5;
    private const int MaxPageSize = 500;
    private const int DefaultLookbackDays = 7;

    private static readonly string[] StaticSubjects =
        ["Arabic", "English", "Math (A)", "Science (A)", "Social", "ICT (A)", "MUL (A)", "Religion", "Other"];
    private static readonly string[] StaticStatuses = ["Approved", "Hold", "Rollback", "Red Flag"];

    private readonly IAuthService _authService;
    private readonly string _connectionString;

    public TaskLoggerService(DataContext context, IAuthService authService)
    {
        _authService = authService;
        _connectionString = context.Database.GetConnectionString()
            ?? throw new InvalidOperationException("Database connection string is not configured.");
    }

    public async Task<ActionResult<ResponseService<GetTaskLoggerDashboardDto>>> GetDashboardAsync(
        TaskLoggerFilterDto filter)
    {
        var user = await _authService.GetAuthedUser();
        if (user is null)
            return Unauthorized<GetTaskLoggerDashboardDto>("Invalid Auth");

        var normalized = NormalizeFilter(filter);
        var page = normalized.Page;
        var pageSize = normalized.PageSize;

        var rowsTask = QueryPagedRowsTwoPhaseAsync(normalized, user, page, pageSize);
        var summaryTask = QuerySummaryAsync(normalized, user);
        var rankingsTask = QueryRankingsAsync(normalized, user);
        var lookupsTask = QueryLightLookupsAsync(normalized, user);

        await System.Threading.Tasks.Task.WhenAll(rowsTask, summaryTask, rankingsTask, lookupsTask);

        var (rows, totalCount) = await rowsTask;

        return new ResponseService<GetTaskLoggerDashboardDto>
        {
            Error = false,
            Message = "Task logger dashboard",
            Data = new GetTaskLoggerDashboardDto
            {
                Rows = new GetTaskLoggerPagedDto
                {
                    Rows = rows,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                },
                Summary = await summaryTask,
                Rankings = await rankingsTask,
                Lookups = await lookupsTask
            }
        };
    }

    public async Task<ActionResult<ResponseService<GetTaskLoggerPagedDto>>> GetRowsAsync(TaskLoggerFilterDto filter)
    {
        var user = await _authService.GetAuthedUser();
        if (user is null)
            return Unauthorized<GetTaskLoggerPagedDto>("Invalid Auth");

        var normalized = NormalizeFilter(filter);
        var (rows, totalCount) = await QueryPagedRowsTwoPhaseAsync(
            normalized, user, normalized.Page, normalized.PageSize);

        return new ResponseService<GetTaskLoggerPagedDto>
        {
            Error = false,
            Message = "Task logger rows",
            Data = new GetTaskLoggerPagedDto
            {
                Rows = rows,
                TotalCount = totalCount,
                Page = normalized.Page,
                PageSize = normalized.PageSize
            }
        };
    }

    public async Task<ActionResult<ResponseService<GetTaskLoggerLookupsDto>>> GetLookupsAsync()
    {
        var user = await _authService.GetAuthedUser();
        if (user is null)
            return Unauthorized<GetTaskLoggerLookupsDto>("Invalid Auth");

        var filter = new TaskLoggerFilterDto
        {
            From = DateTime.Today.AddDays(-DefaultLookbackDays),
            To = DateTime.Today
        };
        var lookups = await QueryLightLookupsAsync(NormalizeFilter(filter), user);

        return new ResponseService<GetTaskLoggerLookupsDto>
        {
            Error = false,
            Message = "Task logger lookups",
            Data = lookups
        };
    }

    private async Task<(List<GetTaskLoggerRowDto> Rows, int TotalCount)> QueryPagedRowsTwoPhaseAsync(
        TaskLoggerFilterDto filter,
        User user,
        int page,
        int pageSize)
    {
        var (cteSql, filteredSql, parameters) = BuildQueryParts(filter, user, forRankings: false);
        parameters.Add("offset", (page - 1) * pageSize);
        parameters.Add("pageSize", pageSize);

        var phaseASql = $"""
            SET NOCOUNT ON;
            {cteSql},
            Filtered AS ({filteredSql})
            {TaskLoggerSql.CountAndPagedTaskIds}
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

        var enrichParams = BuildBaseParameters();
        enrichParams.Add("taskIds", taskIds);

        var enriched = (await connection.QueryAsync<GetTaskLoggerRowDto>(
            TaskLoggerSql.EnrichRowsSql, enrichParams, commandTimeout: 60)).ToList();

        var byId = enriched.ToDictionary(r => r.TaskId);
        var ordered = taskIds
            .Where(byId.ContainsKey)
            .Select(id => byId[id])
            .ToList();

        return (ordered, totalCount);
    }

    private async Task<GetTaskLoggerSummaryDto> QuerySummaryAsync(TaskLoggerFilterDto filter, User user)
    {
        var (cteSql, filteredSql, parameters) = BuildQueryParts(filter, user, forRankings: false);
        var sql = $"""
            SET NOCOUNT ON;
            {cteSql},
            Filtered AS ({filteredSql})
            {TaskLoggerSql.SummarySql}
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var row = await connection.QuerySingleAsync<SummaryRow>(sql, parameters, commandTimeout: 60);

        return new GetTaskLoggerSummaryDto
        {
            TotalTasks = row.TotalTasks,
            TotalTimeMinutes = row.TotalTimeMinutes,
            RollbackTasks = row.RollbackTasks,
            TotalPoints = row.TotalPoints
        };
    }

    private async Task<List<TaskLoggerMemberRankDto>> QueryRankingsAsync(TaskLoggerFilterDto filter, User user)
    {
        var (cteSql, filteredSql, parameters) = BuildQueryParts(filter, user, forRankings: true);
        var sql = $"""
            {cteSql},
            Filtered AS ({filteredSql})
            {TaskLoggerSql.RankingsSql}
            """;

        var rows = await RunQueryAsync<RankRow>(sql, parameters);
        return rows
            .Select((r, i) => new TaskLoggerMemberRankDto
            {
                Member = r.Member,
                Points = r.Points,
                Rank = i + 1
            })
            .ToList();
    }

    private async Task<GetTaskLoggerLookupsDto> QueryLightLookupsAsync(TaskLoggerFilterDto filter, User user)
    {
        var parameters = BuildBaseParameters();
        AddDateParameters(filter.From, filter.To, parameters);
        var roleFilter = BuildRoleFilterSql(user, parameters);

        var tasksSql = TaskLoggerSql.LightLookupsTaskNames.Replace("{ROLE_FILTER}", roleFilter);
        var batchSql = TaskLoggerSql.LightLookupsMembers + ";\n" + tasksSql;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var multi = await connection.QueryMultipleAsync(batchSql, parameters, commandTimeout: 60);
        var members = (await multi.ReadAsync<LookupValueRow>()).ToList();
        var taskNames = (await multi.ReadAsync<LookupValueRow>()).ToList();

        return new GetTaskLoggerLookupsDto
        {
            Members = members.Select(r => r.Value).ToList(),
            Subjects = StaticSubjects.ToList(),
            TaskNames = taskNames.Select(r => r.Value).ToList(),
            Statuses = StaticStatuses.ToList()
        };
    }

    private (string BaseCte, string FilteredSql, DynamicParameters Parameters) BuildQueryParts(
        TaskLoggerFilterDto filter,
        User user,
        bool forRankings)
    {
        var parameters = BuildBaseParameters();
        var roleFilter = BuildRoleFilterSql(user, parameters);

        DateTime fromDate;
        DateTime toDate;
        string rankingActivityDate = "";

        if (forRankings)
        {
            toDate = DateTime.Today;
            switch ((filter.RankingRange ?? "all").Trim().ToLowerInvariant())
            {
                case "week":
                    fromDate = StartOfWeek(DateTime.Today);
                    rankingActivityDate = "AND TimeStamp >= @fromDate AND TimeStamp < DATEADD(DAY, 1, @toDate)";
                    break;
                case "month":
                    fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    rankingActivityDate = "AND TimeStamp >= @fromDate AND TimeStamp < DATEADD(DAY, 1, @toDate)";
                    break;
                default:
                    // All time: no activity date bound; still require report date coalesce for consistency
                    fromDate = new DateTime(2000, 1, 1);
                    rankingActivityDate = "";
                    break;
            }
            parameters.Add("fromDate", fromDate);
            parameters.Add("toDate", toDate);
        }
        else
        {
            fromDate = filter.From?.Date ?? DateTime.Today.AddDays(-DefaultLookbackDays);
            toDate = filter.To?.Date ?? DateTime.Today;
            parameters.Add("fromDate", fromDate);
            parameters.Add("toDate", toDate);
        }

        var dateFilterBase = forRankings && (filter.RankingRange ?? "all").Equals("all", StringComparison.OrdinalIgnoreCase)
            ? ""
            : """
              AND COALESCE(ad.ReportDate, t.CreatedAt) >= @fromDate
              AND COALESCE(ad.ReportDate, t.CreatedAt) < DATEADD(DAY, 1, @toDate)
              """;

        if (!string.IsNullOrWhiteSpace(filter.Search))
            parameters.Add("search", $"%{filter.Search.Trim()}%");
        if (!string.IsNullOrWhiteSpace(filter.Member))
            parameters.Add("member", filter.Member);
        if (!string.IsNullOrWhiteSpace(filter.Subject))
            parameters.Add("subject", filter.Subject);
        if (!string.IsNullOrWhiteSpace(filter.Status))
            parameters.Add("status", filter.Status);
        if (!string.IsNullOrWhiteSpace(filter.TaskName))
            parameters.Add("taskName", filter.TaskName);

        var baseTemplate = forRankings ? TaskLoggerSql.RankingsBaseCte : TaskLoggerSql.LightBaseCte;
        var baseCte = baseTemplate
            .Replace("{ROLE_FILTER}", roleFilter)
            .Replace("{DATE_FILTER_BASE}", dateFilterBase)
            .Replace("{RANKING_ACTIVITY_DATE}", rankingActivityDate);

        // Rankings CTE has no LoCode/Subject filters applied from table filters for ranking purity —
        // but plan says rankings are by member within role scope; apply same filters except search on notes.
        var filteredRows = TaskLoggerSql.FilteredRows
            .Replace("{SEARCH_FILTER}",
                forRankings || string.IsNullOrWhiteSpace(filter.Search)
                    ? ""
                    : "AND (LoCode LIKE @search OR TaskName LIKE @search)")
            .Replace("{MEMBER_FILTER}",
                forRankings || string.IsNullOrWhiteSpace(filter.Member) ? "" : "AND Member = @member")
            .Replace("{SUBJECT_FILTER}",
                forRankings || string.IsNullOrWhiteSpace(filter.Subject) ? "" : "AND Subject = @subject")
            .Replace("{STATUS_FILTER}",
                forRankings || string.IsNullOrWhiteSpace(filter.Status) ? "" : "AND [Status] = @status")
            .Replace("{TASK_FILTER}",
                forRankings || string.IsNullOrWhiteSpace(filter.TaskName) ? "" : "AND TaskName = @taskName");

        // For rankings FilteredRows references BaseRows which only has Member + Points — strip filters that don't exist
        if (forRankings)
        {
            filteredRows = """
                SELECT * FROM BaseRows
                WHERE 1 = 1
                """;
        }

        return (baseCte, filteredRows, parameters);
    }

    private static TaskLoggerFilterDto NormalizeFilter(TaskLoggerFilterDto filter) =>
        new()
        {
            From = filter.From,
            To = filter.To,
            Search = filter.Search,
            Member = filter.Member,
            Subject = filter.Subject,
            Status = filter.Status,
            TaskName = filter.TaskName,
            RankingRange = string.IsNullOrWhiteSpace(filter.RankingRange) ? "all" : filter.RankingRange.Trim().ToLowerInvariant(),
            Page = Math.Max(1, filter.Page),
            PageSize = Math.Clamp(filter.PageSize <= 0 ? DefaultPageSize : filter.PageSize, 1, MaxPageSize)
        };

    private static void AddDateParameters(DateTime? from, DateTime? to, DynamicParameters parameters)
    {
        parameters.Add("fromDate", from?.Date ?? DateTime.Today.AddDays(-DefaultLookbackDays));
        parameters.Add("toDate", to?.Date ?? DateTime.Today);
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
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

    private static DynamicParameters BuildBaseParameters()
    {
        var parameters = new DynamicParameters();
        parameters.Add("statusDone", (int)TaskActivityTypeEnum.Status_Done);
        parameters.Add("statusRollback", (int)TaskActivityTypeEnum.Status_Rollback);
        parameters.Add("rollbackActivity", (int)TaskActivityTypeEnum.Rollback);
        parameters.Add("taskDone", (int)TaskStatusEnum.Done);
        parameters.Add("taskRollback", (int)TaskStatusEnum.Rollback);
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

    private sealed class SummaryRow
    {
        public int TotalTasks { get; set; }
        public double TotalTimeMinutes { get; set; }
        public int RollbackTasks { get; set; }
        public double TotalPoints { get; set; }
    }

    private sealed class RankRow
    {
        public string Member { get; set; } = "";
        public double Points { get; set; }
    }

    private sealed class LookupValueRow
    {
        public string Value { get; set; } = "";
    }
}

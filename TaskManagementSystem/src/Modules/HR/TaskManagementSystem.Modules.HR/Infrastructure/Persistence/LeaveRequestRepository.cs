using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence;

internal sealed class LeaveRequestRepository(HrDbContext context)
    : GenericRepository<LeaveRequest, HrDbContext>(context), ILeaveRequestRepository
{
    public Task AddAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default) =>
        AddEntityAsync(leaveRequest, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<LeaveRequest> leaveRequests, CancellationToken cancellationToken = default)
    {
        await Set.AddRangeAsync(leaveRequests, cancellationToken);
    }

    public Task<LeaveRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(leave => leave.Id == id, cancellationToken);

    public Task<LeaveRequest?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(leave => leave.Id == id, cancellationToken);

    public async Task<IReadOnlyList<LeaveRequest>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(leave => leave.UserId == userId)
            .OrderByDescending(leave => leave.DateCreated)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LeaveRequest>> GetPendingAsync(
        CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(leave => leave.Status == LeaveStatus.Pending)
            .OrderBy(leave => leave.DateCreated)
            .ToListAsync(cancellationToken);

    public async Task<int> SumPendingWorkingDaysAsync(
        int userId,
        LeaveType type,
        int? excludeLeaveRequestId = null,
        CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking()
            .Where(leave =>
                leave.UserId == userId &&
                leave.Type == type &&
                leave.Status == LeaveStatus.Pending);

        if (excludeLeaveRequestId.HasValue)
        {
            query = query.Where(leave => leave.Id != excludeLeaveRequestId.Value);
        }

        var requests = await query.ToListAsync(cancellationToken);
        return requests.Sum(leave => leave.WorkingDays);
    }

    public async Task<LeaveRequestSearchResult> SearchAsync(
        LeaveRequestSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = from leave in Set.AsNoTracking()
                    join employee in context.Employees on leave.UserId equals employee.Id
                    where leave.Status != LeaveStatus.Cancelled
                    select new SearchRow(leave, employee);

        query = ApplyRoleScope(query, criteria);

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            query = query.Where(x =>
                x.Employee.Name.Contains(criteria.Search) ||
                (x.Leave.Reason != null && x.Leave.Reason.Contains(criteria.Search)));
        }

        if (criteria.FromDate.HasValue)
        {
            query = query.Where(x => x.Leave.StartDate >= criteria.FromDate.Value.Date);
        }

        if (criteria.ToDate.HasValue)
        {
            query = query.Where(x => x.Leave.EndDate <= criteria.ToDate.Value.Date);
        }

        if (criteria.Status.HasValue)
        {
            query = query.Where(x => x.Leave.Status == criteria.Status.Value);
        }

        if (criteria.Type.HasValue)
        {
            query = query.Where(x => x.Leave.Type == criteria.Type.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.MyStatus))
        {
            query = ApplyMyStatusFilter(query, criteria.MyStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var page = Math.Max(1, criteria.Page);
        var pageSize = Math.Clamp(criteria.PageSize, 1, 100);

        var items = await query
            .OrderByDescending(x => x.Leave.DateCreated)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.Leave)
            .ToListAsync(cancellationToken);

        return new LeaveRequestSearchResult(items, totalCount);
    }

    private static IQueryable<SearchRow> ApplyRoleScope(IQueryable<SearchRow> query, LeaveRequestSearchCriteria criteria) =>
        criteria.ViewerRole switch
        {
            "Owner" => query,
            "ProjectManger" => query.Where(x => x.Leave.UserId != criteria.ViewerUserId),
            "TeamLeader" => query.Where(x =>
                x.Leave.UserId != criteria.ViewerUserId &&
                (x.Employee.TeamleaderId == criteria.ViewerUserId ||
                 (criteria.ViewerTeamId.HasValue && x.Employee.TeamId == criteria.ViewerTeamId))),
            "SectionHead" => query.Where(x =>
                x.Leave.UserId != criteria.ViewerUserId &&
                criteria.SectionTeamIds.Contains(x.Employee.TeamId ?? -1)),
            _ => query.Where(x => x.Leave.UserId == criteria.ViewerUserId)
        };

    private static IQueryable<SearchRow> ApplyMyStatusFilter(IQueryable<SearchRow> query, string myStatus)
    {
        var normalized = myStatus.ToLowerInvariant();
        return normalized switch
        {
            "pending" => query.Where(x => x.Leave.Status == LeaveStatus.Pending),
            "approved" => query.Where(x => x.Leave.Status == LeaveStatus.Approved),
            "rejected" => query.Where(x => x.Leave.Status == LeaveStatus.Rejected),
            _ => query
        };
    }

    private sealed record SearchRow(LeaveRequest Leave, EmployeeReadModel Employee);
}

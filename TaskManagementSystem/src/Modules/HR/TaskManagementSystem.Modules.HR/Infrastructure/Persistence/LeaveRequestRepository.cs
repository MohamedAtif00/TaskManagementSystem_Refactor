using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence;

internal sealed class LeaveRequestRepository(
    HrDbContext context,
    LeaveRequestSearchQueries searchQueries)
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
        var totals = await SumPendingWorkingDaysByTypeAsync(
            userId,
            [type],
            excludeLeaveRequestId,
            cancellationToken);

        return totals.GetValueOrDefault(type);
    }

    public async Task<IReadOnlyDictionary<LeaveType, int>> SumPendingWorkingDaysByTypeAsync(
        int userId,
        IReadOnlyCollection<LeaveType> types,
        int? excludeLeaveRequestId = null,
        CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking()
            .Where(leave =>
                leave.UserId == userId &&
                leave.Status == LeaveStatus.Pending &&
                types.Contains(leave.Type));

        if (excludeLeaveRequestId.HasValue)
        {
            query = query.Where(leave => leave.Id != excludeLeaveRequestId.Value);
        }

        return await query
            .GroupBy(leave => leave.Type)
            .Select(group => new { Type = group.Key, Total = group.Sum(leave => leave.WorkingDays) })
            .ToDictionaryAsync(row => row.Type, row => row.Total, cancellationToken);
    }

    public Task<LeaveRequestSearchResult> SearchAsync(
        LeaveRequestSearchCriteria criteria,
        CancellationToken cancellationToken = default) =>
        searchQueries.SearchAsync(criteria, cancellationToken);
}

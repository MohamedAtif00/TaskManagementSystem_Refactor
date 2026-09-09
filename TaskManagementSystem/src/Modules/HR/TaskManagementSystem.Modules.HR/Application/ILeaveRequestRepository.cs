using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Application;

public interface ILeaveRequestRepository
{
    Task AddAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<LeaveRequest> leaveRequests, CancellationToken cancellationToken = default);

    Task<LeaveRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<LeaveRequest?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeaveRequest>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeaveRequest>> GetPendingAsync(CancellationToken cancellationToken = default);

    Task<int> SumPendingWorkingDaysAsync(
        int userId,
        LeaveType type,
        int? excludeLeaveRequestId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<LeaveType, int>> SumPendingWorkingDaysByTypeAsync(
        int userId,
        IReadOnlyCollection<LeaveType> types,
        int? excludeLeaveRequestId = null,
        CancellationToken cancellationToken = default);

    Task<LeaveRequestSearchResult> SearchAsync(
        LeaveRequestSearchCriteria criteria,
        CancellationToken cancellationToken = default);
}

public sealed record LeaveRequestSearchCriteria(
    int ViewerUserId,
    string ViewerRole,
    int? ViewerTeamId,
    IReadOnlyList<int> SectionTeamIds,
    int Page,
    int PageSize,
    string? Search,
    DateTime? FromDate,
    DateTime? ToDate,
    LeaveStatus? Status,
    LeaveType? Type,
    string? MyStatus);

public sealed record LeaveRequestSearchResult(
    IReadOnlyList<LeaveRequest> Items,
    int TotalCount);

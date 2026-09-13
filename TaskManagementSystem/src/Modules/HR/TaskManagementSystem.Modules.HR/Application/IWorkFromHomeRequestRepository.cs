using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Application;

public interface IWorkFromHomeRequestRepository
{
    Task AddAsync(WorkFromHomeRequest workFromHomeRequest, CancellationToken cancellationToken = default);

    Task<WorkFromHomeRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<WorkFromHomeRequest?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkFromHomeRequest>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkFromHomeRequest>> GetPendingAsync(CancellationToken cancellationToken = default);

    Task<int> CountPendingAsync(int userId, int? excludeWorkFromHomeRequestId = null, CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveForDateAsync(
        int userId,
        DateTime date,
        int? excludeWorkFromHomeRequestId = null,
        CancellationToken cancellationToken = default);

    Task<WorkFromHomeRequestSearchResult> SearchAsync(
        WorkFromHomeRequestSearchCriteria criteria,
        CancellationToken cancellationToken = default);
}

public sealed record WorkFromHomeRequestSearchCriteria(
    int ViewerUserId,
    string ViewerRole,
    int? ViewerTeamId,
    int Page,
    int PageSize,
    string? Search,
    DateTime? FromDate,
    DateTime? ToDate,
    WorkFromHomeStatus? Status,
    string? MyStatus);

public sealed record WorkFromHomeRequestSearchResult(
    IReadOnlyList<WorkFromHomeRequest> Items,
    int TotalCount);

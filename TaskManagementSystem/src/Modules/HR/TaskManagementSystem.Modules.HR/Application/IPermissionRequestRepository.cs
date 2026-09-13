using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Application;

public interface IPermissionRequestRepository
{
    Task AddAsync(PermissionRequest permissionRequest, CancellationToken cancellationToken = default);

    Task<PermissionRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PermissionRequest?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PermissionRequest>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PermissionRequest>> GetPendingAsync(CancellationToken cancellationToken = default);

    Task<int> CountPendingAsync(int userId, int? excludePermissionId = null, CancellationToken cancellationToken = default);

    Task<PermissionRequestSearchResult> SearchAsync(
        PermissionRequestSearchCriteria criteria,
        CancellationToken cancellationToken = default);
}

public sealed record PermissionRequestSearchCriteria(
    int ViewerUserId,
    string ViewerRole,
    int? ViewerTeamId,
    int Page,
    int PageSize,
    string? Search,
    DateTime? Date,
    DateTime? FromDate,
    DateTime? ToDate,
    PermissionStatus? Status,
    PermissionType? Type,
    string? MyStatus);

public sealed record PermissionRequestSearchResult(
    IReadOnlyList<PermissionRequest> Items,
    int TotalCount);

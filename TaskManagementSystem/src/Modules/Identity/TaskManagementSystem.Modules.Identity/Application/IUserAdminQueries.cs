using TaskManagementSystem.BuildingBlocks.Application.Paging;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Identity.Application;

public interface IUserAdminQueries
{
    Task<IReadOnlyList<UserListItemReadModel>> ListActiveAsync(CancellationToken cancellationToken = default);

    Task<Result<PageListResult<UserListItemReadModel>>> ListActivePagedAsync(
        string? search,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default);

    Task<UserDetailReadModel?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<UserDetailReadModel?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TeamLeaderReadModel>> ListTeamLeadersAsync(CancellationToken cancellationToken = default);
}

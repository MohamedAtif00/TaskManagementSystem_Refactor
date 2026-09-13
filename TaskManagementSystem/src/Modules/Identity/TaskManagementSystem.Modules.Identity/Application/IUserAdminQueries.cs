namespace TaskManagementSystem.Modules.Identity.Application;

public interface IUserAdminQueries
{
    Task<IReadOnlyList<UserListItemReadModel>> ListActiveAsync(CancellationToken cancellationToken = default);

    Task<UserDetailReadModel?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TeamLeaderReadModel>> ListTeamLeadersAsync(CancellationToken cancellationToken = default);
}

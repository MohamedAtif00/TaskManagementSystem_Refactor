namespace TaskManagementSystem.Modules.Curriculum.Application;

public interface IIdentityUserLookup
{
    Task<bool> ActiveUsersExistAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserSummary>> GetActiveUsersByIdsAsync(
        IReadOnlyCollection<int> userIds,
        CancellationToken cancellationToken = default);
}

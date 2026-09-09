namespace TaskManagementSystem.Modules.HR.Application;

public interface IOrgLookupRepository
{
    Task<int?> GetSectionHeadIdForTeamAsync(int teamId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetTeamIdsForSectionHeadAsync(int sectionHeadUserId, CancellationToken cancellationToken = default);
}

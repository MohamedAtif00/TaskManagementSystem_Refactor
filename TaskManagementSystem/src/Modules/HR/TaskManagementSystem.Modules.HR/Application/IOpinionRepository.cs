using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Application;

public interface IOpinionRepository
{
    Task AddAsync(Opinion opinion, CancellationToken cancellationToken = default);

    Task<bool> ExistsForUserAsync(int leaveRequestId, int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Opinion>> GetByLeaveRequestIdAsync(int leaveRequestId, CancellationToken cancellationToken = default);
}

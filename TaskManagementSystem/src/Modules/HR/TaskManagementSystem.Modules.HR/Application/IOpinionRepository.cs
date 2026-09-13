using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Application;

public interface IOpinionRepository
{
    Task AddAsync(Opinion opinion, CancellationToken cancellationToken = default);

    Task<bool> ExistsForLeaveUserAsync(int leaveRequestId, int userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsForPermissionUserAsync(int permissionId, int userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsForWorkFromHomeUserAsync(int workFromHomeRequestId, int userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsForForgotClockUserAsync(int forgotClockRequestId, int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Opinion>> GetByLeaveRequestIdAsync(int leaveRequestId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Opinion>> GetByPermissionIdAsync(int permissionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Opinion>> GetByWorkFromHomeRequestIdAsync(int workFromHomeRequestId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Opinion>> GetByForgotClockRequestIdAsync(int forgotClockRequestId, CancellationToken cancellationToken = default);
}

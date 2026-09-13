using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Application;

public interface IForgotClockRequestRepository
{
    Task AddAsync(ForgotClockRequest forgotClockRequest, CancellationToken cancellationToken = default);

    Task<ForgotClockRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ForgotClockRequest?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ForgotClockRequest>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ForgotClockRequest>> GetPendingAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveForDateAndPunchTypeAsync(
        int userId,
        DateTime attendanceDate,
        ForgotClockPunchType punchType,
        int? excludeForgotClockRequestId = null,
        CancellationToken cancellationToken = default);

    Task<ForgotClockRequestSearchResult> SearchAsync(
        ForgotClockRequestSearchCriteria criteria,
        CancellationToken cancellationToken = default);
}

public sealed record ForgotClockRequestSearchCriteria(
    int ViewerUserId,
    string ViewerRole,
    int? ViewerTeamId,
    int Page,
    int PageSize,
    string? Search,
    DateTime? Date,
    DateTime? FromDate,
    DateTime? ToDate,
    ForgotClockStatus? Status,
    ForgotClockPunchType? PunchType,
    string? MyStatus);

public sealed record ForgotClockRequestSearchResult(
    IReadOnlyList<ForgotClockRequest> Items,
    int TotalCount);

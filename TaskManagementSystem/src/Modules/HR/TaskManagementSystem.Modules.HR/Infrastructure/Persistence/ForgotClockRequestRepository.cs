using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence;

internal sealed class ForgotClockRequestRepository(
    HrDbContext context,
    ForgotClockRequestSearchQueries searchQueries)
    : GenericRepository<ForgotClockRequest, HrDbContext>(context), IForgotClockRequestRepository
{
    public Task AddAsync(ForgotClockRequest forgotClockRequest, CancellationToken cancellationToken = default) =>
        AddEntityAsync(forgotClockRequest, cancellationToken);

    public Task<ForgotClockRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(request => request.Id == id, cancellationToken);

    public Task<ForgotClockRequest?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(request => request.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ForgotClockRequest>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(request => request.UserId == userId)
            .OrderByDescending(request => request.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ForgotClockRequest>> GetPendingAsync(
        CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(request => request.Status == ForgotClockStatus.Pending)
            .OrderBy(request => request.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsActiveForDateAndPunchTypeAsync(
        int userId,
        DateTime attendanceDate,
        ForgotClockPunchType punchType,
        int? excludeForgotClockRequestId = null,
        CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking()
            .Where(request =>
                request.UserId == userId &&
                request.AttendanceDate.Date == attendanceDate.Date &&
                request.PunchType == punchType &&
                request.Status != ForgotClockStatus.Cancelled &&
                request.Status != ForgotClockStatus.Rejected);

        if (excludeForgotClockRequestId.HasValue)
        {
            query = query.Where(request => request.Id != excludeForgotClockRequestId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task<ForgotClockRequestSearchResult> SearchAsync(
        ForgotClockRequestSearchCriteria criteria,
        CancellationToken cancellationToken = default) =>
        searchQueries.SearchAsync(criteria, cancellationToken);
}

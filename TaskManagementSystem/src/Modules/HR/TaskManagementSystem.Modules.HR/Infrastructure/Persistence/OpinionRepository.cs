using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence;

internal sealed class OpinionRepository(HrDbContext context) : IOpinionRepository
{
    public async Task AddAsync(Opinion opinion, CancellationToken cancellationToken = default)
    {
        await context.Opinions.AddAsync(opinion, cancellationToken);
    }

    public Task<bool> ExistsForUserAsync(int leaveRequestId, int userId, CancellationToken cancellationToken = default) =>
        context.Opinions.AsNoTracking()
            .AnyAsync(opinion => opinion.LeaveRequestId == leaveRequestId && opinion.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Opinion>> GetByLeaveRequestIdAsync(
        int leaveRequestId,
        CancellationToken cancellationToken = default) =>
        await context.Opinions.AsNoTracking()
            .Where(opinion => opinion.LeaveRequestId == leaveRequestId)
            .OrderBy(opinion => opinion.CreatedAt)
            .ToListAsync(cancellationToken);
}

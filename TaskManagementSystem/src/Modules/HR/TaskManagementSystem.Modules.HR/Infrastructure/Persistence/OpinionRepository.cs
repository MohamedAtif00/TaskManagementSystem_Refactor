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

    public Task<bool> ExistsForLeaveUserAsync(int leaveRequestId, int userId, CancellationToken cancellationToken = default) =>
        context.Opinions.AsNoTracking()
            .AnyAsync(opinion => opinion.LeaveRequestId == leaveRequestId && opinion.UserId == userId, cancellationToken);

    public Task<bool> ExistsForPermissionUserAsync(int permissionId, int userId, CancellationToken cancellationToken = default) =>
        context.Opinions.AsNoTracking()
            .AnyAsync(opinion => opinion.PermissionId == permissionId && opinion.UserId == userId, cancellationToken);

    public Task<bool> ExistsForWorkFromHomeUserAsync(int workFromHomeRequestId, int userId, CancellationToken cancellationToken = default) =>
        context.Opinions.AsNoTracking()
            .AnyAsync(opinion => opinion.WorkFromHomeRequestId == workFromHomeRequestId && opinion.UserId == userId, cancellationToken);

    public Task<bool> ExistsForForgotClockUserAsync(int forgotClockRequestId, int userId, CancellationToken cancellationToken = default) =>
        context.Opinions.AsNoTracking()
            .AnyAsync(opinion => opinion.ForgotClockRequestId == forgotClockRequestId && opinion.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Opinion>> GetByLeaveRequestIdAsync(
        int leaveRequestId,
        CancellationToken cancellationToken = default) =>
        await context.Opinions.AsNoTracking()
            .Where(opinion => opinion.LeaveRequestId == leaveRequestId)
            .OrderBy(opinion => opinion.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Opinion>> GetByPermissionIdAsync(
        int permissionId,
        CancellationToken cancellationToken = default) =>
        await context.Opinions.AsNoTracking()
            .Where(opinion => opinion.PermissionId == permissionId)
            .OrderBy(opinion => opinion.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Opinion>> GetByWorkFromHomeRequestIdAsync(
        int workFromHomeRequestId,
        CancellationToken cancellationToken = default) =>
        await context.Opinions.AsNoTracking()
            .Where(opinion => opinion.WorkFromHomeRequestId == workFromHomeRequestId)
            .OrderBy(opinion => opinion.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Opinion>> GetByForgotClockRequestIdAsync(
        int forgotClockRequestId,
        CancellationToken cancellationToken = default) =>
        await context.Opinions.AsNoTracking()
            .Where(opinion => opinion.ForgotClockRequestId == forgotClockRequestId)
            .OrderBy(opinion => opinion.CreatedAt)
            .ToListAsync(cancellationToken);
}

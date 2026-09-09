using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Infrastructure.Persistence;

internal sealed class AboutMeRepository(IdentityDbContext context) : IAboutMeRepository
{
    public async Task<AboutMeReadModel?> GetAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .Include(u => u.Role!)
            .ThenInclude(role => role.Permissions)
            .Active()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        string? teamName = null;
        if (user.TeamId is not null)
        {
            teamName = await context.Teams
                .AsNoTracking()
                .Where(team => team.Id == user.TeamId)
                .Select(team => team.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var unreadNotifications = await context.Notifications
            .AsNoTracking()
            .CountAsync(
                notification => notification.UserId == userId && !notification.IsRead,
                cancellationToken);

        return new AboutMeReadModel(
            user.Id,
            user.Name,
            user.RoleId,
            user.RoleName,
            user.PermissionCodes.ToList(),
            teamName,
            unreadNotifications);
    }
}

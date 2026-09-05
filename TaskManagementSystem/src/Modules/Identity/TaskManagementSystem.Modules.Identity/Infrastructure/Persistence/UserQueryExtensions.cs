using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Infrastructure.Persistence;

internal static class UserQueryExtensions
{
    internal static IQueryable<User> Active(this IQueryable<User> users) =>
        users.Where(user => !user.Archived);
}

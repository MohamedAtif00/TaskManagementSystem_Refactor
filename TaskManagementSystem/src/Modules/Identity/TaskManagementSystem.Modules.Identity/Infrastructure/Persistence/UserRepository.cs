using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Infrastructure.Persistence;

internal sealed class UserRepository(IdentityDbContext context) : IUserRepository
{
    private static readonly char[] CodeCharacters =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

    private IQueryable<User> AuthQuery() =>
        context.Users
            .AsNoTracking()
            .Include(user => user.Role!)
            .ThenInclude(role => role.Permissions);

    public Task<User?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        AuthQuery()
            .Active()
            .FirstOrDefaultAsync(user => user.Code == code, cancellationToken);

    public Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default) =>
        AuthQuery()
            .Active()
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

    public Task<User?> GetByIdTrackedAsync(int userId, CancellationToken cancellationToken = default) =>
        context.Users
            .Include(user => user.Role!)
            .ThenInclude(role => role.Permissions)
            .Active()
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

    public async Task<IReadOnlyList<User>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        await AuthQuery()
            .Active()
            .OrderBy(user => user.Name)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsActiveByEmailAsync(
        string email,
        int? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return context.Users
            .AsNoTracking()
            .Active()
            .AnyAsync(
                user => user.Email != null
                    && user.Email.ToLower() == normalizedEmail
                    && (!excludeUserId.HasValue || user.Id != excludeUserId.Value),
                cancellationToken);
    }

    public Task<bool> ExistsActiveCodeAsync(string code, CancellationToken cancellationToken = default) =>
        context.Users
            .AsNoTracking()
            .AnyAsync(user => user.Code == code, cancellationToken);

    public async Task<string> GenerateUniqueCodeAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var available = CodeCharacters.ToList();
            var codeChars = new char[6];
            for (var index = 0; index < codeChars.Length; index++)
            {
                var pick = Random.Shared.Next(available.Count);
                codeChars[index] = available[pick];
                available.RemoveAt(pick);
            }

            var code = new string(codeChars);
            if (!await ExistsActiveCodeAsync(code, cancellationToken))
            {
                return code;
            }
        }
    }

    public Task<bool> IsActiveTeamLeaderOrSectionHeadAsync(int userId, CancellationToken cancellationToken = default) =>
        context.Users
            .AsNoTracking()
            .Active()
            .AnyAsync(
                user => user.Id == userId
                    && (user.RoleId == (int)UserRole.TeamLeader || user.RoleId == (int)UserRole.SectionHead),
                cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await context.Users.AddAsync(user, cancellationToken);
    }
}

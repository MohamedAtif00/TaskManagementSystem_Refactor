using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Application;

public interface ITokenGenerator
{
    string CreateAccessToken(User user);

    RefreshToken CreateRefreshToken(User user, DateTime utcNow);
}

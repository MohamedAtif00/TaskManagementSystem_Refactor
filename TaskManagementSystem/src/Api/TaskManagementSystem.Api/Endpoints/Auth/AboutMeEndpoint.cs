using MediatR;
using TaskManagementSystem.Api.Contracts.Auth;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.Identity.Features.AboutMe;

namespace TaskManagementSystem.Api.Endpoints.Auth;

/// <summary>
/// POST /auth/about-me — Returns authenticated user profile, role, permissions, and notification summary.
/// Requires bearer authorization.
/// </summary>
public static class AboutMeEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapPost("/about-me", HandleAsync).RequireAuthorization();
        return group;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var result = await mediator.Send(new AboutMeQuery(userId), cancellationToken);

        return result.ToHttpResult(profile => Results.Ok(new AuthInfoResponse
        {
            Id = profile.Id,
            Name = profile.Name,
            Role = profile.Role,
            RoleName = profile.RoleName,
            Permissions = profile.Permissions.ToArray(),
            Group = profile.Group,
            TeamId = profile.TeamId,
            HeadedTeamIds = profile.HeadedTeamIds.ToArray(),
            Notifications = profile.Notifications
        }));
    }
}

using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Users.GetUserById;

public sealed record GetUserByIdQuery(int UserId) : IQuery<Result<UserDetailResult>>;

public sealed class GetUserByIdQueryHandler(IUserAdminQueries userAdminQueries)
    : IRequestHandler<GetUserByIdQuery, Result<UserDetailResult>>
{
    public async Task<Result<UserDetailResult>> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userAdminQueries.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Fail<UserDetailResult>(IdentityErrors.UserNotFound);
        }

        return Result.Ok(UserDetailResult.From(user));
    }
}

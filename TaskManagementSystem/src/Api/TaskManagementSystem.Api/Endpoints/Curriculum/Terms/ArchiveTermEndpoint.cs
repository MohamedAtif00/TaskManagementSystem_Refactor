using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
 using TaskManagementSystem.Modules.Curriculum.Features.Terms.ArchiveTerm;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Terms;

/// <summary>
/// Archives a term.
/// </summary>
public static class ArchiveTermEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder terms)
    {
        terms.MapDelete("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Delete);
        return terms;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveTermCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}

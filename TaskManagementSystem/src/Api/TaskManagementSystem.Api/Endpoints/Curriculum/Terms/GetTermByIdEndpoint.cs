using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.Terms.GetTermById;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Terms;

/// <summary>
/// Gets a term by identifier.
/// </summary>
public static class GetTermByIdEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder terms)
    {
        terms.MapGet("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Read);
        return terms;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTermByIdQuery(id), cancellationToken);
        return result.ToHttpResult(term => Results.Ok(CurriculumMapping.MapTermDetail(term)));
    }
}

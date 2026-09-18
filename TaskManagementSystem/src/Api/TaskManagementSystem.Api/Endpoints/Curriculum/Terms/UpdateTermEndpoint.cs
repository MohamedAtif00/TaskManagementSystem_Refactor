using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.Terms.UpdateTerm;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Terms;

/// <summary>
/// Updates an existing term.
/// </summary>
public static class UpdateTermEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder terms)
    {
        terms.MapPut("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Update);
        return terms;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        UpdateTermRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateTermCommand(id, request.Name, request.StartDate, request.EndDate), cancellationToken);
        return result.ToHttpResult(term => Results.Ok(CurriculumMapping.MapTermDetail(term)));
    }
}

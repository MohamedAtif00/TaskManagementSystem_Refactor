using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.ListSubjectGroupsByTerm;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Terms;

/// <summary>
/// Lists subject groups for a term.
/// </summary>
public static class ListSubjectGroupsByTermEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder terms)
    {
        terms.MapGet("/{termId:int}/subject-groups", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Read);
        return terms;
    }

    private static async Task<IResult> HandleAsync(
        int termId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListSubjectGroupsByTermQuery(termId), cancellationToken);
        return result.ToHttpResult(groups => Results.Ok(groups.Select(CurriculumMapping.MapSubjectGroupListItem).ToList()));
    }
}

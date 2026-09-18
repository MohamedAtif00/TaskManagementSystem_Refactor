using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.CreateSubjectGroup;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Terms;

/// <summary>
/// Creates a subject group under a term.
/// </summary>
public static class CreateSubjectGroupEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder terms)
    {
        terms.MapPost("/{termId:int}/subject-groups", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Create);
        return terms;
    }

    private static async Task<IResult> HandleAsync(
        int termId,
        CreateSubjectGroupRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateSubjectGroupCommand(termId, request.Name), cancellationToken);
        return result.ToHttpResult(group => Results.Created($"/curriculum/subject-groups/{group.Id}", CurriculumMapping.MapSubjectGroupDetail(group)));
    }
}

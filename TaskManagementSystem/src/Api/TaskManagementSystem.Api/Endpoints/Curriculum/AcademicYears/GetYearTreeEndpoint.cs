using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.YearTree.GetYearTree;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.AcademicYears;

/// <summary>
/// Gets the curriculum tree for an academic year.
/// </summary>
public static class GetYearTreeEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder years)
    {
        years.MapGet("/{yearId:int}/tree", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Read);
        return years;
    }

    private static async Task<IResult> HandleAsync(
        int yearId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetYearTreeQuery(yearId), cancellationToken);
        return result.ToHttpResult(tree => Results.Ok(CurriculumMapping.MapYearTree(tree)));
    }
}

using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.YearTree;
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
        string? statusTab,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var statuses = YearTreeStatusTabFilter.TryParseStatusTab(statusTab, out var valid);
        if (!valid)
        {
            return Results.BadRequest(new { message = "statusTab must be active, hold, or closed." });
        }

        var result = await mediator.Send(new GetYearTreeQuery(yearId, statuses), cancellationToken);
        return result.ToHttpResult(tree => Results.Ok(CurriculumMapping.MapYearTree(tree)));
    }
}

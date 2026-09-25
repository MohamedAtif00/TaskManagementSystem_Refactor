using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Endpoints.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Curriculum.Features.Subjects.ListSubjectsPaged;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Subjects;

public static class ListSubjectsPagedEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder subjects)
    {
        subjects.MapGet("", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Read);
        subjects.MapGet("/filter-options", HandleFilterOptionsAsync).RequirePermissionCode(PermissionCodes.Curriculum.Read);
        subjects.MapGet("/export", HandleExportAsync).RequirePermissionCode(PermissionCodes.Curriculum.Read);
        return subjects;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        string? search = null,
        string? year = null,
        string? term = null,
        int? page = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new ListSubjectsPagedQuery(search, year, term, page, pageSize),
            cancellationToken);

        return result.ToHttpResult(page => Results.Ok(new SubjectCatalogPageResponse
        {
            Items = page.Items.Select(CurriculumMapping.MapSubjectCatalogListItem).ToList(),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount
        }));
    }

    private static async Task<IResult> HandleFilterOptionsAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSubjectCatalogFilterOptionsQuery(), cancellationToken);
        return result.ToHttpResult(options => Results.Ok(new SubjectCatalogFilterOptionsResponse
        {
            Years = options.Years,
            Terms = options.Terms
        }));
    }

    private static async Task<IResult> HandleExportAsync(
        IMediator mediator,
        string? search = null,
        string? year = null,
        string? term = null,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new ListSubjectsExportQuery(search, year, term), cancellationToken);
        return result.ToHttpResult(items =>
            Results.Ok(items.Select(CurriculumMapping.MapSubjectCatalogListItem).ToList()));
    }
}

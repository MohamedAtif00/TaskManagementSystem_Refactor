using MediatR;
using TaskManagementSystem.Api.Contracts.Organization;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Organization.Features.Sections;
using TaskManagementSystem.Modules.Organization.Features.Sections.ArchiveSection;
using TaskManagementSystem.Modules.Organization.Features.Sections.CreateSection;
using TaskManagementSystem.Modules.Organization.Features.Sections.GetSectionById;
using TaskManagementSystem.Modules.Organization.Features.Sections.ListSections;
using TaskManagementSystem.Modules.Organization.Features.Sections.UpdateSection;
using TaskManagementSystem.Modules.Organization.Features.Teams;
using TaskManagementSystem.Modules.Organization.Features.Teams.ArchiveTeam;
using TaskManagementSystem.Modules.Organization.Features.Teams.CreateTeam;
using TaskManagementSystem.Modules.Organization.Features.Teams.GetTeamById;
using TaskManagementSystem.Modules.Organization.Features.Teams.ListTeams;
using TaskManagementSystem.Modules.Organization.Features.Teams.UpdateTeam;

namespace TaskManagementSystem.Api.Endpoints.Organization;

public static class OrganizationEndpoints
{
    public static RouteGroupBuilder MapOrganizationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/organization").WithTags("Organization").RequireAuthorization();

        var teams = group.MapGroup("/teams");
        teams.MapGet("", ListTeamsAsync);
        teams.MapGet("/{id:int}", GetTeamByIdAsync);
        teams.MapPost("", CreateTeamAsync);
        teams.MapPut("/{id:int}", UpdateTeamAsync);
        teams.MapDelete("/{id:int}", ArchiveTeamAsync);

        var sections = group.MapGroup("/sections");
        sections.MapGet("", ListSectionsAsync);
        sections.MapGet("/{id:int}", GetSectionByIdAsync);
        sections.MapPost("", CreateSectionAsync);
        sections.MapPut("/{id:int}", UpdateSectionAsync);
        sections.MapDelete("/{id:int}", ArchiveSectionAsync);

        return group;
    }

    private static async Task<IResult> ListTeamsAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListTeamsQuery(), cancellationToken);
        return result.ToHttpResult(teams =>
            Results.Ok(teams.Select(MapTeamListItem).ToList()));
    }

    private static async Task<IResult> GetTeamByIdAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTeamByIdQuery(id), cancellationToken);
        return result.ToHttpResult(team => Results.Ok(MapTeamDetail(team)));
    }

    private static async Task<IResult> CreateTeamAsync(
        CreateTeamRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateTeamCommand(request.Name), cancellationToken);
        return result.ToHttpResult(team =>
            Results.Created($"/organization/teams/{team.Id}", MapTeamListItem(team)));
    }

    private static async Task<IResult> UpdateTeamAsync(
        int id,
        UpdateTeamRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateTeamCommand(id, request.Name), cancellationToken);
        return result.ToHttpResult(team => Results.Ok(MapTeamListItem(team)));
    }

    private static async Task<IResult> ArchiveTeamAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveTeamCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }

    private static async Task<IResult> ListSectionsAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListSectionsQuery(), cancellationToken);
        return result.ToHttpResult(sections =>
            Results.Ok(sections.Select(MapSectionListItem).ToList()));
    }

    private static async Task<IResult> GetSectionByIdAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSectionByIdQuery(id), cancellationToken);
        return result.ToHttpResult(section => Results.Ok(MapSectionDetail(section)));
    }

    private static async Task<IResult> CreateSectionAsync(
        CreateSectionRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateSectionCommand(request.Name, request.HeadId, request.TeamIds),
            cancellationToken);

        return result.ToHttpResult(section =>
            Results.Created($"/organization/sections/{section.Id}", MapSectionDetail(section)));
    }

    private static async Task<IResult> UpdateSectionAsync(
        int id,
        UpdateSectionRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateSectionCommand(id, request.Name, request.HeadId, request.TeamIds),
            cancellationToken);

        return result.ToHttpResult(section => Results.Ok(MapSectionDetail(section)));
    }

    private static async Task<IResult> ArchiveSectionAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveSectionCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }

    private static TeamListItemResponse MapTeamListItem(TeamListItemResult team) =>
        new()
        {
            Id = team.Id,
            Name = team.Name,
            Members = team.Members
        };

    private static TeamDetailResponse MapTeamDetail(TeamDetailResult team) =>
        new()
        {
            Id = team.Id,
            Name = team.Name,
            Members = team.Members
                .Select(member => new TeamMemberResponse { Id = member.Id, Name = member.Name })
                .ToList()
        };

    private static SectionListItemResponse MapSectionListItem(SectionListItemResult section) =>
        new()
        {
            Id = section.Id,
            Name = section.Name
        };

    private static SectionDetailResponse MapSectionDetail(SectionDetailResult section) =>
        new()
        {
            Id = section.Id,
            Name = section.Name,
            Head = new IdNameResponse { Id = section.Head.Id, Name = section.Head.Name },
            Teams = section.Teams
                .Select(team => new SectionTeamResponse { Id = team.Id, Name = team.Name })
                .ToList()
        };
}

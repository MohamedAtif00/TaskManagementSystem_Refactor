using MediatR;
using TaskManagementSystem.Api.Contracts.Ticket;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.Ticket.Features;
using TaskManagementSystem.Modules.Ticket.Features.Comments.AddComment;
using TaskManagementSystem.Modules.Ticket.Features.Comments.ListCommentsByTicket;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.AssignTicket;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.CompleteTicket;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.CreateTicket;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.GetTicketById;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsByLearningObjective;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsBySubject;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.ProceedTicket;
using TaskManagementSystem.Modules.Ticket.Features.WorkTimes.StartWorkTime;
using TaskManagementSystem.Modules.Ticket.Features.WorkTimes.StopWorkTime;

namespace TaskManagementSystem.Api.Endpoints.Ticket;

public static class TicketEndpoints
{
    public static RouteGroupBuilder MapTicketEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/tickets").WithTags("Tickets").RequireAuthorization();

        group.MapPost("", CreateTicketAsync);
        group.MapGet("/{id:int}", GetTicketByIdAsync);
        group.MapPatch("/{id:int}/assign", AssignTicketAsync);
        group.MapPatch("/{id:int}/proceed", ProceedTicketAsync);
        group.MapPatch("/{id:int}/complete", CompleteTicketAsync);
        group.MapPost("/{id:int}/comments", AddCommentAsync);
        group.MapGet("/{id:int}/comments", ListCommentsByTicketAsync);
        group.MapPost("/{id:int}/work-times/start", StartWorkTimeAsync);
        group.MapPost("/{id:int}/work-times/stop", StopWorkTimeAsync);

        app.MapGet("/subjects/{subjectId:int}/tickets", ListTicketsBySubjectAsync)
            .WithTags("Tickets")
            .RequireAuthorization();

        app.MapGet("/learning-objectives/{loId:int}/tickets", ListTicketsByLearningObjectiveAsync)
            .WithTags("Tickets")
            .RequireAuthorization();

        return group;
    }

    private static async Task<IResult> CreateTicketAsync(
        CreateTicketRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateTicketCommand(request.LearningObjectiveId, request.TaskBankItemId, request.UserId),
            cancellationToken);

        return result.ToHttpResult(ticket =>
            Results.Created($"/tickets/{ticket.Id}", MapTicketDetail(ticket)));
    }

    private static async Task<IResult> GetTicketByIdAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTicketByIdQuery(id), cancellationToken);
        return result.ToHttpResult(ticket => Results.Ok(MapTicketDetail(ticket)));
    }

    private static async Task<IResult> ListTicketsBySubjectAsync(
        int subjectId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListTicketsBySubjectQuery(subjectId), cancellationToken);
        return result.ToHttpResult(tickets =>
            Results.Ok(tickets.Select(MapTicketListItem).ToList()));
    }

    private static async Task<IResult> ListTicketsByLearningObjectiveAsync(
        int loId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListTicketsByLearningObjectiveQuery(loId), cancellationToken);
        return result.ToHttpResult(tickets =>
            Results.Ok(tickets.Select(MapTicketListItem).ToList()));
    }

    private static async Task<IResult> AssignTicketAsync(
        int id,
        AssignTicketRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new AssignTicketCommand(id, request.UserId), cancellationToken);
        return result.ToHttpResult(ticket => Results.Ok(MapTicketDetail(ticket)));
    }

    private static async Task<IResult> ProceedTicketAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ProceedTicketCommand(id), cancellationToken);
        return result.ToHttpResult(ticket => Results.Ok(MapTicketDetail(ticket)));
    }

    private static async Task<IResult> CompleteTicketAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CompleteTicketCommand(id), cancellationToken);
        return result.ToHttpResult(ticket => Results.Ok(MapTicketDetail(ticket)));
    }

    private static async Task<IResult> AddCommentAsync(
        int id,
        AddCommentRequest request,
        ICurrentUserAccessor currentUserAccessor,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();
        var result = await mediator.Send(new AddCommentCommand(id, userId, request.Content), cancellationToken);
        return result.ToHttpResult(comment => Results.Created($"/tickets/{id}/comments/{comment.Id}", MapComment(comment)));
    }

    private static async Task<IResult> ListCommentsByTicketAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListCommentsByTicketQuery(id), cancellationToken);
        return result.ToHttpResult(comments =>
            Results.Ok(comments.Select(MapComment).ToList()));
    }

    private static async Task<IResult> StartWorkTimeAsync(
        int id,
        ICurrentUserAccessor currentUserAccessor,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();
        var result = await mediator.Send(new StartWorkTimeCommand(id, userId), cancellationToken);
        return result.ToHttpResult(workTime => Results.Ok(MapWorkTime(workTime)));
    }

    private static async Task<IResult> StopWorkTimeAsync(
        int id,
        ICurrentUserAccessor currentUserAccessor,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();
        var result = await mediator.Send(new StopWorkTimeCommand(id, userId), cancellationToken);
        return result.ToHttpResult(workTime => Results.Ok(MapWorkTime(workTime)));
    }

    private static TicketListItemResponse MapTicketListItem(TicketListItemResult ticket) =>
        new()
        {
            Id = ticket.Id,
            Name = ticket.Name,
            Status = ticket.Status,
            Priority = ticket.Priority,
            Duration = ticket.Duration,
            CreatedAt = ticket.CreatedAt,
            LearningObjectiveId = ticket.LearningObjectiveId,
            StepId = ticket.StepId,
            UserId = ticket.UserId,
            TeamId = ticket.TeamId
        };

    private static TicketDetailResponse MapTicketDetail(TicketDetailResult ticket) =>
        new()
        {
            Id = ticket.Id,
            Name = ticket.Name,
            Status = ticket.Status,
            Priority = ticket.Priority,
            Duration = ticket.Duration,
            CreatedAt = ticket.CreatedAt,
            Pause = ticket.Pause,
            Attention = ticket.Attention,
            Flagged = ticket.Flagged,
            Tl = ticket.Tl,
            IsReview = ticket.IsReview,
            IsRollback = ticket.IsRollback,
            RollbackCount = ticket.RollbackCount,
            LearningObjectiveId = ticket.LearningObjectiveId,
            StepId = ticket.StepId,
            UserId = ticket.UserId,
            TeamId = ticket.TeamId,
            FromId = ticket.FromId
        };

    private static CommentListItemResponse MapComment(CommentListItemResult comment) =>
        new()
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            Timestamp = comment.Timestamp,
            UserId = comment.UserId,
            LearningObjectiveId = comment.LearningObjectiveId,
            TaskId = comment.TaskId
        };

    private static TaskWorkTimeResponse MapWorkTime(TaskWorkTimeResult workTime) =>
        new()
        {
            Id = workTime.Id,
            StartDate = workTime.StartDate,
            EndDate = workTime.EndDate,
            Duration = workTime.Duration,
            TaskId = workTime.TaskId,
            UserId = workTime.UserId
        };
}

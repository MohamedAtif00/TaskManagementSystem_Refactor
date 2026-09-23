using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Endpoints.Curriculum;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.CreateLearningObjective;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.CreateTicket;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Lessons;

/// <summary>
/// Creates a learning objective under a lesson.
/// </summary>
public static class CreateLearningObjectiveEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder lessons)
    {
        lessons.MapPost("/{lessonId:int}/learning-objectives", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Create);
        return lessons;
    }

    private static async Task<IResult> HandleAsync(
        int lessonId,
        CreateLearningObjectiveRequest request,
        IMediator mediator,
        IWorkflowStepLookup workflowStepLookup,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateLearningObjectiveCommand(lessonId, request.SchemaId, request.Name, request.Tag, request.Template, request.Environment),
            cancellationToken);
        if (result.IsFailure)
        {
            return result.ToHttpResult(objective => Results.Created(
                $"/curriculum/learning-objectives/{objective.Id}",
                CurriculumMapping.MapLearningObjectiveDetail(objective)));
        }

        var startSteps = await workflowStepLookup.ListStartStepsAsync(result.Value.SchemaId, cancellationToken);
        foreach (var step in startSteps)
        {
            var ticketResult = await mediator.Send(
                new CreateTicketCommand(result.Value.Id, step.TicketBankId, null),
                cancellationToken);
            if (ticketResult.IsFailure)
            {
                return ResultHttpMapper.ToProblemResult(ticketResult.Error);
            }
        }

        return result.ToHttpResult(objective => Results.Created(
            $"/curriculum/learning-objectives/{objective.Id}",
            CurriculumMapping.MapLearningObjectiveDetail(objective)));
    }
}

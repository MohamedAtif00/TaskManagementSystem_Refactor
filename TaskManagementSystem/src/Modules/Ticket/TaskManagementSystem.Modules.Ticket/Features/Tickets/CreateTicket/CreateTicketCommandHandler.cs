using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.CreateTicket;

public sealed class CreateTicketCommandHandler(
    ITicketUnitOfWork unitOfWork,
    ILearningObjectiveLookup learningObjectiveLookup,
    ITicketBankLookup taskBankLookup,
    IWorkflowStepLookup workflowStepLookup,
    IIdentityUserLookup identityUserLookup,
    IOrganizationTeamLookup organizationTeamLookup,
    ITicketActivityWriter activityWriter,
    IRealtimePublisher realtimePublisher)
    : IRequestHandler<CreateTicketCommand, Result<TicketDetailResult>>
{
    public async Task<Result<TicketDetailResult>> Handle(
        CreateTicketCommand request,
        CancellationToken cancellationToken)
    {
        var learningObjective = await learningObjectiveLookup.GetActiveByIdAsync(
            request.LearningObjectiveId,
            cancellationToken);
        if (learningObjective is null)
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.LearningObjectiveNotFound);
        }

        var taskBank = await taskBankLookup.GetActiveByIdAsync(request.TicketBankItemId, cancellationToken);
        if (taskBank is null)
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.TicketBankNotFound);
        }

        if (!await organizationTeamLookup.ActiveTeamExistsAsync(taskBank.TeamId, cancellationToken))
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.TeamInvalid);
        }

        if (request.UserId is int userId)
        {
            if (!await identityUserLookup.ActiveUserExistsAsync(userId, cancellationToken))
            {
                return Result.Fail<TicketDetailResult>(TicketErrors.UserNotFound);
            }
        }

        var firstStep = await workflowStepLookup.GetFirstStepAsync(
            learningObjective.SchemaId,
            request.TicketBankItemId,
            cancellationToken);
        if (firstStep is null)
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.StepNotFound);
        }

        var priority = Enum.IsDefined(typeof(TicketPriority), firstStep.Priority)
            ? (TicketPriority)firstStep.Priority
            : TicketPriority.None;

        var createResult = Domain.Ticket.Create(
            taskBank.Name,
            firstStep.Duration,
            priority,
            request.LearningObjectiveId,
            firstStep.Id,
            taskBank.TeamId,
            taskBank.TeamLeaderOnly,
            request.UserId,
            DateTime.UtcNow);

        if (!createResult.IsSuccess)
        {
            return Result.Fail<TicketDetailResult>(createResult.Error);
        }

        await unitOfWork.Tickets.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        await activityWriter.WriteAsync(
            createResult.Value.Id,
            TicketActivityType.Created,
            request.UserId,
            cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        await TicketRealtimeNotifier.PublishUpdateAsync(realtimePublisher, createResult.Value, cancellationToken);

        return Result.Ok(TicketDetailResult.From(createResult.Value));
    }
}


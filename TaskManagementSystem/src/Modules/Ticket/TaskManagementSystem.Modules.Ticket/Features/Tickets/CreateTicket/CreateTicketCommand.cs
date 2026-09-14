using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.CreateTicket;

public sealed record CreateTicketCommand(
    int LearningObjectiveId,
    int TaskBankItemId,
    int? UserId) : ICommand<Result<TicketDetailResult>>;

public sealed class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator()
    {
        RuleFor(x => x.LearningObjectiveId).GreaterThan(0);
        RuleFor(x => x.TaskBankItemId).GreaterThan(0);
    }
}

public sealed class CreateTicketCommandHandler(
    ITicketUnitOfWork unitOfWork,
    ILearningObjectiveLookup learningObjectiveLookup,
    ITaskBankLookup taskBankLookup,
    IWorkflowStepLookup workflowStepLookup,
    IIdentityUserLookup identityUserLookup,
    IOrganizationTeamLookup organizationTeamLookup,
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

        var taskBank = await taskBankLookup.GetActiveByIdAsync(request.TaskBankItemId, cancellationToken);
        if (taskBank is null)
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.TaskBankNotFound);
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
            request.TaskBankItemId,
            cancellationToken);
        if (firstStep is null)
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.StepNotFound);
        }

        var priority = Enum.IsDefined(typeof(TaskPriority), firstStep.Priority)
            ? (TaskPriority)firstStep.Priority
            : TaskPriority.None;

        var createResult = TicketTask.Create(
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

        await unitOfWork.TicketTasks.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        await TicketRealtimeNotifier.PublishUpdateAsync(realtimePublisher, createResult.Value, cancellationToken);

        return Result.Ok(TicketDetailResult.From(createResult.Value));
    }
}

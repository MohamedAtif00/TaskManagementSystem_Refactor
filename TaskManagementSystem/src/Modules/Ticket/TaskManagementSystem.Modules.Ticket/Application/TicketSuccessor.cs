using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Application;

public static class TicketSuccessor
{
    private const int ReviewBankType = 1;

    public static async Task<IReadOnlyList<Domain.Ticket>> OpenNextAsync(
        ITicketUnitOfWork unitOfWork,
        IWorkflowStepLookup workflowStepLookup,
        ITicketBankLookup ticketBankLookup,
        Domain.Ticket completed,
        CancellationToken cancellationToken)
    {
        if (completed.StepId is not int currentStepId)
        {
            return [];
        }

        var opened = new List<Domain.Ticket>();
        var nextStep = await workflowStepLookup.GetNextStepInNodeAsync(currentStepId, cancellationToken);
        if (nextStep is not null)
        {
            await OpenStepAsync(unitOfWork, ticketBankLookup, completed, nextStep, opened, cancellationToken);
            return opened;
        }

        var currentStep = await workflowStepLookup.GetActiveByIdAsync(currentStepId, cancellationToken);
        if (currentStep is null)
        {
            return opened;
        }

        var nextNodeIds = await workflowStepLookup.ListNextNodeIdsAsync(currentStep.NodeId, cancellationToken);
        foreach (var nextNodeId in nextNodeIds)
        {
            if (!await PreviousNodesAreDoneAsync(
                    unitOfWork,
                    workflowStepLookup,
                    completed.LearningObjectiveId,
                    nextNodeId,
                    cancellationToken))
            {
                continue;
            }

            var firstStep = await workflowStepLookup.GetFirstStepInNodeAsync(nextNodeId, cancellationToken);
            if (firstStep is null)
            {
                break;
            }

            await OpenStepAsync(unitOfWork, ticketBankLookup, completed, firstStep, opened, cancellationToken);
        }

        return opened;
    }

    private static async Task<bool> PreviousNodesAreDoneAsync(
        ITicketUnitOfWork unitOfWork,
        IWorkflowStepLookup workflowStepLookup,
        int learningObjectiveId,
        int nextNodeId,
        CancellationToken cancellationToken)
    {
        var previousNodeIds = await workflowStepLookup.ListPreviousNodeIdsAsync(nextNodeId, cancellationToken);
        foreach (var previousNodeId in previousNodeIds)
        {
            var lastStep = await workflowStepLookup.GetLastStepInNodeAsync(previousNodeId, cancellationToken);
            if (lastStep is null)
            {
                continue;
            }

            var lastTickets = await unitOfWork.Tickets.ListActiveByStepTrackedAsync(
                lastStep.Id,
                learningObjectiveId,
                cancellationToken);
            if (lastTickets.Count == 0 || lastTickets.Any(ticket => ticket.Status != Domain.TicketStatus.Done))
            {
                return false;
            }
        }

        return true;
    }

    private static async Task OpenStepAsync(
        ITicketUnitOfWork unitOfWork,
        ITicketBankLookup ticketBankLookup,
        Domain.Ticket completed,
        WorkflowStepSummary step,
        List<Domain.Ticket> opened,
        CancellationToken cancellationToken)
    {
        var existing = await unitOfWork.Tickets.ListActiveByStepTrackedAsync(
            step.Id,
            completed.LearningObjectiveId,
            cancellationToken);
        if (existing.Count > 0)
        {
            foreach (var ticket in existing)
            {
                ticket.Reactivate();
                opened.Add(ticket);
            }

            return;
        }

        var bank = await ticketBankLookup.GetActiveByIdAsync(step.TicketBankId, cancellationToken);
        if (bank is null || bank.TeamId <= 0)
        {
            return;
        }

        var priority = Enum.IsDefined(typeof(Domain.TicketPriority), step.Priority)
            ? (Domain.TicketPriority)step.Priority
            : Domain.TicketPriority.None;
        var created = Domain.Ticket.Create(
            bank.Name,
            step.Duration,
            priority,
            completed.LearningObjectiveId,
            step.Id,
            bank.TeamId,
            bank.TeamLeaderOnly,
            userId: null,
            DateTime.UtcNow);
        if (!created.IsSuccess)
        {
            return;
        }

        created.Value.PrepareSuccessor(bank.TeamLeaderOnly, bank.Type == ReviewBankType, completed.FromId);
        await unitOfWork.Tickets.AddAsync(created.Value, cancellationToken);
        opened.Add(created.Value);
    }
}

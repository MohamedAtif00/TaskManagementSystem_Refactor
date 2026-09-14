namespace TaskManagementSystem.Modules.Ticket.Application;

public interface IWorkflowStepLookup
{
    Task<WorkflowStepSummary?> GetFirstStepAsync(
        int schemaId,
        int taskBankId,
        CancellationToken cancellationToken = default);

    Task<int?> GetNextStepIdAsync(
        int schemaId,
        int currentStepId,
        CancellationToken cancellationToken = default);

    Task<WorkflowStepSummary?> GetActiveByIdAsync(int stepId, CancellationToken cancellationToken = default);
}

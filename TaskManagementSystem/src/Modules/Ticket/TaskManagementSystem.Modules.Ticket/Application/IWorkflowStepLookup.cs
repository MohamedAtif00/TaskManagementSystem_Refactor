namespace TaskManagementSystem.Modules.Ticket.Application;

public interface IWorkflowStepLookup
{
    Task<WorkflowStepSummary?> GetFirstStepAsync(
        int schemaId,
        int taskBankId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkflowStepSummary>> ListStartStepsAsync(
        int schemaId,
        CancellationToken cancellationToken = default);

    Task<int?> GetNextStepIdAsync(
        int schemaId,
        int currentStepId,
        CancellationToken cancellationToken = default);

    Task<WorkflowStepSummary?> GetActiveByIdAsync(int stepId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkflowJumpPoint>> ListStepsAheadAsync(
        int schemaId,
        int currentStepId,
        CancellationToken cancellationToken = default);

    Task<bool> IsStepAheadAsync(
        int schemaId,
        int currentStepId,
        int targetStepId,
        CancellationToken cancellationToken = default);

    Task<WorkflowStepSummary?> GetNextStepInNodeAsync(
        int currentStepId,
        CancellationToken cancellationToken = default);

    Task<WorkflowStepSummary?> GetFirstStepInNodeAsync(
        int nodeId,
        CancellationToken cancellationToken = default);

    Task<WorkflowStepSummary?> GetLastStepInNodeAsync(
        int nodeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> ListNextNodeIdsAsync(
        int nodeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> ListPreviousNodeIdsAsync(
        int nodeId,
        CancellationToken cancellationToken = default);
}

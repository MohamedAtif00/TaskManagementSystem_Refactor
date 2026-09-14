namespace TaskManagementSystem.Modules.Ticket.Application;

public interface ILearningObjectiveLookup
{
    Task<LearningObjectiveSummary?> GetActiveByIdAsync(int learningObjectiveId, CancellationToken cancellationToken = default);
}

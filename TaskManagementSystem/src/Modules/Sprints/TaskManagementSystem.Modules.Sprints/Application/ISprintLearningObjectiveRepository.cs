using TaskManagementSystem.Modules.Sprints.Domain;

namespace TaskManagementSystem.Modules.Sprints.Application;

public interface ISprintLearningObjectiveRepository
{
    Task<IReadOnlyList<int>> ListLearningObjectiveIdsBySprintIdAsync(
        int sprintId,
        CancellationToken cancellationToken = default);

    Task AddLearningObjectivesAsync(
        int sprintId,
        IReadOnlyCollection<int> learningObjectiveIds,
        CancellationToken cancellationToken = default);

    Task RemoveLearningObjectiveAsync(
        int sprintId,
        int learningObjectiveId,
        CancellationToken cancellationToken = default);
}

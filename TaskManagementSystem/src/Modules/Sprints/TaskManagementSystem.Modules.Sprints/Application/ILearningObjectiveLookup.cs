namespace TaskManagementSystem.Modules.Sprints.Application;

public interface ILearningObjectiveLookup
{
    Task<bool> ActiveLearningObjectivesExistAsync(
        IReadOnlyCollection<int> learningObjectiveIds,
        CancellationToken cancellationToken = default);
}

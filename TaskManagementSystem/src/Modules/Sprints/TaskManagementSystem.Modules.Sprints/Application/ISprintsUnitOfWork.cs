using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.Modules.Sprints.Application;

public interface ISprintsUnitOfWork : IUnitOfWork
{
    ISprintRepository Sprints { get; }
    ISprintLearningObjectiveRepository SprintLearningObjectives { get; }
}

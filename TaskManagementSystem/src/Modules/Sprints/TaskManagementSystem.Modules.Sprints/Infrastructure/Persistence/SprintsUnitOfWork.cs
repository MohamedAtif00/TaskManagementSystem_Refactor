using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.BuildingBlocks.Persistence.Events;
using TaskManagementSystem.Modules.Sprints.Application;

namespace TaskManagementSystem.Modules.Sprints.Infrastructure.Persistence;

internal sealed class SprintsUnitOfWork(
    SprintsDbContext context,
    IDomainEventDispatcher domainEventDispatcher)
    : UnitOfWork<SprintsDbContext>(context, domainEventDispatcher), ISprintsUnitOfWork
{
    private readonly Lazy<SprintRepository> _sprints =
        LazyRepositoryFactory.Create(() => new SprintRepository(context));

    private readonly Lazy<SprintLearningObjectiveRepository> _sprintLearningObjectives =
        LazyRepositoryFactory.Create(() => new SprintLearningObjectiveRepository(context));

    public ISprintRepository Sprints => _sprints.Value;
    public ISprintLearningObjectiveRepository SprintLearningObjectives => _sprintLearningObjectives.Value;
}

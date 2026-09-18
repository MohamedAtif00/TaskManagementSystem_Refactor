using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Modules.Sprints.Application;
using TaskManagementSystem.Modules.Sprints.Domain;

namespace TaskManagementSystem.Modules.Sprints.Infrastructure.Persistence;

internal sealed class SprintLearningObjectiveRepository(SprintsDbContext context) : ISprintLearningObjectiveRepository
{
    public async Task<IReadOnlyList<int>> ListLearningObjectiveIdsBySprintIdAsync(
        int sprintId,
        CancellationToken cancellationToken = default) =>
        await context.SprintLearningObjectives.AsNoTracking()
            .Where(link => link.SprintId == sprintId)
            .OrderBy(link => link.LearningObjectiveId)
            .Select(link => link.LearningObjectiveId)
            .ToListAsync(cancellationToken);

    public async Task AddLearningObjectivesAsync(
        int sprintId,
        IReadOnlyCollection<int> learningObjectiveIds,
        CancellationToken cancellationToken = default)
    {
        if (learningObjectiveIds.Count == 0)
        {
            return;
        }

        var existingIds = await context.SprintLearningObjectives.AsNoTracking()
            .Where(link => link.SprintId == sprintId)
            .Select(link => link.LearningObjectiveId)
            .ToListAsync(cancellationToken);

        foreach (var learningObjectiveId in learningObjectiveIds.Distinct().Except(existingIds))
        {
            await context.SprintLearningObjectives.AddAsync(
                SprintLearningObjective.Create(sprintId, learningObjectiveId),
                cancellationToken);
        }
    }

    public async Task RemoveLearningObjectiveAsync(
        int sprintId,
        int learningObjectiveId,
        CancellationToken cancellationToken = default)
    {
        var link = await context.SprintLearningObjectives
            .FirstOrDefaultAsync(
                item => item.SprintId == sprintId && item.LearningObjectiveId == learningObjectiveId,
                cancellationToken);

        if (link is not null)
        {
            context.SprintLearningObjectives.Remove(link);
        }
    }
}

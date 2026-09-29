using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Workflows.Application;

internal static class ReorderValidation
{
    public static Result<NoValue> Validate(IReadOnlyList<int> orderedIds, IReadOnlyCollection<int> existingIds)
    {
        if (orderedIds.Count != existingIds.Count)
        {
            return Result.Fail<NoValue>(WorkflowsErrors.InvalidReorder);
        }

        if (orderedIds.Distinct().Count() != orderedIds.Count)
        {
            return Result.Fail<NoValue>(WorkflowsErrors.InvalidReorder);
        }

        var existing = existingIds.ToHashSet();
        if (orderedIds.Any(id => !existing.Contains(id)))
        {
            return Result.Fail<NoValue>(WorkflowsErrors.InvalidReorder);
        }

        return Result.Ok();
    }
}

using TaskManagementSystem.Api.Contracts.Sprints;
using TaskManagementSystem.Modules.Sprints.Features;

namespace TaskManagementSystem.Api.Endpoints.Sprints;

internal static class SprintMapping
{
    internal static SprintListItemResponse MapSprintListItem(SprintListItemResult sprint) =>
        new()
        {
            Id = sprint.Id,
            Name = sprint.Name,
            Description = sprint.Description,
            StartDate = sprint.StartDate,
            EndDate = sprint.EndDate
        };

    internal static SprintDetailResponse MapSprintDetail(SprintDetailResult sprint) =>
        new()
        {
            Id = sprint.Id,
            Name = sprint.Name,
            Description = sprint.Description,
            StartDate = sprint.StartDate,
            EndDate = sprint.EndDate,
            LearningObjectiveIds = sprint.LearningObjectiveIds
        };
}

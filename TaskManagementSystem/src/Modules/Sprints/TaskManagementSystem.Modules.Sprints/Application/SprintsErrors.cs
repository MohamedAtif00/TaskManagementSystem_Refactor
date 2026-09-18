using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Sprints.Application;

public static class SprintsErrors
{
    public static ResultError SprintNotFound =>
        new("sprint_not_found", "Sprint not found.");

    public static ResultError LearningObjectiveNotFound =>
        new("learning_objective_not_found", "Learning objective not found.");

    public static ResultError SprintLearningObjectiveNotFound =>
        new("sprint_learning_objective_not_found", "Sprint learning objective link not found.");
}

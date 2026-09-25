using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Analytics.Application;

public static class AnalyticsErrors
{
    public static ResultError SubjectNotFound =>
        new("subject_not_found", "Subject not found.");

    public static ResultError SprintNotFound =>
        new("sprint_not_found", "Sprint not found.");
}

using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Organization.Application;

public static class OrganizationErrors
{
    public static ResultError TeamNotFound =>
        new("team_not_found", "Team not found.");

    public static ResultError SectionNotFound =>
        new("section_not_found", "Section not found.");

    public static ResultError SectionAlreadyExists =>
        new("section_already_exists", "Section already exists.");

    public static ResultError UserNotFound =>
        new("user_not_found", "User not found.");

    public static ResultError TeamInvalid =>
        new("team_invalid", "One or more teams are invalid.");
}

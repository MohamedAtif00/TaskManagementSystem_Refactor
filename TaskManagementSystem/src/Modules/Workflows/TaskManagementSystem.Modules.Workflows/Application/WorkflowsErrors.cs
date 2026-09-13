using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Workflows.Application;

public static class WorkflowsErrors
{
    public static ResultError SchemaNotFound =>
        new("schema_not_found", "Schema not found.");

    public static ResultError SchemaTypeNotFound =>
        new("schema_type_not_found", "Schema type not found.");

    public static ResultError TaskBankNotFound =>
        new("task_bank_not_found", "Task bank item not found.");

    public static ResultError NodeNotFound =>
        new("node_not_found", "Node not found.");

    public static ResultError StepNotFound =>
        new("step_not_found", "Step not found.");

    public static ResultError TeamInvalid =>
        new("team_invalid", "Team not found.");
}

using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Workflows.Application;

public static class WorkflowsErrors
{
    public static ResultError SchemaNotFound =>
        new("schema_not_found", "Schema not found.");

    public static ResultError SchemaTypeNotFound =>
        new("schema_type_not_found", "Schema type not found.");

    public static ResultError TicketBankNotFound =>
        new("ticket_bank_not_found", "Ticket bank item not found.");

    public static ResultError NodeNotFound =>
        new("node_not_found", "Node not found.");

    public static ResultError StepNotFound =>
        new("step_not_found", "Step not found.");

    public static ResultError TeamInvalid =>
        new("team_invalid", "Team not found.");

    public static ResultError InvalidReorder =>
        new("invalid_reorder", "Reorder request must include every active item exactly once.");

    public static ResultError PredecessorInvalid =>
        new("predecessor_invalid", "A predecessor must be another active node in the same workflow.");
}

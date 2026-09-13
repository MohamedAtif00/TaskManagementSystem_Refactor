using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Features;

public sealed record SchemaTypeResult(int Id, string Name, string Description)
{
    public static SchemaTypeResult From(SchemaType type) => new(type.Id, type.Name, type.Description);
}

public sealed record SchemaListItemResult(int Id, string Name, string Description, int? TypeId)
{
    public static SchemaListItemResult From(WorkflowSchema schema) =>
        new(schema.Id, schema.Name, schema.Description, schema.TypeId);
}

public sealed record SchemaDetailResult(int Id, string Name, string Description, int? TypeId)
{
    public static SchemaDetailResult From(WorkflowSchema schema) =>
        new(schema.Id, schema.Name, schema.Description, schema.TypeId);
}

public sealed record TaskBankListItemResult(
    int Id,
    string Name,
    int Duration,
    TaskBankType Type,
    bool TeamLeaderOnly,
    int TeamId)
{
    public static TaskBankListItemResult From(TaskBankItem item) =>
        new(item.Id, item.Name, item.Duration, item.Type, item.TeamLeaderOnly, item.TeamId);
}

public sealed record NodeListItemResult(int Id, string Name, int Order, bool IsStart, bool IsEnd, int SchemaId)
{
    public static NodeListItemResult From(WorkflowNode node) =>
        new(node.Id, node.Name, node.Order, node.IsStart, node.IsEnd, node.SchemaId);
}

public sealed record StepListItemResult(
    int Id,
    int Order,
    int Duration,
    int Priority,
    int NodeId,
    int TaskBankId)
{
    public static StepListItemResult From(WorkflowStep step) =>
        new(step.Id, step.Order, step.Duration, step.Priority, step.NodeId, step.TaskBankId);
}

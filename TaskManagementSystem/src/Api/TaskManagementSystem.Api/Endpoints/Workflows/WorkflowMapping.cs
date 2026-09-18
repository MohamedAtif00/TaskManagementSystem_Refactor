using TaskManagementSystem.Api.Contracts.Workflows;
using TaskManagementSystem.Modules.Workflows.Features;

namespace TaskManagementSystem.Api.Endpoints.Workflows;

internal static class WorkflowMapping
{
    internal static SchemaTypeResponse MapSchemaType(SchemaTypeResult type) =>
        new()
        {
            Id = type.Id,
            Name = type.Name,
            Description = type.Description
        };

    internal static SchemaListItemResponse MapSchemaListItem(SchemaListItemResult schema) =>
        new()
        {
            Id = schema.Id,
            Name = schema.Name,
            Description = schema.Description,
            TypeId = schema.TypeId
        };

    internal static SchemaDetailResponse MapSchemaDetail(SchemaDetailResult schema) =>
        new()
        {
            Id = schema.Id,
            Name = schema.Name,
            Description = schema.Description,
            TypeId = schema.TypeId
        };

    internal static TaskBankListItemResponse MapTaskBankListItem(TaskBankListItemResult item) =>
        new()
        {
            Id = item.Id,
            Name = item.Name,
            Duration = item.Duration,
            Type = item.Type,
            TeamLeaderOnly = item.TeamLeaderOnly,
            TeamId = item.TeamId
        };

    internal static NodeListItemResponse MapNodeListItem(NodeListItemResult node) =>
        new()
        {
            Id = node.Id,
            Name = node.Name,
            Order = node.Order,
            IsStart = node.IsStart,
            IsEnd = node.IsEnd,
            SchemaId = node.SchemaId
        };

    internal static StepListItemResponse MapStepListItem(StepListItemResult step) =>
        new()
        {
            Id = step.Id,
            Order = step.Order,
            Duration = step.Duration,
            Priority = step.Priority,
            NodeId = step.NodeId,
            TaskBankId = step.TaskBankId
        };
}

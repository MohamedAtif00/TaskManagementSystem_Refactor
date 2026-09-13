using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Workflows.Domain;

public sealed class WorkflowNode : Entity, IAggregateRoot
{
    private WorkflowNode()
    {
    }

    internal static WorkflowNode CreateForPersistence() => new();

    public int Id { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public int Order { get; internal set; }
    public bool IsStart { get; internal set; }
    public bool IsEnd { get; internal set; }
    public bool Archived { get; internal set; }
    public int SchemaId { get; internal set; }

    public static Result<WorkflowNode> Create(
        string name,
        int order,
        bool isStart,
        bool isEnd,
        int schemaId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<WorkflowNode>(new ResultError("node_invalid_name", "Node name is required."));
        }

        if (schemaId <= 0)
        {
            return Result.Fail<WorkflowNode>(new ResultError("schema_not_found", "Schema not found."));
        }

        return Result.Ok(new WorkflowNode
        {
            Name = name.Trim(),
            Order = order,
            IsStart = isStart,
            IsEnd = isEnd,
            SchemaId = schemaId,
            Archived = false
        });
    }

    public Result<NoValue> Update(string name, bool isStart, bool isEnd)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<NoValue>(new ResultError("node_invalid_name", "Node name is required."));
        }

        Name = name.Trim();
        IsStart = isStart;
        IsEnd = isEnd;
        return Result.Ok();
    }

    public void Archive() => Archived = true;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

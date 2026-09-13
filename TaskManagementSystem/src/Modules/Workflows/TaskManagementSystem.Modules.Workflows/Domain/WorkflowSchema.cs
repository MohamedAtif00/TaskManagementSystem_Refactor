using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Workflows.Domain;

public sealed class WorkflowSchema : Entity, IAggregateRoot
{
    private WorkflowSchema()
    {
    }

    internal static WorkflowSchema CreateForPersistence() => new();

    public int Id { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public string Description { get; internal set; } = string.Empty;
    public bool Archived { get; internal set; }
    public int? TypeId { get; internal set; }

    public static Result<WorkflowSchema> Create(string name, string description, int? typeId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<WorkflowSchema>(new ResultError("schema_invalid_name", "Schema name is required."));
        }

        return Result.Ok(new WorkflowSchema
        {
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty,
            TypeId = typeId,
            Archived = false
        });
    }

    public Result<NoValue> Update(string name, string description, int? typeId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<NoValue>(new ResultError("schema_invalid_name", "Schema name is required."));
        }

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        TypeId = typeId;
        return Result.Ok();
    }

    public void Archive() => Archived = true;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

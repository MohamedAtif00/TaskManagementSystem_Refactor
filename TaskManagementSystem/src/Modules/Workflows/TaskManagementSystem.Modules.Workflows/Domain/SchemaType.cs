using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Workflows.Domain;

public sealed class SchemaType : Entity
{
    private SchemaType()
    {
    }

    public int Id { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public string Description { get; internal set; } = string.Empty;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

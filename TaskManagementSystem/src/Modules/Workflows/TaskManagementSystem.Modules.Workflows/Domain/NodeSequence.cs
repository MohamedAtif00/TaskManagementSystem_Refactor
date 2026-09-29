namespace TaskManagementSystem.Modules.Workflows.Domain;

public sealed class NodeSequence
{
    public int NextId { get; set; }

    public int PreviousId { get; set; }
}

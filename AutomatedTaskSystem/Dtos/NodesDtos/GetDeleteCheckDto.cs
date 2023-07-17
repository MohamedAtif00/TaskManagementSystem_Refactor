namespace AutomatedTaskSystem.Dtos.Nodes;

public class GetNodeDeleteCheckDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool isSafeToDelete { get; set; }
}

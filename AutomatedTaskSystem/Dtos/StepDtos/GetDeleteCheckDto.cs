namespace AutomatedTaskSystem.Dtos.Steps;

public class GetStepDeleteCheckDto
{
    public int Id { get; set; }
    public int NodeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool isSafeToDelete { get; set; }
}

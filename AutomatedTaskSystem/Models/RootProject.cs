namespace AutomatedTaskSystem.Models;

public class RootProject
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public List<ProjectYear> Years { get; set; } = new();
}

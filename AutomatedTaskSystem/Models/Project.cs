namespace AutomatedTaskSystem.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? LevelNamesJson { get; set; }
    public List<Folder> Folders { get; set; } = new();
}

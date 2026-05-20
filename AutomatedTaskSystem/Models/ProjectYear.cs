namespace AutomatedTaskSystem.Models;

public class ProjectYear
{
    public int Id { get; set; }
    public int RootProjectId { get; set; }
    public RootProject RootProject { get; set; } = null!;
    /// <summary>Academic year label, e.g. 2016/2017.</summary>
    public string Label { get; set; } = "";
    public List<ProjectTerm> Terms { get; set; } = new();
}

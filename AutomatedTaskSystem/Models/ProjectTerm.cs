namespace AutomatedTaskSystem.Models;

/// <summary>Term within a project year (e.g. Term1, Term2).</summary>
public class ProjectTerm
{
    public int Id { get; set; }
    public int ProjectYearId { get; set; }
    public ProjectYear ProjectYear { get; set; } = null!;
    public string Name { get; set; } = "";
    public int Order { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<Subject> Subjects { get; set; } = new();
}

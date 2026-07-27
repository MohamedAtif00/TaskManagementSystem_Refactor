namespace AutomatedTaskSystem.Models;



/// <summary>Term within a curriculum project.</summary>

public class CurriculumTerm

{

    public int Id { get; set; }

    public int ProjectId { get; set; }

    public CurriculumProject Project { get; set; } = null!;

    public string Name { get; set; } = "";

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public List<SubjectGroup> SubjectGroups { get; set; } = new();

}



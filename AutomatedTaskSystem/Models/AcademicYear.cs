namespace AutomatedTaskSystem.Models;



/// <summary>Top-level academic year (e.g. 2026/2027).</summary>

public class AcademicYear

{

    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string? Description { get; set; }

    public List<CurriculumProject> Projects { get; set; } = new();

}



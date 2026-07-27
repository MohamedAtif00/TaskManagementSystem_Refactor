namespace AutomatedTaskSystem.Models;



/// <summary>Project within an academic year (e.g. Selah Eltelmeez).</summary>

public class CurriculumProject

{

    public int Id { get; set; }

    public int YearId { get; set; }

    public AcademicYear Year { get; set; } = null!;

    public string Name { get; set; } = "";

    public string? Description { get; set; }

    public List<CurriculumTerm> Terms { get; set; } = new();

}



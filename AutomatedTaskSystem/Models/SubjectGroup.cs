namespace AutomatedTaskSystem.Models;



/// <summary>Grouping of related subjects (e.g. Arabic, Math).</summary>

public class SubjectGroup

{

    public int Id { get; set; }

    public int TermId { get; set; }

    public CurriculumTerm Term { get; set; } = null!;

    public string Name { get; set; } = "";

    public List<Subject> Subjects { get; set; } = new();

}



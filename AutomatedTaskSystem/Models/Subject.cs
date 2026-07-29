using System.ComponentModel.DataAnnotations;

using AutomatedTaskSystem.Models.Enums.ProjectStatus;



namespace AutomatedTaskSystem.Models;



public class Subject

{

    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public List<Unit> Units { get; set; } = new();

    public List<User> Users { get; set; } = new();

    public SubjectGroup SubjectGroup { get; set; } = null!;

    public int SubjectGroupId { get; set; }

    public bool Archived { get; set; } = false;

    /// <summary>True when archived as part of a folder (curriculum) delete, not direct subject delete.</summary>
    public bool ArchivedWithFolder { get; set; } = false;

    [Range(0, 3)]

    public ProjectStatusEnum Status { get; set; } = ProjectStatusEnum.Active;

}



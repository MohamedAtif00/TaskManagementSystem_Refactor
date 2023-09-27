using System.ComponentModel.DataAnnotations;
using AutomatedTaskSystem.Models.Enums.ProjectStatus;
using AutomatedTaskSystem.Models.YearModel;

namespace AutomatedTaskSystem.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<Unit> Units { get; set; } = new List<Unit> { };
    public List<User> Users { get; set; } = new List<User> { };
    public Year Year { get; set; } = new Year { };
    public int YearId { get; set; }
    public bool Term { get; set; }
    public bool Archived { get; set; } = false;
	[Range(0,3)]
    public ProjectStatusEnum Status { get; set; } = ProjectStatusEnum.Active;
}

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
    public Folder Folder { get; set; } = null!;
    public int FolderId { get; set; }
    public bool Archived { get; set; } = false;
    [Range(0, 3)]
    public ProjectStatusEnum Status { get; set; } = ProjectStatusEnum.Active;
}

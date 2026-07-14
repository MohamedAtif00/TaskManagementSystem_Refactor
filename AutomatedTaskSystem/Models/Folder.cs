namespace AutomatedTaskSystem.Models;

public class Folder
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public int? ParentFolderId { get; set; }
    public Folder? ParentFolder { get; set; }
    public List<Folder> Children { get; set; } = new();
    public List<Subject> Subjects { get; set; } = new();
}

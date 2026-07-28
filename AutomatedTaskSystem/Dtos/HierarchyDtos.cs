namespace AutomatedTaskSystem.DTO;

public class FolderDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int ProjectId { get; set; }
    public int? ParentFolderId { get; set; }
    public int Level { get; set; }
    public string LevelName { get; set; } = "";
    public string Path { get; set; } = "";
    public List<string> LevelNames { get; set; } = new();
}

public class FolderTreeDto : FolderDto
{
    public int SubjectCount { get; set; }
    public List<FolderTreeDto> Children { get; set; } = new();
}

public class CreateFolderRequestDto
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int? ParentFolderId { get; set; }
    public List<string>? LevelNames { get; set; }
}

public class UpdateFolderRequestDto
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public List<string>? LevelNames { get; set; }
}

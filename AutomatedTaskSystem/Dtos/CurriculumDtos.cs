namespace AutomatedTaskSystem.DTO;

public class CurriculumNodeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string LevelName { get; set; } = "";
    public int Depth { get; set; }
    public string Path { get; set; } = "";
    public int? ParentId { get; set; }
    public string NodeType { get; set; } = "";
}

public class CreateYearDto
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}

public class UpdateYearDto
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}

public class CreateProjectDto
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}

public class UpdateProjectDto
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}

public class CreateTermDto
{
    public string Name { get; set; } = "";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class UpdateTermDto
{
    public string Name { get; set; } = "";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class CreateSubjectGroupDto
{
    public string Name { get; set; } = "";
}

public class UpdateSubjectGroupDto
{
    public string Name { get; set; } = "";
}

public class ArchivedSubjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class ArchivedCurriculumTreeNodeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string NodeType { get; set; } = "";
    public string Path { get; set; } = "";
    public int Depth { get; set; }
    public bool Archived { get; set; }
    public List<ArchivedSubjectDto> Subjects { get; set; } = new();
    public List<ArchivedCurriculumTreeNodeDto> Children { get; set; } = new();
}

public class RestoreCurriculumDto
{
    public List<int>? SubjectIds { get; set; }
}

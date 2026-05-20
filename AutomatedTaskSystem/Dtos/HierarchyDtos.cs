namespace AutomatedTaskSystem.DTO;

public class RootProjectListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}

public class RootProjectDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}

public class ProjectYearDetailDto
{
    public int Id { get; set; }
    public int RootProjectId { get; set; }
    public string Label { get; set; } = "";
}

public class ProjectTermListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Order { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class ProjectTermDetailDto
{
    public int Id { get; set; }
    public int ProjectYearId { get; set; }
    public string Name { get; set; } = "";
    public int Order { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

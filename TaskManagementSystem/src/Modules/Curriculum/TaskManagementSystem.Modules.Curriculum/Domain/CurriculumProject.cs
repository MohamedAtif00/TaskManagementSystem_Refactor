using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Domain;

public sealed class CurriculumProject : Entity, IAggregateRoot
{
    private CurriculumProject()
    {
    }

    internal static CurriculumProject CreateForPersistence() => new();

    public int Id { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public string? Description { get; internal set; }
    public int YearId { get; internal set; }
    public bool Archived { get; internal set; }

    public static Result<CurriculumProject> Create(string name, string? description, int yearId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<CurriculumProject>(new ResultError("curriculum_project_invalid_name", "Project name is required."));
        }

        if (yearId <= 0)
        {
            return Result.Fail<CurriculumProject>(new ResultError("curriculum_project_invalid_year", "Year is required."));
        }

        return Result.Ok(new CurriculumProject
        {
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            YearId = yearId,
            Archived = false
        });
    }

    public Result<NoValue> Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<NoValue>(new ResultError("curriculum_project_invalid_name", "Project name is required."));
        }

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        return Result.Ok();
    }

    public Result<NoValue> Archive()
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("curriculum_project_already_archived", "Project is already archived."));
        }

        Archived = true;
        return Result.Ok();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

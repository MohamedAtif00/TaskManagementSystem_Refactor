using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Domain;

public sealed class AcademicYear : Entity, IAggregateRoot
{
    private AcademicYear()
    {
    }

    internal static AcademicYear CreateForPersistence() => new();

    public int Id { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public string? Description { get; internal set; }
    public bool Archived { get; internal set; }

    public static Result<AcademicYear> Create(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<AcademicYear>(new ResultError("academic_year_invalid_name", "Academic year name is required."));
        }

        return Result.Ok(new AcademicYear
        {
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Archived = false
        });
    }

    public Result<NoValue> Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<NoValue>(new ResultError("academic_year_invalid_name", "Academic year name is required."));
        }

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        return Result.Ok();
    }

    public Result<NoValue> Archive()
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("academic_year_already_archived", "Academic year is already archived."));
        }

        Archived = true;
        return Result.Ok();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

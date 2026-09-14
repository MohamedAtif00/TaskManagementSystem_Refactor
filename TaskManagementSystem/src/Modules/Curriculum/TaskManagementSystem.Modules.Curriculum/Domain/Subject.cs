using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Domain;

public sealed class Subject : Entity, IAggregateRoot
{
    private Subject()
    {
    }

    internal static Subject CreateForPersistence() => new();

    public int Id { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public string Description { get; internal set; } = string.Empty;
    public SubjectStatus Status { get; internal set; }
    public int SubjectGroupId { get; internal set; }
    public bool Archived { get; internal set; }
    public bool ArchivedWithFolder { get; internal set; }

    public static Result<Subject> Create(string name, string description, int subjectGroupId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<Subject>(new ResultError("subject_invalid_name", "Subject name is required."));
        }

        if (subjectGroupId <= 0)
        {
            return Result.Fail<Subject>(new ResultError("subject_invalid_group", "Subject group is required."));
        }

        return Result.Ok(new Subject
        {
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty,
            Status = SubjectStatus.Active,
            SubjectGroupId = subjectGroupId,
            Archived = false,
            ArchivedWithFolder = false
        });
    }

    public Result<NoValue> Update(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<NoValue>(new ResultError("subject_invalid_name", "Subject name is required."));
        }

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        return Result.Ok();
    }

    public Result<NoValue> UpdateStatus(SubjectStatus status)
    {
        Status = status;
        return Result.Ok();
    }

    public Result<NoValue> Archive()
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("subject_already_archived", "Subject is already archived."));
        }

        Archived = true;
        return Result.Ok();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

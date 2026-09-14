using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Domain;

public sealed class SubjectGroup : Entity, IAggregateRoot
{
    private SubjectGroup()
    {
    }

    internal static SubjectGroup CreateForPersistence() => new();

    public int Id { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public int TermId { get; internal set; }
    public bool Archived { get; internal set; }

    public static Result<SubjectGroup> Create(string name, int termId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<SubjectGroup>(new ResultError("subject_group_invalid_name", "Subject group name is required."));
        }

        if (termId <= 0)
        {
            return Result.Fail<SubjectGroup>(new ResultError("subject_group_invalid_term", "Term is required."));
        }

        return Result.Ok(new SubjectGroup
        {
            Name = name.Trim(),
            TermId = termId,
            Archived = false
        });
    }

    public Result<NoValue> Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<NoValue>(new ResultError("subject_group_invalid_name", "Subject group name is required."));
        }

        Name = name.Trim();
        return Result.Ok();
    }

    public Result<NoValue> Archive()
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("subject_group_already_archived", "Subject group is already archived."));
        }

        Archived = true;
        return Result.Ok();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

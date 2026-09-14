using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Domain;

public sealed class Unit : Entity, IAggregateRoot
{
    private Unit()
    {
    }

    internal static Unit CreateForPersistence() => new();

    public int Id { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public int SubjectId { get; internal set; }
    public bool Archived { get; internal set; }

    public static Result<Unit> Create(string name, int subjectId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<Unit>(new ResultError("unit_invalid_name", "Unit name is required."));
        }

        if (subjectId <= 0)
        {
            return Result.Fail<Unit>(new ResultError("unit_invalid_subject", "Subject is required."));
        }

        return Result.Ok(new Unit
        {
            Name = name.Trim(),
            SubjectId = subjectId,
            Archived = false
        });
    }

    public Result<NoValue> Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<NoValue>(new ResultError("unit_invalid_name", "Unit name is required."));
        }

        Name = name.Trim();
        return Result.Ok();
    }

    public Result<NoValue> Archive()
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("unit_already_archived", "Unit is already archived."));
        }

        Archived = true;
        return Result.Ok();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

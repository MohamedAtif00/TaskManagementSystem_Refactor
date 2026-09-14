using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Domain;

public sealed class Lesson : Entity, IAggregateRoot
{
    private Lesson()
    {
    }

    internal static Lesson CreateForPersistence() => new();

    public int Id { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public int UnitId { get; internal set; }
    public bool Archived { get; internal set; }

    public static Result<Lesson> Create(string name, int unitId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<Lesson>(new ResultError("lesson_invalid_name", "Lesson name is required."));
        }

        if (unitId <= 0)
        {
            return Result.Fail<Lesson>(new ResultError("lesson_invalid_unit", "Unit is required."));
        }

        return Result.Ok(new Lesson
        {
            Name = name.Trim(),
            UnitId = unitId,
            Archived = false
        });
    }

    public Result<NoValue> Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<NoValue>(new ResultError("lesson_invalid_name", "Lesson name is required."));
        }

        Name = name.Trim();
        return Result.Ok();
    }

    public Result<NoValue> Archive()
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("lesson_already_archived", "Lesson is already archived."));
        }

        Archived = true;
        return Result.Ok();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

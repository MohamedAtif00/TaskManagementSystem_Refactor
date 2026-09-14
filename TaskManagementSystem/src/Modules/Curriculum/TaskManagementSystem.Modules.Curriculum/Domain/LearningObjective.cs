using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Domain;

public sealed class LearningObjective : Entity, IAggregateRoot
{
    private LearningObjective()
    {
    }

    internal static LearningObjective CreateForPersistence() => new();

    public int Id { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public string Tag { get; internal set; } = string.Empty;
    public string Template { get; internal set; } = string.Empty;
    public string Environment { get; internal set; } = string.Empty;
    public DateTime CreateAt { get; internal set; }
    public DateTime? StartedAt { get; internal set; }
    public DateTime? DoneAt { get; internal set; }
    public int LessonId { get; internal set; }
    public int SchemaId { get; internal set; }
    public bool Archived { get; internal set; }

    public static Result<LearningObjective> Create(
        string name,
        string tag,
        string template,
        string environment,
        int lessonId,
        int schemaId,
        DateTime createAt)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<LearningObjective>(new ResultError("learning_objective_invalid_name", "Learning objective name is required."));
        }

        if (lessonId <= 0)
        {
            return Result.Fail<LearningObjective>(new ResultError("learning_objective_invalid_lesson", "Lesson is required."));
        }

        if (schemaId <= 0)
        {
            return Result.Fail<LearningObjective>(new ResultError("learning_objective_invalid_schema", "Schema is required."));
        }

        return Result.Ok(new LearningObjective
        {
            Name = name.Trim(),
            Tag = tag?.Trim() ?? string.Empty,
            Template = template?.Trim() ?? string.Empty,
            Environment = environment?.Trim() ?? string.Empty,
            CreateAt = createAt,
            LessonId = lessonId,
            SchemaId = schemaId,
            Archived = false
        });
    }

    public Result<NoValue> Update(
        string name,
        string tag,
        string template,
        string environment,
        DateTime? startedAt,
        DateTime? doneAt)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<NoValue>(new ResultError("learning_objective_invalid_name", "Learning objective name is required."));
        }

        Name = name.Trim();
        Tag = tag?.Trim() ?? string.Empty;
        Template = template?.Trim() ?? string.Empty;
        Environment = environment?.Trim() ?? string.Empty;
        StartedAt = startedAt;
        DoneAt = doneAt;
        return Result.Ok();
    }

    public Result<NoValue> Archive()
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("learning_objective_already_archived", "Learning objective is already archived."));
        }

        Archived = true;
        return Result.Ok();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

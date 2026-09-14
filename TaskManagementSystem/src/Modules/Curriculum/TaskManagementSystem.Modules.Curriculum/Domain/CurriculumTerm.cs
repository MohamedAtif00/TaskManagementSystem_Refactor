using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Domain;

public sealed class CurriculumTerm : Entity, IAggregateRoot
{
    private CurriculumTerm()
    {
    }

    internal static CurriculumTerm CreateForPersistence() => new();

    public int Id { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public DateTime? StartDate { get; internal set; }
    public DateTime? EndDate { get; internal set; }
    public int ProjectId { get; internal set; }
    public bool Archived { get; internal set; }

    public static Result<CurriculumTerm> Create(string name, DateTime? startDate, DateTime? endDate, int projectId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<CurriculumTerm>(new ResultError("curriculum_term_invalid_name", "Term name is required."));
        }

        if (projectId <= 0)
        {
            return Result.Fail<CurriculumTerm>(new ResultError("curriculum_term_invalid_project", "Project is required."));
        }

        return Result.Ok(new CurriculumTerm
        {
            Name = name.Trim(),
            StartDate = startDate,
            EndDate = endDate,
            ProjectId = projectId,
            Archived = false
        });
    }

    public Result<NoValue> Update(string name, DateTime? startDate, DateTime? endDate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<NoValue>(new ResultError("curriculum_term_invalid_name", "Term name is required."));
        }

        Name = name.Trim();
        StartDate = startDate;
        EndDate = endDate;
        return Result.Ok();
    }

    public Result<NoValue> Archive()
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("curriculum_term_already_archived", "Term is already archived."));
        }

        Archived = true;
        return Result.Ok();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

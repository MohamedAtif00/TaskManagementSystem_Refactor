using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Sprints.Domain;

public sealed class Sprint : Entity, IAggregateRoot
{
    private Sprint()
    {
    }

    internal static Sprint CreateForPersistence() => new();

    public int Id { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public string Description { get; internal set; } = string.Empty;
    public DateTime StartDate { get; internal set; }
    public DateTime EndDate { get; internal set; }
    public bool IsArchived { get; internal set; }

    public static Result<Sprint> Create(string name, string description, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<Sprint>(new ResultError("sprint_invalid_name", "Sprint name is required."));
        }

        if (endDate < startDate)
        {
            return Result.Fail<Sprint>(new ResultError("sprint_invalid_dates", "End date must be on or after start date."));
        }

        return Result.Ok(new Sprint
        {
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? string.Empty : description.Trim(),
            StartDate = startDate,
            EndDate = endDate,
            IsArchived = false
        });
    }

    public Result<NoValue> Update(string name, string description, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<NoValue>(new ResultError("sprint_invalid_name", "Sprint name is required."));
        }

        if (endDate < startDate)
        {
            return Result.Fail<NoValue>(new ResultError("sprint_invalid_dates", "End date must be on or after start date."));
        }

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? string.Empty : description.Trim();
        StartDate = startDate;
        EndDate = endDate;
        return Result.Ok();
    }

    public Result<NoValue> Archive()
    {
        if (IsArchived)
        {
            return Result.Fail<NoValue>(new ResultError("sprint_already_archived", "Sprint is already archived."));
        }

        IsArchived = true;
        return Result.Ok();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

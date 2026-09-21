using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Organization.Domain;

public sealed class Team : Entity, IAggregateRoot
{
    private Team()
    {
    }

    public int Id { get; internal set; }

    public string Name { get; internal set; } = string.Empty;

    public int? TeamleaderId { get; internal set; }

    public bool Archived { get; internal set; }

    internal static Team CreateForPersistence() => new();

    public static Result<Team> Create(string name, int? teamleaderId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<Team>(new ResultError("team_invalid_name", "Team name is required."));
        }

        return Result.Ok(new Team
        {
            Name = name.Trim(),
            TeamleaderId = teamleaderId is > 0 ? teamleaderId : null,
            Archived = false
        });
    }

    public Result<NoValue> Update(string name, int? teamleaderId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<NoValue>(new ResultError("team_invalid_name", "Team name is required."));
        }

        Name = name.Trim();
        TeamleaderId = teamleaderId is > 0 ? teamleaderId : null;
        return Result.Ok();
    }

    public Result<NoValue> Archive()
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("team_already_archived", "Team is already archived."));
        }

        Archived = true;
        return Result.Ok();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

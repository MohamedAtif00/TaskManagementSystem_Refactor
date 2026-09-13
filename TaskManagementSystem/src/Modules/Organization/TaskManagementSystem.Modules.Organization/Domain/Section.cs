using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Organization.Domain;

public sealed class Section : Entity, IAggregateRoot
{
    private readonly List<SectionTeam> _sectionTeams = [];

    private Section()
    {
    }

    public int Id { get; internal set; }

    public string Name { get; internal set; } = string.Empty;

    public bool Archived { get; internal set; }

    public int HeadId { get; internal set; }

    public IReadOnlyCollection<SectionTeam> SectionTeams => _sectionTeams;

    internal static Section CreateForPersistence() => new();

    public static Result<Section> Create(string name, int headId)
    {
        var validation = Validate(name, headId);
        if (!validation.IsSuccess)
        {
            return Result.Fail<Section>(validation.Error);
        }

        return Result.Ok(new Section
        {
            Name = name.Trim(),
            HeadId = headId,
            Archived = false
        });
    }

    public Result<NoValue> Update(string name, int headId)
    {
        var validation = Validate(name, headId);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        Name = name.Trim();
        HeadId = headId;
        return Result.Ok();
    }

    public void ReplaceTeamLinks(IEnumerable<int> teamIds)
    {
        _sectionTeams.Clear();

        foreach (var teamId in teamIds.Distinct())
        {
            _sectionTeams.Add(SectionTeam.Create(teamId));
        }
    }

    public Result<NoValue> Archive()
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("section_already_archived", "Section is already archived."));
        }

        Archived = true;
        return Result.Ok();
    }

    private static Result<NoValue> Validate(string name, int headId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<NoValue>(new ResultError("section_invalid_name", "Section name is required."));
        }

        if (headId <= 0)
        {
            return Result.Fail<NoValue>(new ResultError("section_invalid_head", "Section head is required."));
        }

        return Result.Ok();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

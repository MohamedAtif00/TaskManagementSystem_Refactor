namespace TaskManagementSystem.Modules.Organization.Domain;

public sealed class SectionTeam
{
    public int Id { get; internal set; }

    public int SectionId { get; internal set; }

    public int TeamId { get; internal set; }

    public Section Section { get; internal set; } = null!;

    internal static SectionTeam Create(int teamId) =>
        new()
        {
            TeamId = teamId
        };
}

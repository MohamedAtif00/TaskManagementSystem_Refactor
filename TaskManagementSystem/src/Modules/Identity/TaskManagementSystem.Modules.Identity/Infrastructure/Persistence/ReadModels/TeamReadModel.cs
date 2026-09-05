namespace TaskManagementSystem.Modules.Identity.Infrastructure.Persistence.ReadModels;

internal sealed class TeamReadModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Archived { get; set; }
}

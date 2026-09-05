namespace TaskManagementSystem.Modules.Identity.Infrastructure.Persistence.ReadModels;

internal sealed class NotificationReadModel
{
    public int Id { get; set; }
    public bool IsRead { get; set; }
    public int UserId { get; set; }
}

using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Notifications.Domain;

public sealed class Notification : Entity, IAggregateRoot
{
    private Notification()
    {
    }

    internal static Notification CreateForPersistence() => new();

    public int Id { get; internal set; }
    public string Title { get; internal set; } = string.Empty;
    public string Message { get; internal set; } = string.Empty;
    public string Category { get; internal set; } = string.Empty;
    public string Type { get; internal set; } = string.Empty;
    public string? Status { get; internal set; }
    public bool IsRead { get; internal set; }
    public bool HasActions { get; internal set; }
    public string? AdditionalData { get; internal set; }
    public int? RelatedEntityId { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public int UserId { get; internal set; }

    public static Result<Notification> Create(
        int userId,
        string title,
        string message,
        string category,
        string type,
        int? relatedEntityId = null,
        string? additionalData = null)
    {
        if (userId <= 0)
        {
            return Result.Fail<Notification>(new ResultError("notification_invalid_user", "User is required."));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Fail<Notification>(new ResultError("notification_invalid_title", "Title is required."));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            return Result.Fail<Notification>(new ResultError("notification_invalid_message", "Message is required."));
        }

        return Result.Ok(new Notification
        {
            UserId = userId,
            Title = title.Trim(),
            Message = message.Trim(),
            Category = string.IsNullOrWhiteSpace(category) ? string.Empty : category.Trim(),
            Type = string.IsNullOrWhiteSpace(type) ? string.Empty : type.Trim(),
            Status = null,
            IsRead = false,
            HasActions = false,
            RelatedEntityId = relatedEntityId,
            AdditionalData = additionalData,
            CreatedAt = DateTime.UtcNow
        });
    }

    public Result<NoValue> MarkRead()
    {
        if (IsRead)
        {
            return Result.Fail<NoValue>(new ResultError("notification_already_read", "Notification is already read."));
        }

        IsRead = true;
        return Result.Ok();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

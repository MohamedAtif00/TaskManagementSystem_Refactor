using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Ticket.Domain;

public sealed class Comment : Entity, IAggregateRoot
{
    private Comment()
    {
    }

    internal static Comment CreateForPersistence() => new();

    public int Id { get; internal set; }
    public string Content { get; internal set; } = string.Empty;
    public DateTime CreatedAt { get; internal set; }
    public DateTime Timestamp { get; internal set; }
    public bool Archived { get; internal set; }
    public int LearningObjectiveId { get; internal set; }
    public int? TicketId { get; internal set; }
    public int UserId { get; internal set; }
    public int? ChildId { get; internal set; }

    public Result<NoValue> UpdateContent(string content, int userId)
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("comment_archived", "Comment is archived."));
        }

        if (UserId != userId)
        {
            return Result.Fail<NoValue>(new ResultError("comment_forbidden", "You can only edit your own comments."));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return Result.Fail<NoValue>(new ResultError("comment_invalid_content", "Comment content is required."));
        }

        Content = content.Trim();
        Timestamp = DateTime.UtcNow;
        return Result.Ok();
    }

    public Result<NoValue> Archive(int userId)
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("comment_archived", "Comment is archived."));
        }

        if (UserId != userId)
        {
            return Result.Fail<NoValue>(new ResultError("comment_forbidden", "You can only delete your own comments."));
        }

        Archived = true;
        return Result.Ok();
    }

    public static Result<Comment> Create(
        string content,
        int learningObjectiveId,
        int taskId,
        int userId,
        DateTime timestamp)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Result.Fail<Comment>(new ResultError("comment_invalid_content", "Comment content is required."));
        }

        if (learningObjectiveId <= 0)
        {
            return Result.Fail<Comment>(new ResultError("learning_objective_not_found", "Learning objective is required."));
        }

        if (taskId <= 0)
        {
            return Result.Fail<Comment>(new ResultError("ticket_not_found", "Domain.Ticket is required."));
        }

        if (userId <= 0)
        {
            return Result.Fail<Comment>(new ResultError("user_not_found", "User is required."));
        }

        return Result.Ok(new Comment
        {
            Content = content.Trim(),
            CreatedAt = timestamp,
            Timestamp = timestamp,
            Archived = false,
            LearningObjectiveId = learningObjectiveId,
            TicketId = taskId,
            UserId = userId
        });
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

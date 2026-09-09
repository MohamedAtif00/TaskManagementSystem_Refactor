using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.HR.Domain;

public sealed class Opinion : Entity
{
    private Opinion()
    {
    }

    public int Id { get; internal set; }

    public int LeaveRequestId { get; internal set; }

    public int UserId { get; internal set; }

    public bool IsApproved { get; internal set; }

    public string? Comment { get; internal set; }

    public DateTime CreatedAt { get; internal set; }

    internal static Opinion CreateForPersistence() => new();

    public static Result<Opinion> Create(
        int leaveRequestId,
        int userId,
        bool isApproved,
        string? comment,
        DateTime utcNow)
    {
        return Result.Ok(new Opinion
        {
            LeaveRequestId = leaveRequestId,
            UserId = userId,
            IsApproved = isApproved,
            Comment = comment,
            CreatedAt = utcNow
        });
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

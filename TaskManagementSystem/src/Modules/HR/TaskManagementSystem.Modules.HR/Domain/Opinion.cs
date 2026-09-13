using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.HR.Domain;

public sealed class Opinion : Entity
{
    private Opinion()
    {
    }

    public int Id { get; internal set; }

    public int? LeaveRequestId { get; internal set; }

    public int? PermissionId { get; internal set; }

    public int? WorkFromHomeRequestId { get; internal set; }

    public int? ForgotClockRequestId { get; internal set; }

    public int UserId { get; internal set; }

    public bool IsApproved { get; internal set; }

    public string? Comment { get; internal set; }

    public DateTime CreatedAt { get; internal set; }

    internal static Opinion CreateForPersistence() => new();

    public static Result<Opinion> CreateForLeave(
        int leaveRequestId,
        int userId,
        bool isApproved,
        string? comment,
        DateTime utcNow) =>
        CreateInternal(leaveRequestId, null, null, null, userId, isApproved, comment, utcNow);

    public static Result<Opinion> CreateForPermission(
        int permissionId,
        int userId,
        bool isApproved,
        string? comment,
        DateTime utcNow) =>
        CreateInternal(null, permissionId, null, null, userId, isApproved, comment, utcNow);

    public static Result<Opinion> CreateForWorkFromHome(
        int workFromHomeRequestId,
        int userId,
        bool isApproved,
        string? comment,
        DateTime utcNow) =>
        CreateInternal(null, null, workFromHomeRequestId, null, userId, isApproved, comment, utcNow);

    public static Result<Opinion> CreateForForgotClock(
        int forgotClockRequestId,
        int userId,
        bool isApproved,
        string? comment,
        DateTime utcNow) =>
        CreateInternal(null, null, null, forgotClockRequestId, userId, isApproved, comment, utcNow);

    private static Result<Opinion> CreateInternal(
        int? leaveRequestId,
        int? permissionId,
        int? workFromHomeRequestId,
        int? forgotClockRequestId,
        int userId,
        bool isApproved,
        string? comment,
        DateTime utcNow) =>
        Result.Ok(new Opinion
        {
            LeaveRequestId = leaveRequestId,
            PermissionId = permissionId,
            WorkFromHomeRequestId = workFromHomeRequestId,
            ForgotClockRequestId = forgotClockRequestId,
            UserId = userId,
            IsApproved = isApproved,
            Comment = comment,
            CreatedAt = utcNow
        });

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

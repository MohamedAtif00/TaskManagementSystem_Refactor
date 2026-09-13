using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.HR.Domain;

public sealed class PermissionRequest : Entity, IAggregateRoot
{
    private PermissionRequest()
    {
    }

    public int Id { get; internal set; }

    public int UserId { get; internal set; }

    public PermissionType Type { get; internal set; }

    public PermissionStatus Status { get; internal set; }

    public DateTime PermissionDate { get; internal set; }

    public TimeOnly FromTime { get; internal set; }

    public TimeOnly ToTime { get; internal set; }

    public string? Reason { get; internal set; }

    public DateTime CreatedAt { get; internal set; }

    public DateTime? UpdatedAt { get; internal set; }

    public int? TeamleaderId { get; internal set; }

    public int? SectionheadId { get; internal set; }

    internal static PermissionRequest CreateForPersistence() => new();

    public static Result<PermissionRequest> Create(
        int userId,
        PermissionType type,
        DateTime permissionDate,
        TimeOnly fromTime,
        TimeOnly toTime,
        string? reason,
        int? teamleaderId,
        int? sectionheadId,
        DateTime utcNow)
    {
        var dateValidation = ValidateDateAndTimes(permissionDate, fromTime, toTime, utcNow);
        if (!dateValidation.IsSuccess)
        {
            return Result.Fail<PermissionRequest>(dateValidation.Error);
        }

        return Result.Ok(new PermissionRequest
        {
            UserId = userId,
            Type = type,
            Status = PermissionStatus.Pending,
            PermissionDate = permissionDate.Date,
            FromTime = fromTime,
            ToTime = toTime,
            Reason = reason,
            CreatedAt = utcNow,
            TeamleaderId = teamleaderId,
            SectionheadId = sectionheadId
        });
    }

    public Result<NoValue> Cancel(DateTime utcNow, bool wasApproved)
    {
        if (Status == PermissionStatus.Cancelled)
        {
            return Result.Fail<NoValue>(new ResultError(
                "permission_cannot_cancel",
                "Permission request is already cancelled."));
        }

        if (Status == PermissionStatus.Rejected)
        {
            return Result.Fail<NoValue>(new ResultError(
                "permission_cannot_cancel",
                "Rejected permission requests cannot be cancelled."));
        }

        if (Status == PermissionStatus.Approved && PermissionDate.Date < utcNow.Date)
        {
            return Result.Fail<NoValue>(new ResultError(
                "permission_cannot_cancel",
                "Approved permission that has passed cannot be cancelled."));
        }

        Status = PermissionStatus.Cancelled;
        UpdatedAt = utcNow;
        return Result.Ok();
    }

    public Result<NoValue> Approve(DateTime utcNow)
    {
        if (Status != PermissionStatus.Pending)
        {
            return Result.Fail<NoValue>(new ResultError(
                "permission_not_pending",
                "Only pending permission requests can be approved."));
        }

        Status = PermissionStatus.Approved;
        UpdatedAt = utcNow;
        return Result.Ok();
    }

    public Result<NoValue> Reject(DateTime utcNow)
    {
        if (Status != PermissionStatus.Pending)
        {
            return Result.Fail<NoValue>(new ResultError(
                "permission_not_pending",
                "Only pending permission requests can be rejected."));
        }

        Status = PermissionStatus.Rejected;
        UpdatedAt = utcNow;
        return Result.Ok();
    }

    private static Result<NoValue> ValidateDateAndTimes(
        DateTime permissionDate,
        TimeOnly fromTime,
        TimeOnly toTime,
        DateTime utcNow)
    {
        if (permissionDate.Date < utcNow.Date)
        {
            return Result.Fail<NoValue>(new ResultError(
                "permission_invalid_dates",
                "Permission date cannot be in the past."));
        }

        if (toTime < fromTime)
        {
            return Result.Fail<NoValue>(new ResultError(
                "permission_invalid_times",
                "End time cannot be earlier than start time."));
        }

        return Result.Ok();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

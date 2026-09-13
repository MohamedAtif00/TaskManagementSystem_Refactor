using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.HR.Domain;

public sealed class ForgotClockRequest : Entity, IAggregateRoot
{
    private ForgotClockRequest()
    {
    }

    public int Id { get; internal set; }

    public int UserId { get; internal set; }

    public ForgotClockPunchType PunchType { get; internal set; }

    public ForgotClockStatus Status { get; internal set; }

    public DateTime AttendanceDate { get; internal set; }

    public TimeOnly IntendedTime { get; internal set; }

    public string? Reason { get; internal set; }

    public DateTime CreatedAt { get; internal set; }

    public DateTime? UpdatedAt { get; internal set; }

    public int? TeamleaderId { get; internal set; }

    public int? SectionheadId { get; internal set; }

    internal static ForgotClockRequest CreateForPersistence() => new();

    public static Result<ForgotClockRequest> Create(
        int userId,
        ForgotClockPunchType punchType,
        DateTime attendanceDate,
        TimeOnly intendedTime,
        string? reason,
        int? teamleaderId,
        int? sectionheadId,
        DateTime utcNow)
    {
        if (attendanceDate.Date > utcNow.Date)
        {
            return Result.Fail<ForgotClockRequest>(new ResultError(
                "forgot_clock_invalid_date",
                "Attendance date cannot be in the future."));
        }

        return Result.Ok(new ForgotClockRequest
        {
            UserId = userId,
            PunchType = punchType,
            Status = ForgotClockStatus.Pending,
            AttendanceDate = attendanceDate.Date,
            IntendedTime = intendedTime,
            Reason = reason,
            CreatedAt = utcNow,
            TeamleaderId = teamleaderId,
            SectionheadId = sectionheadId
        });
    }

    public Result<NoValue> Cancel(DateTime utcNow)
    {
        if (Status == ForgotClockStatus.Cancelled)
        {
            return Result.Fail<NoValue>(new ResultError(
                "forgot_clock_cannot_cancel",
                "Forgot clock request is already cancelled."));
        }

        if (Status == ForgotClockStatus.Rejected)
        {
            return Result.Fail<NoValue>(new ResultError(
                "forgot_clock_cannot_cancel",
                "Rejected forgot clock requests cannot be cancelled."));
        }

        if (Status == ForgotClockStatus.Approved && AttendanceDate.Date < utcNow.Date)
        {
            return Result.Fail<NoValue>(new ResultError(
                "forgot_clock_cannot_cancel",
                "Approved forgot clock request for a past date cannot be cancelled."));
        }

        Status = ForgotClockStatus.Cancelled;
        UpdatedAt = utcNow;
        return Result.Ok();
    }

    public Result<NoValue> Approve(DateTime utcNow)
    {
        if (Status != ForgotClockStatus.Pending)
        {
            return Result.Fail<NoValue>(new ResultError(
                "forgot_clock_not_pending",
                "Only pending forgot clock requests can be approved."));
        }

        Status = ForgotClockStatus.Approved;
        UpdatedAt = utcNow;
        return Result.Ok();
    }

    public Result<NoValue> Reject(DateTime utcNow)
    {
        if (Status != ForgotClockStatus.Pending)
        {
            return Result.Fail<NoValue>(new ResultError(
                "forgot_clock_not_pending",
                "Only pending forgot clock requests can be rejected."));
        }

        Status = ForgotClockStatus.Rejected;
        UpdatedAt = utcNow;
        return Result.Ok();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

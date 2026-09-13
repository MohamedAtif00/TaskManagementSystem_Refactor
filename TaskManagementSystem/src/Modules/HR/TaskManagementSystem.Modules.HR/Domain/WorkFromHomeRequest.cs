using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.HR.Domain;

public sealed class WorkFromHomeRequest : Entity, IAggregateRoot
{
    private WorkFromHomeRequest()
    {
    }

    public int Id { get; internal set; }

    public int UserId { get; internal set; }

    public DateTime Date { get; internal set; }

    public DateTime? DateCreated { get; internal set; }

    public string? NoteForManager { get; internal set; }

    public WorkFromHomeStatus Status { get; internal set; }

    public int? TeamleaderId { get; internal set; }

    public int? SectionheadId { get; internal set; }

    internal static WorkFromHomeRequest CreateForPersistence() => new();

    public static Result<WorkFromHomeRequest> Create(
        int userId,
        DateTime date,
        string? noteForManager,
        int? teamleaderId,
        int? sectionheadId,
        DateTime utcNow)
    {
        if (date.Date < utcNow.Date)
        {
            return Result.Fail<WorkFromHomeRequest>(new ResultError(
                "work_from_home_invalid_date",
                "Cannot request work from home for a past date."));
        }

        return Result.Ok(new WorkFromHomeRequest
        {
            UserId = userId,
            Date = date.Date,
            NoteForManager = noteForManager,
            Status = WorkFromHomeStatus.Pending,
            DateCreated = utcNow,
            TeamleaderId = teamleaderId,
            SectionheadId = sectionheadId
        });
    }

    public Result<NoValue> Cancel(DateTime utcNow)
    {
        if (Status == WorkFromHomeStatus.Cancelled)
        {
            return Result.Fail<NoValue>(new ResultError(
                "work_from_home_cannot_cancel",
                "Work from home request is already cancelled."));
        }

        if (Status == WorkFromHomeStatus.Rejected)
        {
            return Result.Fail<NoValue>(new ResultError(
                "work_from_home_cannot_cancel",
                "Rejected work from home requests cannot be cancelled."));
        }

        if (Status == WorkFromHomeStatus.Approved && Date.Date < utcNow.Date)
        {
            return Result.Fail<NoValue>(new ResultError(
                "work_from_home_cannot_cancel",
                "Approved work from home that has passed cannot be cancelled."));
        }

        Status = WorkFromHomeStatus.Cancelled;
        return Result.Ok();
    }

    public Result<NoValue> Approve()
    {
        if (Status != WorkFromHomeStatus.Pending)
        {
            return Result.Fail<NoValue>(new ResultError(
                "work_from_home_not_pending",
                "Only pending work from home requests can be approved."));
        }

        Status = WorkFromHomeStatus.Approved;
        return Result.Ok();
    }

    public Result<NoValue> Reject()
    {
        if (Status != WorkFromHomeStatus.Pending)
        {
            return Result.Fail<NoValue>(new ResultError(
                "work_from_home_not_pending",
                "Only pending work from home requests can be rejected."));
        }

        Status = WorkFromHomeStatus.Rejected;
        return Result.Ok();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

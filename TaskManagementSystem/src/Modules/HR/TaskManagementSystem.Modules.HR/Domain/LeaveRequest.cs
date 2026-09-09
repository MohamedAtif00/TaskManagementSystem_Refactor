using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.HR.Domain;

public sealed class LeaveRequest : Entity, IAggregateRoot
{
    private LeaveRequest()
    {
    }

    public int Id { get; internal set; }
    public int UserId { get; internal set; }
    public LeaveType Type { get; internal set; }
    public LeaveStatus Status { get; internal set; }
    public DateTime StartDate { get; internal set; }
    public DateTime EndDate { get; internal set; }
    public int WorkingDays { get; internal set; }
    public string? Reason { get; internal set; }
    public string? NoteForManager { get; internal set; }
    public string? MedicalCertificateFileName { get; internal set; }
    public string? MedicalCertificatePath { get; internal set; }
    public DateTime? DateCreated { get; internal set; }
    public int? TeamleaderId { get; internal set; }
    public int? SectionheadId { get; internal set; }

    internal static LeaveRequest CreateForPersistence() => new();

    public static Result<LeaveRequest> Create(
        int userId,
        LeaveType type,
        DateTime startDate,
        DateTime endDate,
        int workingDays,
        string? reason,
        string? noteForManager,
        int? teamleaderId,
        int? sectionheadId,
        DateTime utcNow,
        bool requiresMedicalCertificate = false,
        bool hasMedicalCertificate = false)
    {
        var dateValidation = ValidateDates(startDate, endDate, utcNow);
        if (!dateValidation.IsSuccess)
        {
            return Result.Fail<LeaveRequest>(dateValidation.Error);
        }

        if (workingDays <= 0)
        {
            return Result.Fail<LeaveRequest>(new ResultError(
                "leave_invalid_dates",
                "Leave request must include at least one working day."));
        }

        if (requiresMedicalCertificate && !hasMedicalCertificate)
        {
            return Result.Fail<LeaveRequest>(new ResultError(
                "leave_medical_required",
                "Medical certificate is required for sick leave longer than 3 days."));
        }

        return Result.Ok(new LeaveRequest
        {
            UserId = userId,
            Type = type,
            Status = LeaveStatus.Pending,
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            WorkingDays = workingDays,
            Reason = reason,
            NoteForManager = noteForManager,
            DateCreated = utcNow,
            TeamleaderId = teamleaderId,
            SectionheadId = sectionheadId
        });
    }

    public static Result<LeaveRequest> CreateAnnual(
        int userId,
        DateTime startDate,
        DateTime endDate,
        int workingDays,
        string? reason,
        string? noteForManager,
        int? teamleaderId,
        int? sectionheadId,
        DateTime utcNow) =>
        Create(userId, LeaveType.Annual, startDate, endDate, workingDays, reason, noteForManager, teamleaderId, sectionheadId, utcNow);

    public static Result<LeaveRequest> CreateEmergency(
        int userId,
        DateTime startDate,
        DateTime endDate,
        int workingDays,
        string? reason,
        string? noteForManager,
        int? teamleaderId,
        int? sectionheadId,
        DateTime utcNow) =>
        Create(userId, LeaveType.Emergency, startDate, endDate, workingDays, reason, noteForManager, teamleaderId, sectionheadId, utcNow);

    public static Result<LeaveRequest> CreateSick(
        int userId,
        DateTime startDate,
        DateTime endDate,
        int workingDays,
        string? reason,
        string? noteForManager,
        int? teamleaderId,
        int? sectionheadId,
        DateTime utcNow,
        bool hasMedicalCertificate) =>
        Create(
            userId,
            LeaveType.Sick,
            startDate,
            endDate,
            workingDays,
            reason,
            noteForManager,
            teamleaderId,
            sectionheadId,
            utcNow,
            requiresMedicalCertificate: workingDays > 3,
            hasMedicalCertificate: hasMedicalCertificate);

    public static Result<LeaveRequest> CreateUnpaid(
        int userId,
        DateTime startDate,
        DateTime endDate,
        int workingDays,
        string? reason,
        string? noteForManager,
        int? teamleaderId,
        int? sectionheadId,
        DateTime utcNow) =>
        Create(userId, LeaveType.UnpaidLeave, startDate, endDate, workingDays, reason, noteForManager, teamleaderId, sectionheadId, utcNow);

    public static Result<LeaveRequest> CreateFromNextBalance(
        int userId,
        DateTime startDate,
        DateTime endDate,
        int workingDays,
        string? reason,
        string? noteForManager,
        int? teamleaderId,
        int? sectionheadId,
        DateTime utcNow) =>
        Create(userId, LeaveType.FromNextBalance, startDate, endDate, workingDays, reason, noteForManager, teamleaderId, sectionheadId, utcNow);

    internal void SetMedicalCertificate(string fileName, string relativePath)
    {
        MedicalCertificateFileName = fileName;
        MedicalCertificatePath = relativePath;
    }

    internal void ClearMedicalCertificate()
    {
        MedicalCertificateFileName = null;
        MedicalCertificatePath = null;
    }

    public Result<NoValue> Cancel(DateTime utcNow)
    {
        if (Status == LeaveStatus.Cancelled)
        {
            return Result.Fail<NoValue>(new ResultError("leave_cannot_cancel", "Leave request is already cancelled."));
        }

        if (Status == LeaveStatus.Rejected)
        {
            return Result.Fail<NoValue>(new ResultError("leave_cannot_cancel", "Rejected leave requests cannot be cancelled."));
        }

        if (Status == LeaveStatus.Approved && StartDate.Date <= utcNow.Date)
        {
            return Result.Fail<NoValue>(new ResultError("leave_cannot_cancel", "Approved leave that has started or passed cannot be cancelled."));
        }

        Status = LeaveStatus.Cancelled;
        return Result.Ok();
    }

    public Result<NoValue> Approve()
    {
        if (Status != LeaveStatus.Pending)
        {
            return Result.Fail<NoValue>(new ResultError("leave_not_pending", "Only pending leave requests can be approved."));
        }

        Status = LeaveStatus.Approved;
        return Result.Ok();
    }

    public Result<NoValue> Reject()
    {
        if (Status != LeaveStatus.Pending)
        {
            return Result.Fail<NoValue>(new ResultError("leave_not_pending", "Only pending leave requests can be rejected."));
        }

        Status = LeaveStatus.Rejected;
        return Result.Ok();
    }

    private static Result<NoValue> ValidateDates(DateTime startDate, DateTime endDate, DateTime utcNow)
    {
        var start = startDate.Date;
        var end = endDate.Date;

        if (start < utcNow.Date)
        {
            return Result.Fail<NoValue>(new ResultError("leave_invalid_dates", "Start date cannot be in the past."));
        }

        if (end < start)
        {
            return Result.Fail<NoValue>(new ResultError("leave_invalid_dates", "End date must be on or after start date."));
        }

        return Result.Ok();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

namespace TaskManagementSystem.Api.Contracts.HR;

public sealed class LeaveRequestResponse
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int WorkingDays { get; set; }

    public string? Reason { get; set; }

    public string? NoteForManager { get; set; }

    public DateTime? DateCreated { get; set; }

    public string? MedicalCertificateFileName { get; set; }

    public IReadOnlyList<OpinionResponse>? Opinions { get; set; }
}

public sealed class OpinionResponse
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public bool IsApproved { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }
}

public sealed class LeaveRequestListResponse
{
    public IReadOnlyList<LeaveRequestResponse> Items { get; set; } = [];

    public int TotalCount { get; set; }
}

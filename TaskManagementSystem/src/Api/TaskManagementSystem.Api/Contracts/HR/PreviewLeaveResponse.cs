namespace TaskManagementSystem.Api.Contracts.HR;

public sealed class PreviewLeaveResponse
{
    public int RequestedDays { get; set; }

    public int AvailableAnnual { get; set; }

    public int NeededFromNext { get; set; }

    public int FromNextBalanceMaxDays { get; set; }

    public int AlreadyUsedFromNext { get; set; }

    public int PendingFromNext { get; set; }

    public bool RequiresConfirmation { get; set; }

    public string? ErrorMessage { get; set; }
}

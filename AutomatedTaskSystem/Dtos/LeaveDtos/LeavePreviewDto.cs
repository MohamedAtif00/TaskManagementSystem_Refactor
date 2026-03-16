namespace AutomatedTaskSystem.Dtos.LeaveDtos
{
    /// <summary>
    /// Result of previewing an annual leave request (for FromNextBalance confirmation popup).
    /// </summary>
    public class LeavePreviewDto
    {
        public int RequestedDays { get; set; }
        public int AvailableAnnual { get; set; }
        public int NeededFromNext { get; set; }
        public int FromNextBalanceMaxDays { get; set; }
        public int AlreadyUsedFromNext { get; set; }
        /// <summary>Pending FromNextBalance working days (not yet approved).</summary>
        public int PendingFromNext { get; set; }
        /// <summary>True when part of the request would use next balance and user must confirm.</summary>
        public bool NeedsConfirmation { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

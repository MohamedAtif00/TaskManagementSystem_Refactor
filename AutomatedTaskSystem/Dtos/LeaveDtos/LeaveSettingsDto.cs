namespace AutomatedTaskSystem.Dtos.LeaveDtos
{
    /// <summary>
    /// Leave-related settings exposed to the UI (from LeaveSettings + computed state).
    /// </summary>
    public class LeaveSettingsDto
    {
        public int FromNextBalanceMaxDays { get; set; }
        public string? FromNextBalanceStartDate { get; set; }
        public string? FromNextBalanceEndDate { get; set; }
        public string? EmergencyBlackoutCutoffDate { get; set; }
        public string? ResetDate { get; set; }
        /// <summary>Whether the user can currently submit an emergency leave request.</summary>
        public bool EmergencyAllowed { get; set; }
        /// <summary>Whether the current date is within the FromNextBalance window.</summary>
        public bool FromNextBalanceWindowActive { get; set; }
    }
}

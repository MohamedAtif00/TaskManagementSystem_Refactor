namespace AutomatedTaskSystem.Dtos.NotificationDtos
{
    /// <summary>
    /// Request DTO for acting on an actionable notification (Accept / Reject).
    /// </summary>
    public class NotificationActionDto
    {
        /// <summary>
        /// Action to perform: expected values are "accept" or "reject" (case-insensitive).
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Optional comment to attach to the underlying leave/permission/work-from-home opinion.
        /// </summary>
        public string? Comment { get; set; }
    }
}


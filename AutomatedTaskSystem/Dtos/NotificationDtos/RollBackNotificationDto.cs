namespace AutomatedTaskSystem.Dtos.NotificationDtos
{
    public class RollBackNotificationDto
    {
        // User IDs involved in the process
        public int FromUserId { get; set; }
        public string FromUserName { get; set; } = string.Empty;
        public int ToUserId { get; set; }
        public string ToUserName { get; set; } = string.Empty;
        public int TeamLeaderId { get; set; }

        public string FromTaskName { get; set; } = string.Empty;
        public int FromTaskId { get; set; }
        public string ToTaskName { get; set; } = string.Empty;
        public int ToTaskId { get; set; }
        public string LoName { get; set; } = string.Empty;
        public string SprintName { get; set; } = string.Empty;

        // Project info for proper routing on frontend
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;

        // Rollback count for determining criticality (critical when > 1)
        public int RollbackCount { get; set; }
    }
}

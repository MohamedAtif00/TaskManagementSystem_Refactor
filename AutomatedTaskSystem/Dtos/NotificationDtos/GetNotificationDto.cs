namespace AutomatedTaskSystem.Dtos.NotificationDtos
{
    public class GetNotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty; // ISO 8601 string
        public bool HasActions { get; set; }
        public string? Status { get; set; }
        public bool IsRead { get; set; }
        public int? RelatedEntityId { get; set; }   
        public string? AdditionalData { get; set; }
    }
}


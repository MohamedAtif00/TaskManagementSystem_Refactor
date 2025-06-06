    using System.Text.Json.Serialization;

    namespace AutomatedTaskSystem.Models
    {
        public class Permission
        {
            public int Id { get; set; }
            public PermissionType Type { get; set; } 
            public string? Reason { get; set; } = "";
            public int? TeamleaderId { get; set; }
            public int? SectionheadId { get; set; }
            public TimeOnly FromTime { get; set; }
            public TimeOnly ToTime { get; set; }
            public DateTime PermissionDate { get; set; } = DateTime.UtcNow;
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public DateTime? UpdatedAt { get; set; } = null;
            public PermissionStatusEnum Status { get; set; } = PermissionStatusEnum.Pending;
        public int UserId { get; set; }
            public User User { get; set; }
            public List<Opinion> Opinions { get; set; } = new();
            

        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum PermissionType
        {
            WorkAssignment,
            EarlyDeparture,
            LateArrival,
            Departure
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum PermissionStatusEnum
        { 
            Pending,
            Approved,
            Rejected,
            Cancelled
        }
    }

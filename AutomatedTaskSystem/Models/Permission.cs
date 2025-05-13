    using System.Text.Json.Serialization;

    namespace AutomatedTaskSystem.Models
    {
        public class Permission
        {
            public int Id { get; set; }
            public PermissionType Type { get; set; } = PermissionType.Morning;
            public string Reason { get; set; } = "";
            public DateTime PermissionDate { get; set; } = DateTime.UtcNow;
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public DateTime? UpdatedAt { get; set; } = null;
            public int UserId { get; set; }
            public User User { get; set; }


        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum PermissionType
        {
            Morning,
            Night,

        }
    }

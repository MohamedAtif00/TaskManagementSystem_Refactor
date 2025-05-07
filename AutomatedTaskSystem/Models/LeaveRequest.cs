using System.Text.Json.Serialization;

namespace AutomatedTaskSystem.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = new();
        public int? TeamleaderId { get; set; }
        public int? SectionheadId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string? Reason { get; set; }

        public LeaveRequestStatusEnum Status { get; set; } = LeaveRequestStatusEnum.Pending;
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LeaveRequestStatusEnum
    {
        Pending,
        Approved,
        Rejected
    }
}

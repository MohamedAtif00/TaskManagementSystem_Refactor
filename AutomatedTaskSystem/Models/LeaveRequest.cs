using System.Text.Json.Serialization;

namespace AutomatedTaskSystem.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } 
        public int? TeamleaderId { get; set; }
        public int? SectionheadId { get; set; }
        public DateTime? DateCreated { get; init; } = DateTime.Now;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string? Reason { get; set; }
        public string? NoteForManager { get; set; }
        public LeaveRequestType Type { get; set; } = LeaveRequestType.Annual;
        public LeaveRequestStatusEnum Status { get; set; } = LeaveRequestStatusEnum.Pending;
        public List<Opinion> Opinions { get; set; } = new();
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LeaveRequestStatusEnum
    {
        Pending,
        Approved,
        Rejected
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LeaveRequestType { 
        Annual,
        Sick,
        Emergency
    }
}

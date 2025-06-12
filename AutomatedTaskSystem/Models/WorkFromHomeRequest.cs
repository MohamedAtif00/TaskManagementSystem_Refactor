using System.Text.Json.Serialization;

namespace AutomatedTaskSystem.Models
{
    public class WorkFromHomeRequest
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
        public int? TeamleaderId { get; set; }
        public int? SectionheadId { get; set; }
        public DateTime? DateCreated { get; init; } = DateTime.Now;
        public string? NoteForManager { get; set; }

        public DateTime Date { get; set; }
        public WorkFromHomeStatusEnum Status { get; set; } = WorkFromHomeStatusEnum.Pending;
        public List<Opinion> Opinions { get; set; } = new();

    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WorkFromHomeStatusEnum
    {
        Pending,
        Approved,
        Rejected,
        Cancelled
    }
}
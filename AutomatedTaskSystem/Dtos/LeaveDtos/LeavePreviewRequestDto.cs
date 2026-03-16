using System.Text.Json.Serialization;

namespace AutomatedTaskSystem.Dtos.LeaveDtos
{
    public class LeavePreviewRequestDto
    {
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int UserId { get; set; }
        public string StartDate { get; set; } = "";
        public string EndDate { get; set; } = "";
    }
}

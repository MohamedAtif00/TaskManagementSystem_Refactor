using System.Text.Json.Serialization;

namespace AutomatedTaskSystem.Models.Enums.ProjectStatus
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProjectStatus
    {
        Active = 1,
        Closed = 2,
        Hold = 3,
        Reopened = 4
    }
}

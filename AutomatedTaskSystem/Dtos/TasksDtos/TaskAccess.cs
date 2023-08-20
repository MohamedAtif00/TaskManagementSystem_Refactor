using System.Text.Json.Serialization;

namespace AutomatedTaskSystem.Dtos.Tasks;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskAccess
{
    WorkOn = 1,
    Manage = 2,
    WorkOnAndManage = 3,
    None = 4
}

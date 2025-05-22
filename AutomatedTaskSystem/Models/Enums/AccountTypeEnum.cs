using System.Text.Json.Serialization;

namespace AutomatedTaskSystem.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AccountTypeEnum
    {
        Internal,
        External
    }
}

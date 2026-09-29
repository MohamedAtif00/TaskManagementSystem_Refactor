using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace TaskManagementSystem.Api.IntegrationTests;

internal sealed record ApiFailureDto(
    bool Success,
    string Message,
    string? Code = null,
    Dictionary<string, string[]>? Errors = null);

internal static class ApiFailureTestHelper
{
    public static async Task<ApiFailureDto> AssertOkFailureAsync(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.TryGetProperty("success", out var success).Should().BeTrue();
        success.GetBoolean().Should().BeFalse();
        json.TryGetProperty("message", out var message).Should().BeTrue();
        var messageText = message.GetString();
        messageText.Should().NotBeNullOrWhiteSpace();

        string? code = null;
        if (json.TryGetProperty("code", out var codeElement) && codeElement.ValueKind == JsonValueKind.String)
        {
            code = codeElement.GetString();
        }

        Dictionary<string, string[]>? errors = null;
        if (json.TryGetProperty("errors", out var errorsElement) && errorsElement.ValueKind == JsonValueKind.Object)
        {
            errors = [];
            foreach (var property in errorsElement.EnumerateObject())
            {
                errors[property.Name] = property.Value
                    .EnumerateArray()
                    .Select(item => item.GetString() ?? string.Empty)
                    .ToArray();
            }
        }

        return new ApiFailureDto(false, messageText!, code, errors);
    }
}

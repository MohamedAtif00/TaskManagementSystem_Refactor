using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace TaskManagementSystem.TestCommon.Integration;

public static class IntegrationHttpAssertions
{
    public static async Task<T> EnsureAsync<T>(HttpResponseMessage response, HttpStatusCode expected)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(expected, "response body was {0}", body);
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    public static async Task EnsureStatusAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(expected, "response body was {0}", body);
    }
}

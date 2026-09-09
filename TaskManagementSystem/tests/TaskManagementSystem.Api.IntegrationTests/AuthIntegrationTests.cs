using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TaskManagementSystem.Api.Contracts.Auth;
using TaskManagementSystem.TestCommon.Integration;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class AuthIntegrationTests(TmsWebApplicationFactory factory) : IClassFixture<TmsWebApplicationFactory>
{
    [Fact]
    public async Task Login_WhenCodeIsInvalid_ReturnsNotFoundProblemDetails()
    {
        var client = factory.CreateClient();
        await factory.SeedTestUserAsync();

        var response = await client.PostAsJsonAsync("/auth/login", new { code = "BAD001" });
        var problem = await ReadProblemDetailsAsync(response);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        problem.Code.Should().Be("invalid_login_code");
        problem.Detail.Should().Be("Invalid code");
    }

    [Fact]
    public async Task Login_WhenCodeIsValid_ReturnsAccessTokenAndRefreshCookie()
    {
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
        await factory.SeedTestUserAsync();

        var response = await client.PostAsJsonAsync("/auth/login", new { code = IntegrationTestDataSeeder.TestUserCode });
        var body = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        cookies!.Any(cookie => cookie.StartsWith("refreshToken=", StringComparison.Ordinal)).Should().BeTrue();
    }

    [Fact]
    public async Task AboutMe_WhenAuthenticated_ReturnsUserProfile()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/auth/about-me", new { });
        var body = await response.Content.ReadFromJsonAsync<AuthInfoResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Id.Should().BeGreaterThan(0);
        body.Name.Should().Be("Integration Test User");
        body.Group.Should().Be(IntegrationTestDataSeeder.TestTeamName);
    }

    [Fact]
    public async Task RefreshToken_WhenCookieIsValid_ReturnsNewAccessToken()
    {
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
        await factory.SeedTestUserAsync();

        var loginResponse = await client.PostAsJsonAsync("/auth/login", new { code = IntegrationTestDataSeeder.TestUserCode });
        loginResponse.EnsureSuccessStatusCode();

        var refreshResponse = await client.PostAsync("/auth/refresh-token", null);
        var body = await refreshResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Logout_WhenRefreshCookieExists_InvalidatesSession()
    {
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
        await factory.SeedTestUserAsync();

        var loginResponse = await client.PostAsJsonAsync("/auth/login", new { code = IntegrationTestDataSeeder.TestUserCode });
        loginResponse.EnsureSuccessStatusCode();

        var logoutResponse = await client.PostAsync("/auth/logout", null);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refreshResponse = await client.PostAsync("/auth/refresh-token", null);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WhenCodeIsEmpty_ReturnsValidationProblemDetails()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/auth/login", new { code = string.Empty });
        var problem = await ReadProblemDetailsAsync(response);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        problem.Code.Should().Be("validation_failed");
        problem.Errors.Should().ContainKey("Code");
    }

    private static async Task<ProblemDetailsDto> ReadProblemDetailsAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = new Dictionary<string, string[]>();

        if (json.TryGetProperty("errors", out var errorsElement) &&
            errorsElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in errorsElement.EnumerateObject())
            {
                errors[property.Name] = property.Value
                    .EnumerateArray()
                    .Select(item => item.GetString() ?? string.Empty)
                    .ToArray();
            }
        }

        return new ProblemDetailsDto
        {
            Detail = json.TryGetProperty("detail", out var detail) ? detail.GetString() : null,
            Code = json.TryGetProperty("code", out var code) ? code.GetString() : null,
            Errors = errors
        };
    }

    private sealed class ProblemDetailsDto
    {
        public string? Detail { get; init; }

        public string? Code { get; init; }

        public Dictionary<string, string[]> Errors { get; init; } = [];
    }
}

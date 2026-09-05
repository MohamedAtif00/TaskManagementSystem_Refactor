using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskManagementSystem.Modules.Identity.Infrastructure.Testing;
using TaskManagementSystem.TestCommon.Integration;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class AuthIntegrationTests(TmsWebApplicationFactory factory) : IClassFixture<TmsWebApplicationFactory>
{
    [Fact]
    public async Task Login_WhenCodeIsInvalid_ReturnsNotFound()
    {
        var client = factory.CreateClient();
        await factory.SeedTestUserAsync();

        var response = await client.PostAsJsonAsync("/auth/login", new { code = "BAD001" });
        var body = await response.Content.ReadFromJsonAsync<LegacyResponse<string>>();

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        body!.Error.Should().BeTrue();
        body.Message.Should().Be("Invalid code");
    }

    [Fact]
    public async Task Login_WhenCodeIsValid_ReturnsAccessTokenAndRefreshCookie()
    {
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
        await factory.SeedTestUserAsync();

        var response = await client.PostAsJsonAsync("/auth/login", new { code = IdentityTestDataSeeder.TestUserCode });
        var body = await response.Content.ReadFromJsonAsync<LegacyResponse<string>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Error.Should().BeFalse();
        body.Data.Should().NotBeNullOrWhiteSpace();
        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        cookies!.Any(cookie => cookie.StartsWith("refreshToken=", StringComparison.Ordinal)).Should().BeTrue();
    }

    [Fact]
    public async Task AboutMe_WhenAuthenticated_ReturnsUserProfile()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/auth/about-me", new { });
        var body = await response.Content.ReadFromJsonAsync<LegacyResponse<AboutMeResponse>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Error.Should().BeFalse();
        body.Data!.Id.Should().BeGreaterThan(0);
        body.Data.Name.Should().Be("Integration Test User");
        body.Data.Group.Should().Be(IdentityTestDataSeeder.TestTeamName);
    }

    [Fact]
    public async Task RefreshToken_WhenCookieIsValid_ReturnsNewAccessToken()
    {
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
        await factory.SeedTestUserAsync();

        var loginResponse = await client.PostAsJsonAsync("/auth/login", new { code = IdentityTestDataSeeder.TestUserCode });
        loginResponse.EnsureSuccessStatusCode();

        var refreshResponse = await client.PostAsync("/auth/refresh-token", null);
        var body = await refreshResponse.Content.ReadFromJsonAsync<LegacyResponse<string>>();

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Error.Should().BeFalse();
        body.Data.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Logout_WhenRefreshCookieExists_InvalidatesSession()
    {
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
        await factory.SeedTestUserAsync();

        var loginResponse = await client.PostAsJsonAsync("/auth/login", new { code = IdentityTestDataSeeder.TestUserCode });
        loginResponse.EnsureSuccessStatusCode();

        var logoutResponse = await client.PostAsync("/auth/logout", null);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshResponse = await client.PostAsync("/auth/refresh-token", null);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed class LegacyResponse<T>
    {
        public T? Data { get; set; }
        public bool Error { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    private sealed class AboutMeResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Group { get; set; }
        public int Notifications { get; set; }
    }
}

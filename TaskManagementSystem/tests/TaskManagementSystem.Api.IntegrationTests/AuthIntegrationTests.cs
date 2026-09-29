using System.Net;
using System.Net.Http.Json;
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
        var failure = await ApiFailureTestHelper.AssertOkFailureAsync(response);

        failure.Code.Should().Be("invalid_login_code");
        failure.Message.Should().Be("Invalid code");
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
        await ApiFailureTestHelper.AssertOkFailureAsync(refreshResponse);
    }

    [Fact]
    public async Task Login_WhenCodeIsEmpty_ReturnsValidationProblemDetails()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/auth/login", new { code = string.Empty });
        var failure = await ApiFailureTestHelper.AssertOkFailureAsync(response);

        failure.Code.Should().Be("validation_failed");
        failure.Errors.Should().ContainKey("Code");
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Task = System.Threading.Tasks.Task;

namespace AutomatedTaskSystem.Test.Integration;

[Collection("Integration")]
public class AllEndpointsIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private static readonly IReadOnlyList<DiscoveredEndpoint> Endpoints = EndpointDiscovery.DiscoverAll();

    public AllEndpointsIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
    }

    public static IEnumerable<object[]> AllEndpoints()
        => Endpoints.Where(e => !e.SkipSmokeTest).Select(e => new object[] { e });

    [Theory]
    [MemberData(nameof(AllEndpoints))]
    public async Task Endpoint_DoesNotReturnInternalServerError(DiscoveredEndpoint endpoint)
    {
        var client = UseAnonymousClient(endpoint)
            ? _factory.CreateClient()
            : await _factory.CreateAuthenticatedClientAsync();

        using var request = BuildRequest(endpoint);
        ResolveSeededRouteValues(request, endpoint);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var response = await client.SendAsync(request, cts.Token);

        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public void DiscoverAll_FindsAtLeastOneHundredEndpoints()
    {
        Assert.True(Endpoints.Count >= 100, $"Expected broad API coverage, found {Endpoints.Count} endpoints.");
    }

    [Fact]
    public async Task Auth_Login_ReturnsToken_ForSeededUser()
    {
        await _factory.SeedTestUserAsync();
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/auth/login",
            new { code = IntegrationTestWebAppFactory.TestUserCode });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
        Assert.NotNull(body);
        Assert.False(body!.Error);
        Assert.False(string.IsNullOrWhiteSpace(body.Data));
    }

    [Fact]
    public async Task Auth_AboutMe_ReturnsUser_WhenAuthenticated()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsync("/auth/about-me", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private void ResolveSeededRouteValues(HttpRequestMessage request, DiscoveredEndpoint endpoint)
    {
        if (request.RequestUri is null)
        {
            return;
        }

        var uri = request.RequestUri.ToString();
        if (uri.Contains("sprintId=99999", StringComparison.Ordinal))
        {
            uri = uri.Replace("sprintId=99999", $"sprintId={_factory.TestSprintId}");
            request.RequestUri = new Uri(uri, UriKind.RelativeOrAbsolute);
        }
    }

    private static bool UseAnonymousClient(DiscoveredEndpoint endpoint)
        => endpoint.Path.StartsWith("/auth/login", StringComparison.OrdinalIgnoreCase)
           || endpoint.Path.StartsWith("/auth/logout", StringComparison.OrdinalIgnoreCase)
           || endpoint.Path.StartsWith("/auth/refresh-token", StringComparison.OrdinalIgnoreCase);

    private static HttpRequestMessage BuildRequest(DiscoveredEndpoint endpoint)
    {
        var request = new HttpRequestMessage(new HttpMethod(endpoint.HttpMethod), endpoint.Path);

        if (endpoint.Path.Contains("medical-certificate", StringComparison.OrdinalIgnoreCase)
            || endpoint.ActionName.Contains("CreateLeave", StringComparison.Ordinal))
        {
            request.Content = new MultipartFormDataContent();
            return request;
        }

        if (endpoint.HttpMethod is "POST" or "PUT" or "PATCH")
        {
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");
        }

        return request;
    }

    private sealed class ApiResponse<T>
    {
        public T? Data { get; set; }
        public bool Error { get; set; }
        public string? Message { get; set; }
    }
}

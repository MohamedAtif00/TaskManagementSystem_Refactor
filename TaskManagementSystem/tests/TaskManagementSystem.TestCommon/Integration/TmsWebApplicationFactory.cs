using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TaskManagementSystem.Modules.Identity.Infrastructure.Testing;

namespace TaskManagementSystem.TestCommon.Integration;

public sealed class TmsWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestJwtSigningKey = "TaskManagementSystemTestSigningKeyMustBe32Chars!";
    private readonly SemaphoreSlim _seedLock = new(1, 1);
    private bool _seeded;
    private HttpClient? _authenticatedClient;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenTelemetry:OtlpEndpoint"] = string.Empty,
                ["AppSetting:Token"] = TestJwtSigningKey
            });
        });

        builder.ConfigureServices(services =>
        {
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtSigningKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });
        });
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        if (_authenticatedClient is not null)
        {
            return _authenticatedClient;
        }

        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });

        await SeedTestUserAsync();
        await AuthenticateClientAsync(client);
        _authenticatedClient = client;
        return client;
    }

    public async Task SeedTestUserAsync()
    {
        await _seedLock.WaitAsync();
        try
        {
            if (_seeded)
            {
                return;
            }

            await IdentityTestDataSeeder.SeedAsync(Services);
            _seeded = true;
        }
        finally
        {
            _seedLock.Release();
        }
    }

    private async Task AuthenticateClientAsync(HttpClient client)
    {
        var loginResponse = await client.PostAsJsonAsync(
            "/auth/login",
            new { code = IdentityTestDataSeeder.TestUserCode });

        loginResponse.EnsureSuccessStatusCode();

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        if (string.IsNullOrWhiteSpace(loginBody?.Data))
        {
            throw new InvalidOperationException("Failed to obtain JWT for integration tests.");
        }

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody.Data);
    }

    private sealed class LoginResponse
    {
        public string? Data { get; set; }
        public bool Error { get; set; }
        public string? Message { get; set; }
    }
}

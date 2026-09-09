using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence;
using TaskManagementSystem.Modules.Identity.Infrastructure.Persistence;

namespace TaskManagementSystem.TestCommon.Integration;

public sealed class TmsWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestJwtSigningKey = "TaskManagementSystemTestSigningKeyMustBe32Chars!";
    private readonly string _databaseSuffix = Guid.NewGuid().ToString("N");
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
                ["AppSetting:Token"] = TestJwtSigningKey,
                ["LeaveSettings:FromNextBalanceMaxDays"] = "3",
                ["LeaveSettings:FromNextBalanceStartDate"] = "01-01",
                ["LeaveSettings:FromNextBalanceEndDate"] = "12-31",
                ["LeaveSettings:EmergencyBlackoutCutoffDate"] = "12-31",
                ["LeaveSettings:ResetDate"] = "01-01",
                ["LeaveSettings:MedicalCertificateRelativePath"] = "medical-certificates"
            });
        });

        builder.ConfigureServices(services =>
        {
            ReplaceInMemoryDbContext<IdentityDbContext>(services, $"IdentityTests_{_databaseSuffix}");
            ReplaceInMemoryDbContext<HrDbContext>(services, $"HrTests_{_databaseSuffix}");

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

    private static void ReplaceInMemoryDbContext<TContext>(IServiceCollection services, string databaseName)
        where TContext : DbContext
    {
        services.RemoveAll<DbContextOptions<TContext>>();
        services.RemoveAll<TContext>();
        services.AddDbContext<TContext>(options => options.UseInMemoryDatabase(databaseName));
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

            await IntegrationTestDataSeeder.SeedAsync(Services);
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
            new { code = IntegrationTestDataSeeder.TestUserCode });

        loginResponse.EnsureSuccessStatusCode();

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        if (string.IsNullOrWhiteSpace(loginBody?.AccessToken))
        {
            throw new InvalidOperationException("Failed to obtain JWT for integration tests.");
        }

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody.AccessToken);
    }

    private sealed class LoginResponse
    {
        public string? AccessToken { get; set; }
    }
}

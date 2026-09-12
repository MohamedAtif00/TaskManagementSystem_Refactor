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
    private readonly string _databaseName = $"TmsTests_{Guid.NewGuid():N}";
    private readonly SemaphoreSlim _seedLock = new(1, 1);
    private bool _databaseInitialized;
    private bool _seeded;
    private HttpClient? _authenticatedClient;

    private string ConnectionString =>
        $"Server=(localdb)\\MSSQLLocalDB;Database={_databaseName};Trusted_Connection=True;TrustServerCertificate=True";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString,
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
            ReplaceSqlServerDbContext<IdentityDbContext>(services, ConnectionString);
            ReplaceSqlServerDbContext<HrDbContext>(services, ConnectionString);

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

    private static void ReplaceSqlServerDbContext<TContext>(IServiceCollection services, string connectionString)
        where TContext : DbContext
    {
        services.RemoveAll<DbContextOptions<TContext>>();
        services.RemoveAll<TContext>();
        services.AddDbContext<TContext>(options => options.UseSqlServer(connectionString));
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
            if (!_databaseInitialized)
            {
                await IntegrationTestDatabaseBootstrap.InitializeAsync(ConnectionString);
                _databaseInitialized = true;
            }

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

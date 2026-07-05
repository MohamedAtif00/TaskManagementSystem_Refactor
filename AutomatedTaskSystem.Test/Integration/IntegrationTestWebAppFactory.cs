using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums;
using AutomatedTaskSystem.Models.Enums.UserRole;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System.Net.Http.Json;
using Task = System.Threading.Tasks.Task;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace AutomatedTaskSystem.Test.Integration;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    public const string TestUserCode = "TST001";
    private readonly SemaphoreSlim _seedLock = new(1, 1);
    private bool _seeded;
    public int TestSprintId { get; private set; } = 99999;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IHostedService>();
        });
    }

    private HttpClient? _authenticatedClient;

    public async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        if (_authenticatedClient is not null)
        {
            return _authenticatedClient;
        }

        var client = CreateClient();
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

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();
        await db.Database.EnsureCreatedAsync();

        if (!await db.Users.AnyAsync(u => u.Code == TestUserCode))
        {
        db.Users.Add(new User
        {
            Code = TestUserCode,
            Name = "Integration Test User",
            Role = UserRoleEnum.Owner,
            HR_code = "999999",
            AccountType = AccountTypeEnum.Internal,
            Annual_leave_MAX = 30,
            Emergency_leave_MAX = 5,
            Permission_MAX = 10,
            WorkFromHome_MAX = 5,
        });

        await db.SaveChangesAsync();
        }

        if (!await db.Sprints.AnyAsync())
        {
            var sprint = new Sprint
            {
                Name = "Integration Test Sprint",
                Description = "Seeded for API integration tests",
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow.AddDays(7),
            };
            db.Sprints.Add(sprint);
            await db.SaveChangesAsync();
            TestSprintId = sprint.Id;
        }
        else
        {
            TestSprintId = await db.Sprints.Select(s => s.Id).FirstAsync();
        }

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
            new { code = TestUserCode });

        loginResponse.EnsureSuccessStatusCode();

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        if (string.IsNullOrWhiteSpace(loginBody?.Data))
        {
            throw new InvalidOperationException("Failed to obtain JWT for integration tests.");
        }

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginBody.Data);
    }

    private sealed class LoginResponse
    {
        public string? Data { get; set; }
        public bool Error { get; set; }
        public string? Message { get; set; }
    }
}

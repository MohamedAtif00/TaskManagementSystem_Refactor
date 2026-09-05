using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementSystem.BuildingBlocks.Persistence.Audit;
using TaskManagementSystem.Modules.Identity.Features.Authenticate;
using TaskManagementSystem.Modules.Identity.Infrastructure.Testing;
using TaskManagementSystem.TestCommon.Integration;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class AuditIntegrationTests(TmsWebApplicationFactory factory) : IClassFixture<TmsWebApplicationFactory>
{
    [Fact]
    public async Task Login_WhenCodeIsValid_WritesSuccessfulAuthenticateCommandAuditEntry()
    {
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
        await factory.SeedTestUserAsync();

        var response = await client.PostAsJsonAsync("/auth/login", new { code = IdentityTestDataSeeder.TestUserCode });

        response.EnsureSuccessStatusCode();

        using var scope = factory.Services.CreateScope();
        var auditDb = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var entry = await auditDb.AuditLog
            .Where(log => log.ActionName == nameof(AuthenticateCommand) && log.Success)
            .OrderByDescending(log => log.Id)
            .FirstOrDefaultAsync();

        entry.Should().NotBeNull();
        entry!.CorrelationId.Should().NotBeNullOrWhiteSpace();
        entry.OccurredOnUtc.Should().BeBefore(DateTime.UtcNow.AddMinutes(1));
    }

    [Fact]
    public async Task Login_WhenCodeIsInvalid_WritesFailedAuthenticateCommandAuditEntry()
    {
        var client = factory.CreateClient();
        await factory.SeedTestUserAsync();

        var response = await client.PostAsJsonAsync("/auth/login", new { code = "BAD001" });

        response.IsSuccessStatusCode.Should().BeFalse();

        using var scope = factory.Services.CreateScope();
        var auditDb = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var entry = await auditDb.AuditLog
            .Where(log => log.ActionName == nameof(AuthenticateCommand) && !log.Success)
            .OrderByDescending(log => log.Id)
            .FirstOrDefaultAsync();

        entry.Should().NotBeNull();
    }
}

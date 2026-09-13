using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.TestCommon.Integration;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class HrForgotClockIntegrationTests(TmsWebApplicationFactory factory)
    : IClassFixture<TmsWebApplicationFactory>
{
    private static readonly DateTime AttendanceDate = new(2026, 9, 5);

    [Fact]
    public async Task ForgotClockFlow_WhenOwnerRequestsAndApproves_UpdatesStatus()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var createResponse = await client.PostAsJsonAsync(
            "/hr/forgot-clock",
            new CreateForgotClockRequest("ClockIn", AttendanceDate, "09:00:00", "Forgot to punch in"));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ForgotClockRequestResponse>();
        created!.Status.Should().Be("Pending");
        created.PunchType.Should().Be("ClockIn");

        var approveResponse = await client.PostAsJsonAsync(
            $"/hr/forgot-clock/{created.Id}/opinions",
            new GiveForgotClockOpinionRequest(true, "Acknowledged"));
        var approved = await approveResponse.Content.ReadFromJsonAsync<ForgotClockRequestResponse>();
        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        approved!.Status.Should().Be("Approved");
    }

    [Fact]
    public async Task CancelPendingForgotClock_Succeeds()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var createResponse = await client.PostAsJsonAsync(
            "/hr/forgot-clock",
            new CreateForgotClockRequest("ClockOut", AttendanceDate, "17:00:00", "Forgot to punch out"));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ForgotClockRequestResponse>();

        var cancelResponse = await client.PutAsync($"/hr/forgot-clock/{created!.Id}/cancel", null);
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cancelled = await cancelResponse.Content.ReadFromJsonAsync<ForgotClockRequestResponse>();
        cancelled!.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task ForgotClockDuplicatePunch_ReturnsBadRequest()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var duplicateDate = new DateTime(2026, 9, 6);

        var first = await client.PostAsJsonAsync(
            "/hr/forgot-clock",
            new CreateForgotClockRequest("ClockIn", duplicateDate, "09:00:00", "First"));
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await client.PostAsJsonAsync(
            "/hr/forgot-clock",
            new CreateForgotClockRequest("ClockIn", duplicateDate, "09:30:00", "Second"));
        var json = await second.Content.ReadFromJsonAsync<JsonElement>();

        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        json.GetProperty("code").GetString().Should().Be("forgot_clock_duplicate_punch");
    }

    [Fact]
    public async Task GetForgotClockById_WhenNotFound_ReturnsProblemDetails()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/hr/forgot-clock/99999");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        json.GetProperty("code").GetString().Should().Be("forgot_clock_request_not_found");
    }
}

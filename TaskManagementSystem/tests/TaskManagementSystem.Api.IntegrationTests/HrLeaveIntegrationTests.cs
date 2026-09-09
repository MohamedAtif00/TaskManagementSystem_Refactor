using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.TestCommon.Integration;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class HrLeaveIntegrationTests(TmsWebApplicationFactory factory) : IClassFixture<TmsWebApplicationFactory>
{
    private static readonly DateTime LeaveStart = new(2027, 1, 4);
    private static readonly DateTime LeaveEnd = new(2027, 1, 6);

    [Fact]
    public async Task LeaveFlow_WhenOwnerRequestsAndApproves_UpdatesBalancesAndSupportsCancel()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var balancesResponse = await client.GetAsync("/hr/leave/balances");
        var initialBalances = await balancesResponse.Content.ReadFromJsonAsync<LeaveBalancesResponse>();

        balancesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        initialBalances!.AnnualLeaveMax.Should().Be(30);
        initialBalances.AvailableAnnualLeave.Should().Be(30);

        var createResponse = await client.PostAsJsonAsync(
            "/hr/leave/leave-requests",
            new CreateLeaveRequest("Annual", LeaveStart, LeaveEnd, "Vacation", "Please approve"));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdLeave = await createResponse.Content.ReadFromJsonAsync<LeaveRequestResponse>();
        createdLeave!.Status.Should().Be("Pending");
        createdLeave.WorkingDays.Should().Be(3);

        var pendingResponse = await client.GetAsync("/hr/leave/leave-requests/pending");
        var pendingItems = await pendingResponse.Content.ReadFromJsonAsync<List<LeaveRequestResponse>>();
        pendingResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        pendingItems!.Should().Contain(item => item.Id == createdLeave.Id);

        var approveResponse = await client.PostAsync($"/hr/leave/leave-requests/{createdLeave.Id}/approve", null);
        var approvedLeave = await approveResponse.Content.ReadFromJsonAsync<LeaveRequestResponse>();
        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        approvedLeave!.Status.Should().Be("Approved");

        var balancesAfterApproval = await (await client.GetAsync("/hr/leave/balances"))
            .Content.ReadFromJsonAsync<LeaveBalancesResponse>();
        balancesAfterApproval!.AnnualLeave.Should().Be(3);
        balancesAfterApproval.AvailableAnnualLeave.Should().Be(27);

        var secondCreateResponse = await client.PostAsJsonAsync(
            "/hr/leave/leave-requests",
            new CreateLeaveRequest("Annual", new DateTime(2027, 1, 11), new DateTime(2027, 1, 13), "Second", null));
        secondCreateResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var pendingLeave = await secondCreateResponse.Content.ReadFromJsonAsync<LeaveRequestResponse>();

        var cancelResponse = await client.PutAsync($"/hr/leave/leave-requests/{pendingLeave!.Id}/cancel", null);
        var cancelledLeave = await cancelResponse.Content.ReadFromJsonAsync<LeaveRequestResponse>();
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        cancelledLeave!.Status.Should().Be("Cancelled");

        var balancesAfterCancel = await (await client.GetAsync("/hr/leave/balances"))
            .Content.ReadFromJsonAsync<LeaveBalancesResponse>();
        balancesAfterCancel!.AvailableAnnualLeave.Should().Be(27);
    }

    [Fact]
    public async Task GetLeaveRequestById_WhenRequestDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/hr/leave/leave-requests/99999");
        var problem = await ReadProblemDetailsAsync(response);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        problem.Code.Should().Be("leave_request_not_found");
    }

    [Fact]
    public async Task GetLeaveSettings_ReturnsConfiguredWindows()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/hr/leave/leave-settings");
        var settings = await response.Content.ReadFromJsonAsync<LeaveSettingsResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        settings!.FromNextBalanceMaxDays.Should().Be(3);
        settings.FromNextWindowActiveToday.Should().BeTrue();
    }

    [Fact]
    public async Task PreviewLeave_WhenAnnualIsSufficient_DoesNotRequireConfirmation()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/hr/leave/leave-requests/preview",
            new PreviewLeaveRequest(LeaveStart, LeaveEnd));

        var preview = await response.Content.ReadFromJsonAsync<PreviewLeaveResponse>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        preview!.RequestedDays.Should().Be(3);
        preview.RequiresConfirmation.Should().BeFalse();
    }

    [Fact]
    public async Task EmergencyLeave_WhenCreated_StaysPendingUntilOwnerApproves()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var createResponse = await client.PostAsJsonAsync(
            "/hr/leave/leave-requests",
            new CreateLeaveRequest("Emergency", LeaveStart, LeaveStart, "Family emergency", null));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<LeaveRequestResponse>();
        created!.Type.Should().Be("Emergency");

        var approveResponse = await client.PostAsJsonAsync(
            $"/hr/leave/leave-requests/{created.Id}/opinions",
            new GiveLeaveOpinionRequest(true, "Approved"));

        var approved = await approveResponse.Content.ReadFromJsonAsync<LeaveRequestResponse>();
        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        approved!.Status.Should().Be("Approved");
    }

    [Fact]
    public async Task OwnerRejectViaOpinion_SetsRejectedStatusWithoutBalanceChange()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var balancesBefore = await (await client.GetAsync("/hr/leave/balances"))
            .Content.ReadFromJsonAsync<LeaveBalancesResponse>();

        var createResponse = await client.PostAsJsonAsync(
            "/hr/leave/leave-requests",
            new CreateLeaveRequest("Annual", LeaveStart, LeaveEnd, "Reject me", null));

        var created = await createResponse.Content.ReadFromJsonAsync<LeaveRequestResponse>();

        var rejectResponse = await client.PostAsJsonAsync(
            $"/hr/leave/leave-requests/{created!.Id}/opinions",
            new GiveLeaveOpinionRequest(false, "Not now"));

        var rejected = await rejectResponse.Content.ReadFromJsonAsync<LeaveRequestResponse>();
        rejectResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        rejected!.Status.Should().Be("Rejected");

        var balancesAfter = await (await client.GetAsync("/hr/leave/balances"))
            .Content.ReadFromJsonAsync<LeaveBalancesResponse>();
        balancesAfter!.AvailableAnnualLeave.Should().Be(balancesBefore!.AvailableAnnualLeave);
    }

    [Fact]
    public async Task PublicHoliday_WhenOverlapsLeaveRange_ReducesWorkingDays()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var createHolidayResponse = await client.PostAsJsonAsync(
            "/hr/holidays",
            new CreateHolidayRequest("National Day", "Formal vacation", new DateTime(2027, 1, 5), new DateTime(2027, 1, 5)));

        createHolidayResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var previewResponse = await client.PostAsJsonAsync(
            "/hr/leave/leave-requests/preview",
            new PreviewLeaveRequest(LeaveStart, LeaveEnd));

        var preview = await previewResponse.Content.ReadFromJsonAsync<PreviewLeaveResponse>();
        previewResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        preview!.RequestedDays.Should().Be(2);

        var createLeaveResponse = await client.PostAsJsonAsync(
            "/hr/leave/leave-requests",
            new CreateLeaveRequest("Annual", LeaveStart, LeaveEnd, "Holiday overlap", null));

        var createdLeave = await createLeaveResponse.Content.ReadFromJsonAsync<LeaveRequestResponse>();
        createLeaveResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        createdLeave!.WorkingDays.Should().Be(2);

        var holidaysResponse = await client.GetAsync("/hr/holidays?fromDate=2027-01-01&toDate=2027-12-31");
        var holidays = await holidaysResponse.Content.ReadFromJsonAsync<List<HolidayResponse>>();
        holidaysResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        holidays!.Should().Contain(h => h.Name == "National Day");
    }

    private static async Task<ProblemDetailsDto> ReadProblemDetailsAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        return new ProblemDetailsDto
        {
            Detail = json.TryGetProperty("detail", out var detail) ? detail.GetString() : null,
            Code = json.TryGetProperty("code", out var code) ? code.GetString() : null
        };
    }

    private sealed class ProblemDetailsDto
    {
        public string? Detail { get; init; }

        public string? Code { get; init; }
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.TestCommon.Integration;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class HrPermissionWorkFromHomeIntegrationTests(TmsWebApplicationFactory factory)
    : IClassFixture<TmsWebApplicationFactory>
{
    private static readonly DateTime PermissionDate = new(2027, 2, 2);
    private static readonly DateTime WfhDate = new(2027, 2, 3);

    [Fact]
    public async Task PermissionFlow_WhenOwnerRequestsAndApproves_UpdatesBalanceAndSupportsCancel()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var balancesBefore = await (await client.GetAsync("/hr/leave/balances"))
            .Content.ReadFromJsonAsync<LeaveBalancesResponse>();
        balancesBefore!.PermissionMax.Should().Be(10);
        balancesBefore.AvailablePermission.Should().Be(10);

        var createResponse = await client.PostAsJsonAsync(
            "/hr/permissions",
            new CreatePermissionRequest("EarlyDeparture", PermissionDate, "09:00", "11:00", "Appointment"));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<PermissionRequestResponse>();
        created!.Status.Should().Be("Pending");

        var approveResponse = await client.PostAsJsonAsync(
            $"/hr/permissions/{created.Id}/opinions",
            new GivePermissionOpinionRequest(true, "Approved"));
        var approved = await approveResponse.Content.ReadFromJsonAsync<PermissionRequestResponse>();
        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        approved!.Status.Should().Be("Approved");

        var balancesAfterApproval = await (await client.GetAsync("/hr/leave/balances"))
            .Content.ReadFromJsonAsync<LeaveBalancesResponse>();
        balancesAfterApproval!.Permission.Should().Be(1);
        balancesAfterApproval.AvailablePermission.Should().Be(9);

        var cancelResponse = await client.PutAsync($"/hr/permissions/{created.Id}/cancel", null);
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var balancesAfterCancel = await (await client.GetAsync("/hr/leave/balances"))
            .Content.ReadFromJsonAsync<LeaveBalancesResponse>();
        balancesAfterCancel!.Permission.Should().Be(0);
        balancesAfterCancel.AvailablePermission.Should().Be(10);
    }

    [Fact]
    public async Task WorkFromHomeFlow_WhenOwnerRequestsAndApproves_UpdatesBalance()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var createResponse = await client.PostAsJsonAsync(
            "/hr/work-from-home",
            new CreateWorkFromHomeRequest(WfhDate, "Focus work"));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<WorkFromHomeRequestResponse>();
        created!.Status.Should().Be("Pending");

        var approveResponse = await client.PostAsync($"/hr/work-from-home/{created.Id}/approve", null);
        var approved = await approveResponse.Content.ReadFromJsonAsync<WorkFromHomeRequestResponse>();
        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        approved!.Status.Should().Be("Approved");

        var balances = await (await client.GetAsync("/hr/leave/balances"))
            .Content.ReadFromJsonAsync<LeaveBalancesResponse>();
        balances!.WorkFromHome.Should().Be(1);
        balances.AvailableWorkFromHome.Should().Be(4);
    }

    [Fact]
    public async Task GetPermissionById_WhenNotFound_ReturnsProblemDetails()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/hr/permissions/99999");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        json.GetProperty("code").GetString().Should().Be("permission_request_not_found");
    }

    [Fact]
    public async Task WorkFromHomeDuplicateDate_ReturnsBadRequest()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var duplicateDate = new DateTime(2027, 3, 10);

        var first = await client.PostAsJsonAsync(
            "/hr/work-from-home",
            new CreateWorkFromHomeRequest(duplicateDate, "First"));
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await client.PostAsJsonAsync(
            "/hr/work-from-home",
            new CreateWorkFromHomeRequest(duplicateDate, "Second"));
        var json = await second.Content.ReadFromJsonAsync<JsonElement>();

        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        json.GetProperty("code").GetString().Should().Be("work_from_home_duplicate_date");
    }
}

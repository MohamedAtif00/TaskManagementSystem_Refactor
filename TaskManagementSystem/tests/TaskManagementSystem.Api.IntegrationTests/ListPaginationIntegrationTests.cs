using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Contracts.Identity;
using TaskManagementSystem.Api.Contracts.Sprints;
using TaskManagementSystem.TestCommon.Integration;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class ListPaginationIntegrationTests(TmsWebApplicationFactory factory)
    : IClassFixture<TmsWebApplicationFactory>
{
    [Theory]
    [InlineData("/curriculum/subjects?page=0&pageSize=20")]
    [InlineData("/curriculum/subjects?page=1&pageSize=101")]
    [InlineData("/sprints?page=0&pageSize=20")]
    [InlineData("/sprints?page=1&pageSize=101")]
    [InlineData("/identity/users?page=0&pageSize=20")]
    [InlineData("/identity/users?page=1&pageSize=101")]
    [InlineData("/hr/holidays?page=0&pageSize=20")]
    [InlineData("/hr/holidays?page=1&pageSize=101")]
    [InlineData("/hr/leave/balances?page=0&pageSize=20")]
    [InlineData("/hr/leave/balances?page=1&pageSize=101")]
    [InlineData("/hr/leave/leave-requests/search?page=0&pageSize=20")]
    [InlineData("/hr/leave/leave-requests/search?page=1&pageSize=101")]
    public async Task PagedEndpoints_WhenPagingIsInvalid_Return400(string url)
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ListSubjects_ReturnsStablePagedContract()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        await TicketIntegrationTests.CreateTicketSetupAsync(client);

        var response = await client.GetAsync("/curriculum/subjects?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<SubjectCatalogPageResponse>();
        page.Should().NotBeNull();
        page!.Page.Should().Be(1);
        page.PageSize.Should().Be(20);
        page.TotalCount.Should().BeGreaterThan(0);
        page.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ListSprints_WhenPaged_ReturnsStablePagedContract()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/sprints?page=1&pageSize=20&archived=false");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<SprintListPageResponse>();
        page.Should().NotBeNull();
        page!.Page.Should().Be(1);
        page.PageSize.Should().Be(20);
        page.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ListUsers_WhenPaged_ReturnsStablePagedContract()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/identity/users?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<UserListPageResponse>();
        page.Should().NotBeNull();
        page!.Page.Should().Be(1);
        page.PageSize.Should().Be(20);
        page.TotalCount.Should().BeGreaterThan(0);
        page.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ListUsers_WhenSearchChangesFilter_ReturnsFilteredTotalCount()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var allResponse = await client.GetAsync("/identity/users?page=1&pageSize=20");
        var allPage = await allResponse.Content.ReadFromJsonAsync<UserListPageResponse>();

        var filteredResponse = await client.GetAsync(
            $"/identity/users?page=1&pageSize=20&search={IntegrationTestDataSeeder.TestUserName}");
        var filteredPage = await filteredResponse.Content.ReadFromJsonAsync<UserListPageResponse>();

        filteredPage!.TotalCount.Should().BeLessThanOrEqualTo(allPage!.TotalCount);
        filteredPage.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ListHolidays_WhenPaged_ReturnsStablePagedContract()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/hr/holidays?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<HolidayListPageResponse>();
        page.Should().NotBeNull();
        page!.Page.Should().Be(1);
        page.PageSize.Should().Be(20);
        page.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ListMemberBalances_WhenPaged_ReturnsStablePagedContract()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/hr/leave/balances?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<MemberBalanceListPageResponse>();
        page.Should().NotBeNull();
        page!.Page.Should().Be(1);
        page.PageSize.Should().Be(20);
        page.TotalCount.Should().BeGreaterThan(0);
        page.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchLeaveRequests_ReturnsStablePagedContract()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/hr/leave/leave-requests/search?page=1&pageSize=20&status=Pending");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<LeaveRequestListResponse>();
        page.Should().NotBeNull();
        page!.Page.Should().Be(1);
        page.PageSize.Should().Be(20);
        page.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }
}

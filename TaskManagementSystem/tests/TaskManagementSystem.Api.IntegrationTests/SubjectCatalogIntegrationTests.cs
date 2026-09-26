using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Modules.Curriculum.Domain;
using TaskManagementSystem.TestCommon.Integration;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class SubjectCatalogIntegrationTests(TmsWebApplicationFactory factory)
    : IClassFixture<TmsWebApplicationFactory>
{
    [Fact]
    public async Task ListSubjects_WhenActiveOnly_ReturnsOnlyActiveSubjects()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var setup = await TicketIntegrationTests.CreateTicketSetupAsync(client);

        var holdResponse = await client.PutAsJsonAsync(
            $"/curriculum/subjects/{setup.SubjectId}/status",
            new UpdateSubjectStatusRequest { Status = SubjectStatus.Hold });
        holdResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var allResponse = await client.GetAsync("/curriculum/subjects?page=1&pageSize=20");
        allResponse.StatusCode.Should().Be(HttpStatusCode.OK, await allResponse.Content.ReadAsStringAsync());
        var allPage = await allResponse.Content.ReadFromJsonAsync<SubjectCatalogPageResponse>();
        allPage!.Items.Should().Contain(item => item.Id == setup.SubjectId && item.Status == SubjectStatus.Hold);

        var activeResponse = await client.GetAsync("/curriculum/subjects?page=1&pageSize=20&activeOnly=true");
        activeResponse.StatusCode.Should().Be(HttpStatusCode.OK, await activeResponse.Content.ReadAsStringAsync());
        var activePage = await activeResponse.Content.ReadFromJsonAsync<SubjectCatalogPageResponse>();
        activePage!.Items.Should().NotContain(item => item.Id == setup.SubjectId);
        activePage.Items.Should().OnlyContain(item => item.Status == SubjectStatus.Active);
        activePage.TotalCount.Should().BeLessThan(allPage.TotalCount);
    }

    [Fact]
    public async Task ExportSubjects_WhenActiveOnly_ExcludesHoldSubjects()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var setup = await TicketIntegrationTests.CreateTicketSetupAsync(client);

        await client.PutAsJsonAsync(
            $"/curriculum/subjects/{setup.SubjectId}/status",
            new UpdateSubjectStatusRequest { Status = SubjectStatus.Closed });

        var exportResponse = await client.GetAsync("/curriculum/subjects/export?activeOnly=true");
        exportResponse.StatusCode.Should().Be(HttpStatusCode.OK, await exportResponse.Content.ReadAsStringAsync());
        var rows = await exportResponse.Content.ReadFromJsonAsync<List<SubjectCatalogListItemResponse>>();
        rows!.Should().NotContain(item => item.Id == setup.SubjectId);
        rows.Should().OnlyContain(item => item.Status == SubjectStatus.Active);
    }
}

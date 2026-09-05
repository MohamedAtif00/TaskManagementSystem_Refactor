using FluentAssertions;
using TaskManagementSystem.Api.IntegrationTests;
using TaskManagementSystem.TestCommon.Integration;

namespace TaskManagementSystem.Api.IntegrationTests;

[Collection(ApiIntegrationTestCollection.Name)]
public sealed class HostStartupTests(TmsWebApplicationFactory factory)
{
    [Fact]
    public void CreateClient_WhenHostStarts_ReturnsClient()
    {
        var client = factory.CreateClient();

        client.Should().NotBeNull();
        client.BaseAddress.Should().NotBeNull();
    }
}

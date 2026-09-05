using TaskManagementSystem.TestCommon.Integration;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class ApiIntegrationTestCollection : ICollectionFixture<TmsWebApplicationFactory>
{
    public const string Name = "IntegrationTests";
}

using FluentAssertions;
using TaskManagementSystem.Database;
using Xunit;

namespace TaskManagementSystem.BuildingBlocks.UnitTests;

public sealed class SqlScriptSeederTests
{
    [Fact]
    public void SplitIntoBatches_WhenScriptUsesGoSeparators_ReturnsExecutableBatches()
    {
        const string script = """
            SELECT 1;
            GO
            SELECT 2;
            """;

        var batches = SqlScriptSeeder.SplitIntoBatches(script).ToList();

        batches.Should().HaveCount(2);
        batches[0].Trim().Should().Be("SELECT 1;");
        batches[1].Trim().Should().Be("SELECT 2;");
    }
}

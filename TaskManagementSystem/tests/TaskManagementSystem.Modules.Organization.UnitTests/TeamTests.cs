using FluentAssertions;
using TaskManagementSystem.Modules.Organization.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.Organization.UnitTests;

public sealed class TeamTests
{
    [Fact]
    public void Create_WhenNameProvided_ReturnsTeam()
    {
        var result = Team.Create("Platform");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Platform");
        result.Value.Archived.Should().BeFalse();
    }

    [Fact]
    public void Create_WhenNameMissing_ReturnsValidationError()
    {
        var result = Team.Create("   ");

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("team_invalid_name");
    }

    [Fact]
    public void Archive_WhenActive_SetsArchived()
    {
        var team = Team.Create("Platform").Value;

        var result = team.Archive();

        result.IsSuccess.Should().BeTrue();
        team.Archived.Should().BeTrue();
    }
}

using FluentAssertions;
using TaskManagementSystem.Modules.Organization.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.Organization.UnitTests;

public sealed class SectionTests
{
    [Fact]
    public void Create_WhenValid_ReturnsSectionWithTeamLinks()
    {
        var result = Section.Create("Engineering", 5);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Engineering");
        result.Value.HeadId.Should().Be(5);

        result.Value.ReplaceTeamLinks([1, 2]);
        result.Value.SectionTeams.Should().HaveCount(2);
    }

    [Fact]
    public void Create_WhenHeadMissing_ReturnsValidationError()
    {
        var result = Section.Create("Engineering", 0);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("section_invalid_head");
    }

    [Fact]
    public void ReplaceTeamLinks_DeduplicatesTeamIds()
    {
        var section = Section.Create("Engineering", 5).Value;

        section.ReplaceTeamLinks([1, 1, 2]);

        section.SectionTeams.Select(link => link.TeamId).Should().BeEquivalentTo([1, 2]);
    }
}

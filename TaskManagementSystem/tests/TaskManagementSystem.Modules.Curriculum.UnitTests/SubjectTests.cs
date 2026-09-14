using FluentAssertions;
using TaskManagementSystem.Modules.Curriculum.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.Curriculum.UnitTests;

public sealed class SubjectTests
{
    [Fact]
    public void Create_WhenValid_ReturnsActiveSubject()
    {
        var result = Subject.Create("Mathematics", "Core math subject", 1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Mathematics");
        result.Value.Status.Should().Be(SubjectStatus.Active);
        result.Value.ArchivedWithFolder.Should().BeFalse();
    }

    [Fact]
    public void Create_WhenGroupMissing_ReturnsValidationError()
    {
        var result = Subject.Create("Mathematics", "Core math subject", 0);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("subject_invalid_group");
    }

    [Fact]
    public void Archive_WhenActive_SetsArchivedFlag()
    {
        var subject = Subject.Create("Mathematics", "Core math subject", 1).Value;

        var archiveResult = subject.Archive();

        archiveResult.IsSuccess.Should().BeTrue();
        subject.Archived.Should().BeTrue();
    }
}

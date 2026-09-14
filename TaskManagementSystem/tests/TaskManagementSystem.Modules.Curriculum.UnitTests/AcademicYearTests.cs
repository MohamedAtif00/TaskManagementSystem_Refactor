using FluentAssertions;
using TaskManagementSystem.Modules.Curriculum.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.Curriculum.UnitTests;

public sealed class AcademicYearTests
{
    [Fact]
    public void Create_WhenValid_ReturnsAcademicYear()
    {
        var result = AcademicYear.Create("2025-2026", "Current academic year");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("2025-2026");
        result.Value.Description.Should().Be("Current academic year");
        result.Value.Archived.Should().BeFalse();
    }

    [Fact]
    public void Create_WhenNameMissing_ReturnsValidationError()
    {
        var result = AcademicYear.Create(" ", null);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("academic_year_invalid_name");
    }

    [Fact]
    public void Archive_WhenActive_SetsArchivedFlag()
    {
        var year = AcademicYear.Create("2025-2026", null).Value;

        var archiveResult = year.Archive();

        archiveResult.IsSuccess.Should().BeTrue();
        year.Archived.Should().BeTrue();
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ReturnsError()
    {
        var year = AcademicYear.Create("2025-2026", null).Value;
        year.Archive();

        var archiveResult = year.Archive();

        archiveResult.IsSuccess.Should().BeFalse();
        archiveResult.Error.Code.Should().Be("academic_year_already_archived");
    }
}

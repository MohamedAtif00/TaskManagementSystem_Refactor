using FluentAssertions;
using NSubstitute;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;
using TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.CreateAcademicYear;
using Xunit;

namespace TaskManagementSystem.Modules.Curriculum.UnitTests;

public sealed class CreateAcademicYearCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenNameValid_PersistsAcademicYear()
    {
        var unitOfWork = Substitute.For<ICurriculumUnitOfWork>();
        AcademicYear? savedYear = null;
        unitOfWork.AcademicYears.AddAsync(Arg.Do<AcademicYear>(year => savedYear = year), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var handler = new CreateAcademicYearCommandHandler(unitOfWork);
        var result = await handler.Handle(
            new CreateAcademicYearCommand("2025-2026", "Current year"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        savedYear.Should().NotBeNull();
        savedYear!.Name.Should().Be("2025-2026");
        await unitOfWork.AcademicYears.Received(1).AddAsync(Arg.Any<AcademicYear>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}

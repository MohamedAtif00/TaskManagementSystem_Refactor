using FluentAssertions;
using NSubstitute;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Features.AboutMe;
using Xunit;

namespace TaskManagementSystem.Modules.Identity.UnitTests;

public sealed class AboutMeQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserExists_ReturnsProfile()
    {
        var aboutMeRepository = Substitute.For<IAboutMeRepository>();
        aboutMeRepository.GetAsync(42, Arg.Any<CancellationToken>())
            .Returns(new AboutMeReadModel(42, "Test User", UserRole.Owner, "Team A", 3));

        var unitOfWork = Substitute.For<IIdentityUnitOfWork>();
        unitOfWork.AboutMe.Returns(aboutMeRepository);

        var handler = new AboutMeQueryHandler(unitOfWork);

        var result = await handler.Handle(new AboutMeQuery(42), CancellationToken.None);

        result.Id.Should().Be(42);
        result.Name.Should().Be("Test User");
        result.Role.Should().Be(UserRole.Owner);
        result.Group.Should().Be("Team A");
        result.Notifications.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsUserNotFoundException()
    {
        var aboutMeRepository = Substitute.For<IAboutMeRepository>();
        aboutMeRepository.GetAsync(99, Arg.Any<CancellationToken>())
            .Returns((AboutMeReadModel?)null);

        var unitOfWork = Substitute.For<IIdentityUnitOfWork>();
        unitOfWork.AboutMe.Returns(aboutMeRepository);

        var handler = new AboutMeQueryHandler(unitOfWork);

        var act = () => handler.Handle(new AboutMeQuery(99), CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundException>();
    }
}

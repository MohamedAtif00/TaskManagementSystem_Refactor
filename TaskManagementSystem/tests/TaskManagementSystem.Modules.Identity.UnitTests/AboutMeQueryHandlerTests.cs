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
            .Returns(new AboutMeReadModel(
                42,
                "Test User",
                (int)UserRole.Owner,
                nameof(UserRole.Owner),
                [IdentityPermissionCodes.PermissionsManage],
                "Team A",
                3));

        var unitOfWork = Substitute.For<IIdentityUnitOfWork>();
        unitOfWork.AboutMe.Returns(aboutMeRepository);

        var handler = new AboutMeQueryHandler(unitOfWork);

        var result = await handler.Handle(new AboutMeQuery(42), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(42);
        result.Value.Name.Should().Be("Test User");
        result.Value.Role.Should().Be((int)UserRole.Owner);
        result.Value.RoleName.Should().Be(nameof(UserRole.Owner));
        result.Value.Permissions.Should().Contain(IdentityPermissionCodes.PermissionsManage);
        result.Value.Group.Should().Be("Team A");
        result.Value.Notifications.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsUserNotFoundError()
    {
        var aboutMeRepository = Substitute.For<IAboutMeRepository>();
        aboutMeRepository.GetAsync(99, Arg.Any<CancellationToken>())
            .Returns((AboutMeReadModel?)null);

        var unitOfWork = Substitute.For<IIdentityUnitOfWork>();
        unitOfWork.AboutMe.Returns(aboutMeRepository);

        var handler = new AboutMeQueryHandler(unitOfWork);

        var result = await handler.Handle(new AboutMeQuery(99), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(IdentityErrors.UserNotFound);
    }
}

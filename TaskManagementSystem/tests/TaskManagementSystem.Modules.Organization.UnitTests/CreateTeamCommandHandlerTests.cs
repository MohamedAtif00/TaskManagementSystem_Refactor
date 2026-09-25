using FluentAssertions;
using NSubstitute;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.Organization.Application;
using TaskManagementSystem.Modules.Organization.Domain;
using TaskManagementSystem.Modules.Organization.Features.Teams.CreateTeam;
using TaskManagementSystem.Modules.Organization.Infrastructure.Persistence.Queries;
using Xunit;

namespace TaskManagementSystem.Modules.Organization.UnitTests;

public sealed class CreateTeamCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenNameValid_PersistsTeam()
    {
        var unitOfWork = Substitute.For<IOrganizationUnitOfWork>();
        Team? savedTeam = null;
        unitOfWork.Teams.AddAsync(Arg.Do<Team>(team => savedTeam = team), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var identityLookupQueries = new IdentityLookupQueries(Substitute.For<ISqlConnectionFactory>());
        var handler = new CreateTeamCommandHandler(unitOfWork, identityLookupQueries);
        var result = await handler.Handle(new CreateTeamCommand("Platform", null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        savedTeam.Should().NotBeNull();
        savedTeam!.Name.Should().Be("Platform");
        await unitOfWork.Teams.Received(1).AddAsync(Arg.Any<Team>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}

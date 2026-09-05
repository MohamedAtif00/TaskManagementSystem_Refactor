using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Application.Behaviors;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.TestCommon.MediatR;
using Xunit;

namespace TaskManagementSystem.BuildingBlocks.UnitTests;

public sealed class AddBuildingBlocksTests
{
    [Fact]
    public void AddBuildingBlocks_RegistersMediatorAndPipelineBehaviors()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped(_ => Substitute.For<IAuditContext>());
        services.AddScoped(_ => Substitute.For<IAuditStore>());

        services.AddBuildingBlocks(typeof(ValidatedCommand).Assembly, typeof(Entity).Assembly);

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IMediator>().Should().NotBeNull();
        provider.GetServices<IPipelineBehavior<ValidatedCommand, string>>()
            .Select(behavior => behavior.GetType())
            .Should()
            .Contain(typeof(TracingBehavior<ValidatedCommand, string>))
            .And.Contain(typeof(AuditBehavior<ValidatedCommand, string>))
            .And.Contain(typeof(LoggingBehavior<ValidatedCommand, string>))
            .And.Contain(typeof(ValidationBehavior<ValidatedCommand, string>));
        provider.GetRequiredService<IValidator<ValidatedCommand>>().Should().NotBeNull();
    }
}

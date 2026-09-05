using System.Reflection;
using NetArchTest.Rules;
using TaskManagementSystem.Api.Hubs;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Notifications.Contracts;
using Xunit;

namespace TaskManagementSystem.ArchitectureTests;

public sealed class RealtimeBoundaryTests
{
    private const string SignalR = "Microsoft.AspNetCore.SignalR";

    private static readonly string[] ModuleAssemblies =
    [
        "TaskManagementSystem.Modules.Curriculum",
        "TaskManagementSystem.Modules.HR",
        "TaskManagementSystem.Modules.Identity",
        "TaskManagementSystem.Modules.Notifications",
        "TaskManagementSystem.Modules.Organization",
        "TaskManagementSystem.Modules.Sprints",
        "TaskManagementSystem.Modules.Ticket",
        "TaskManagementSystem.Modules.Workflows"
    ];

    [Fact]
    public void BuildingBlocks_ShouldNot_DependOnSignalR()
    {
        var result = Types.InAssembly(typeof(IRealtimePublisher).Assembly)
            .ShouldNot()
            .HaveDependencyOn(SignalR)
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result));
    }

    [Fact]
    public void DomainModules_ShouldNot_DependOnSignalR()
    {
        foreach (var assemblyName in ModuleAssemblies)
        {
            var result = Types.InAssembly(Assembly.Load(assemblyName))
                .ShouldNot()
                .HaveDependencyOn(SignalR)
                .GetResult();

            Assert.True(result.IsSuccessful, $"{assemblyName}: {Format(result)}");
        }
    }

    [Fact]
    public void Notifications_ShouldNot_DependOnTicket()
    {
        var result = Types.InAssembly(typeof(NotificationRealtime).Assembly)
            .ShouldNot()
            .HaveDependencyOn("TaskManagementSystem.Modules.Ticket")
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result));
    }

    [Fact]
    public void Api_Should_HostTheRealtimeHub()
    {
        Assert.Equal("/realtime", RealtimeHub.Path);
        Assert.Equal("Hub", typeof(RealtimeHub).BaseType?.Name);
    }

    private static string Format(TestResult result) =>
        result.FailingTypeNames is null
            ? "architecture rule failed"
            : string.Join(", ", result.FailingTypeNames);
}

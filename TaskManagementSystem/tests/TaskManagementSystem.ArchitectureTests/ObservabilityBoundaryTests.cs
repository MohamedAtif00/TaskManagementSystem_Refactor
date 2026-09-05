using System.Reflection;
using NetArchTest.Rules;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Observability;
using Xunit;

namespace TaskManagementSystem.ArchitectureTests;

public sealed class ObservabilityBoundaryTests
{
    private const string OpenTelemetry = "OpenTelemetry";

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
    public void BuildingBlocks_ShouldNot_DependOnOpenTelemetry()
    {
        var result = Types.InAssembly(typeof(Telemetry).Assembly)
            .ShouldNot()
            .HaveDependencyOn(OpenTelemetry)
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result));
    }

    [Fact]
    public void DomainModules_ShouldNot_DependOnOpenTelemetry()
    {
        foreach (var assemblyName in ModuleAssemblies)
        {
            var result = Types.InAssembly(Assembly.Load(assemblyName))
                .ShouldNot()
                .HaveDependencyOn(OpenTelemetry)
                .GetResult();

            Assert.True(result.IsSuccessful, $"{assemblyName}: {Format(result)}");
        }
    }

    [Fact]
    public void Api_Should_ReferenceOpenTelemetry()
    {
        Assert.Contains(
            typeof(Program).Assembly.GetReferencedAssemblies(),
            name => name.Name is not null && name.Name.StartsWith(OpenTelemetry, StringComparison.Ordinal));
    }

    private static string Format(TestResult result) =>
        result.FailingTypeNames is null
            ? "architecture rule failed"
            : string.Join(", ", result.FailingTypeNames);
}

using System.Reflection;
using NetArchTest.Rules;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.BuildingBlocks.Persistence;
using Xunit;

namespace TaskManagementSystem.ArchitectureTests;

public sealed class PersistenceBoundaryTests
{
    private const string Persistence = "TaskManagementSystem.BuildingBlocks.Persistence";

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
    public void ApplicationLayers_ShouldNot_DependOnPersistenceBuildingBlocks()
    {
        foreach (var assemblyName in ModuleAssemblies)
        {
            var result = Types.InAssembly(Assembly.Load(assemblyName))
                .That()
                .ResideInNamespaceMatching(".*\\.Application")
                .ShouldNot()
                .HaveDependencyOn(Persistence)
                .GetResult();

            Assert.True(result.IsSuccessful, $"{assemblyName}: {Format(result)}");
        }
    }

    [Fact]
    public void BuildingBlocksCore_ShouldNot_DependOnEntityFrameworkCore()
    {
        var result = Types.InAssembly(typeof(Entity).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result));
    }

    private static string Format(TestResult result) =>
        result.FailingTypeNames is null
            ? "architecture rule failed"
            : string.Join(", ", result.FailingTypeNames);
}

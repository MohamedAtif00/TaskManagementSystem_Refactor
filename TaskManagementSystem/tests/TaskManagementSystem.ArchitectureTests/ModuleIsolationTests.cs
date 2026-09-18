using System.Reflection;
using NetArchTest.Rules;
using Xunit;

namespace TaskManagementSystem.ArchitectureTests;

public sealed class ModuleIsolationTests
{
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
    public void Modules_ShouldNot_ReferenceOtherModules()
    {
        foreach (var assemblyName in ModuleAssemblies)
        {
            foreach (var otherAssembly in ModuleAssemblies.Where(name => name != assemblyName))
            {
                var result = Types.InAssembly(Assembly.Load(assemblyName))
                    .ShouldNot()
                    .HaveDependencyOn(otherAssembly)
                    .GetResult();

                Assert.True(result.IsSuccessful, $"{assemblyName} -> {otherAssembly}: {Format(result)}");
            }
        }
    }

    private static string Format(TestResult result) =>
        result.FailingTypeNames is null
            ? "architecture rule failed"
            : string.Join(", ", result.FailingTypeNames);
}

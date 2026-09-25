using System.Reflection;
using Xunit;

namespace TaskManagementSystem.ArchitectureTests;

public sealed class FeaturesBoundaryTests
{
    private static readonly string[] ModuleAssemblies =
    [
        "TaskManagementSystem.Modules.Curriculum",
        "TaskManagementSystem.Modules.HR",
        "TaskManagementSystem.Modules.Identity",
        "TaskManagementSystem.Modules.Notifications",
        "TaskManagementSystem.Modules.Organization",
        "TaskManagementSystem.Modules.Sprints",
        "TaskManagementSystem.Modules.Analytics",
        "TaskManagementSystem.Modules.Ticket",
        "TaskManagementSystem.Modules.Workflows"
    ];

    [Fact]
    public void IntegrationFeatures_ShouldNot_DependOnModuleInfrastructure()
    {
        foreach (var assemblyName in ModuleAssemblies)
        {
            var assembly = Assembly.Load(assemblyName);
            var offenders = assembly.GetTypes()
                .Where(type => type.Namespace?.Contains(".Features.Integration", StringComparison.Ordinal) == true)
                .SelectMany(type => GetInfrastructureDependencies(type, assemblyName))
                .Distinct()
                .OrderBy(value => value)
                .ToList();

            Assert.True(
                offenders.Count == 0,
                $"{assemblyName} integration features must depend on Application ports only: {string.Join(", ", offenders)}");
        }
    }

    private static IEnumerable<string> GetInfrastructureDependencies(Type type, string assemblyName)
    {
        var modulePrefix = assemblyName.Replace("TaskManagementSystem.", string.Empty, StringComparison.Ordinal);
        var infrastructureMarker = $".Modules.{modulePrefix.Replace("Modules.", string.Empty, StringComparison.Ordinal)}.Infrastructure";

        foreach (var dependency in GetReferencedTypes(type))
        {
            if (dependency.Namespace is null ||
                !dependency.Namespace.Contains(infrastructureMarker, StringComparison.Ordinal))
            {
                continue;
            }

            if (dependency.Namespace.Contains(".Infrastructure.Persistence.Queries", StringComparison.Ordinal))
            {
                continue;
            }

            yield return $"{type.FullName} -> {dependency.FullName}";
        }
    }

    private static IEnumerable<Type> GetReferencedTypes(Type type)
    {
        foreach (var constructor in type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            foreach (var parameter in constructor.GetParameters())
            {
                yield return parameter.ParameterType;
            }
        }

        foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
        {
            if (method.ReturnType != typeof(void))
            {
                yield return method.ReturnType;
            }

            foreach (var parameter in method.GetParameters())
            {
                yield return parameter.ParameterType;
            }
        }

        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            yield return property.PropertyType;
        }

        foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            yield return field.FieldType;
        }
    }
}

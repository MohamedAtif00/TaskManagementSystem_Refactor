using FluentAssertions;
using TaskManagementSystem.Database;
using TaskManagementSystem.Modules.Identity.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.Identity.UnitTests;

public sealed class PermissionCodesTests
{
    [Fact]
    public void All_ContainsUniqueLowercaseDottedCodes()
    {
        PermissionCodes.All.Should().NotBeEmpty();
        PermissionCodes.All.Should().OnlyHaveUniqueItems();
        PermissionCodes.All.Should().OnlyContain(code => code == code.ToLowerInvariant());
        PermissionCodes.All.Should().OnlyContain(code => code.Contains('.'));
    }

    [Fact]
    public void All_MatchesMigrationCatalog()
    {
        var migrationPath = SqlScriptSeeder.ResolveScriptPath(
            "src/Database/TaskManagementSystem.Database/Scripts/Migrations/019_identity_PermissionCatalog.sql");
        var migrationSql = File.ReadAllText(migrationPath);

        var migrationCodes = ExtractMigrationPermissionCodes(migrationSql);
        migrationCodes.Should().BeEquivalentTo(PermissionCodes.All);
    }

    [Theory]
    [InlineData("organization.manage", "organization.read", true)]
    [InlineData("organization.manage", "organization.create", true)]
    [InlineData("organization.read", "organization.create", false)]
    [InlineData("hr.leave.manage", "hr.leave.update", true)]
    [InlineData("identity.roles.manage", "identity.roles.read", true)]
    public void IsSatisfiedBy_ManageSuperset_WorksAsExpected(
        string heldPermission,
        string requiredPermission,
        bool expected)
    {
        PermissionCodes.IsSatisfiedBy(heldPermission, requiredPermission).Should().Be(expected);
    }

    private static IReadOnlyList<string> ExtractMigrationPermissionCodes(string migrationSql)
    {
        const string marker = "(N'";
        var codes = new List<string>();

        foreach (var line in migrationSql.Split('\n'))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith("(N'", StringComparison.Ordinal))
            {
                continue;
            }

            var start = trimmed.IndexOf("(N'", StringComparison.Ordinal) + 3;
            var end = trimmed.IndexOf('\'', start);
            if (end <= start)
            {
                continue;
            }

            codes.Add(trimmed[start..end]);
        }

        return codes;
    }
}

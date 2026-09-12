using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Infrastructure.Security;
using Xunit;

namespace TaskManagementSystem.Modules.Identity.UnitTests;

public sealed class JwtTokenGeneratorTests
{
    [Fact]
    public void CreateAccessToken_IncludesRoleAndPermissionClaims()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSetting:Token"] = TmsWebApplicationFactory.TestJwtSigningKey
            })
            .Build();

        var generator = new JwtTokenGenerator(configuration, TimeProvider.System);
        var ownerRole = Role.Create(nameof(UserRole.Owner), null, isSystem: true).Value;
        ownerRole.Id = (int)UserRole.Owner;
        ownerRole.Permissions.Add(
            Permission.Create(IdentityPermissionCodes.PermissionsManage, "Manage permissions", null, true).Value);

        var user = User.CreateForPersistence();
        user.Id = 42;
        user.RoleId = (int)UserRole.Owner;
        user.Role = ownerRole;
        user.Code = "TST001";
        user.Name = "Test User";
        user.HrCode = "999999";
        user.AccountType = AccountType.Internal;

        var token = generator.CreateAccessToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Claims.Should().Contain(claim => claim.Type == "Id" && claim.Value == "42");
        jwt.Claims.Should().Contain(claim => claim.Type == ClaimTypes.Role && claim.Value == nameof(UserRole.Owner));
        jwt.Claims.Should().Contain(claim =>
            claim.Type == IdentityClaimTypes.Permission &&
            claim.Value == IdentityPermissionCodes.PermissionsManage);
        jwt.Claims.Should().Contain(claim => claim.Type == ClaimTypes.NameIdentifier && claim.Value == "42");
    }
}

internal static class TmsWebApplicationFactory
{
    public const string TestJwtSigningKey = "TaskManagementSystemTestSigningKeyMustBe32Chars!";
}

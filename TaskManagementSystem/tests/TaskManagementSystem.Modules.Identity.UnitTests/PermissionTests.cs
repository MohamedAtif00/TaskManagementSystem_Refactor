using FluentAssertions;
using TaskManagementSystem.Modules.Identity.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.Identity.UnitTests;

public sealed class PermissionTests
{
    [Fact]
    public void Create_WhenCodeIsInvalid_ReturnsError()
    {
        var result = Permission.Create("INVALID", "Name", null);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("invalid_permission_code");
    }

    [Fact]
    public void Create_WhenCodeIsValid_NormalizesAndCreatesPermission()
    {
        var result = Permission.Create("HR.Holidays.Manage", "Manage holidays", "desc");

        result.IsSuccess.Should().BeTrue();
        result.Value.Code.Should().Be("hr.holidays.manage");
        result.Value.Name.Should().Be("Manage holidays");
    }

    [Fact]
    public void CanDelete_WhenSystemPermission_ReturnsError()
    {
        var permission = Permission.Create(IdentityPermissionCodes.RolesManage, "Manage roles", null, true).Value;

        var result = permission.CanDelete();

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("cannot_delete_system_permission");
    }
}

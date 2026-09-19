using FluentAssertions;
using TaskManagementSystem.Modules.HR.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.HR.UnitTests;

public sealed class EmployeeBalanceRecordTests
{
    [Fact]
    public void CreateWithDefaultEntitlements_SetsStandardEntitlementValues()
    {
        var balance = EmployeeBalanceRecord.CreateWithDefaultEntitlements(
            userId: 42,
            teamId: 1,
            teamleaderId: 2,
            roleId: 3);

        balance.UserId.Should().Be(42);
        balance.TeamId.Should().Be(1);
        balance.TeamleaderId.Should().Be(2);
        balance.Role.Should().Be(3);
        balance.AnnualLeave.Should().Be(0);
        balance.AnnualLeaveMax.Should().Be(30);
        balance.EmergencyLeave.Should().Be(0);
        balance.EmergencyLeaveMax.Should().Be(5);
        balance.SickLeave.Should().Be(0);
        balance.Permission.Should().Be(0);
        balance.PermissionMax.Should().Be(10);
        balance.WorkFromHome.Should().Be(0);
        balance.WorkFromHomeMax.Should().Be(5);
        balance.FromNextBalanceDaysUsed.Should().Be(0);
        balance.OldAnnualBalance.Should().Be(0);
    }
}

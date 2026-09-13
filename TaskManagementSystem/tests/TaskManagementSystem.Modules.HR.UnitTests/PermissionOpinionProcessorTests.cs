using FluentAssertions;
using NSubstitute;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.HR.UnitTests;

public sealed class PermissionOpinionProcessorTests
{
    private static readonly DateTime UtcNow = new(2026, 9, 7, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task ProcessAsync_WhenTeamLeaderRecordsOpinion_LeavesStatusPending()
    {
        var permission = CreatePendingPermission();
        var unitOfWork = Substitute.For<IHrUnitOfWork>();
        unitOfWork.PermissionRequests.GetByIdTrackedAsync(1, Arg.Any<CancellationToken>()).Returns(permission);
        unitOfWork.Opinions.ExistsForPermissionUserAsync(1, 2, Arg.Any<CancellationToken>()).Returns(false);

        var processor = new PermissionOpinionProcessor(unitOfWork);
        var result = await processor.ProcessAsync(2, "TeamLeader", 1, true, "OK", UtcNow, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        permission.Status.Should().Be(PermissionStatus.Pending);
        await unitOfWork.EmployeeBalances.DidNotReceive().DeductPermissionAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_WhenOwnerApproves_DeductsPermissionBalance()
    {
        var permission = CreatePendingPermission();
        var balance = new EmployeeBalance { Id = 1, Permission = 0, PermissionMax = 10 };

        var employeeBalances = Substitute.For<IEmployeeBalanceRepository>();
        employeeBalances.GetByUserIdAsync(1, Arg.Any<CancellationToken>()).Returns(balance);

        var permissionRequests = Substitute.For<IPermissionRequestRepository>();
        permissionRequests.GetByIdTrackedAsync(1, Arg.Any<CancellationToken>()).Returns(permission);
        permissionRequests.CountPendingAsync(1, 1, Arg.Any<CancellationToken>()).Returns(0);

        var unitOfWork = Substitute.For<IHrUnitOfWork>();
        unitOfWork.PermissionRequests.Returns(permissionRequests);
        unitOfWork.EmployeeBalances.Returns(employeeBalances);
        unitOfWork.Opinions.ExistsForPermissionUserAsync(1, 99, Arg.Any<CancellationToken>()).Returns(false);

        var processor = new PermissionOpinionProcessor(unitOfWork);
        var result = await processor.ProcessAsync(99, "Owner", 1, true, null, UtcNow, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        permission.Status.Should().Be(PermissionStatus.Approved);
        await employeeBalances.Received(1).DeductPermissionAsync(1, Arg.Any<CancellationToken>());
    }

    private static PermissionRequest CreatePendingPermission()
    {
        return PermissionRequest.Create(
            1,
            PermissionType.LateArrival,
            new DateTime(2027, 1, 5),
            new TimeOnly(10, 0),
            new TimeOnly(12, 0),
            null,
            null,
            null,
            UtcNow).Value;
    }
}

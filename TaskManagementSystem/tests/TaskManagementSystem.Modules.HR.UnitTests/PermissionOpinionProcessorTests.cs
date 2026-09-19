using FluentAssertions;
using NSubstitute;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.HR.UnitTests;

public sealed class PermissionOpinionProcessorTests
{
    [Fact]
    public async Task ProcessAsync_WhenAtomicDeductFails_LeavesRequestPending()
    {
        var utcNow = DateTime.UtcNow;
        var permission = PermissionRequest.Create(
            userId: 7,
            type: PermissionType.WorkAssignment,
            permissionDate: utcNow.Date,
            fromTime: new TimeOnly(9, 0),
            toTime: new TimeOnly(10, 0),
            reason: "Doctor",
            teamleaderId: null,
            sectionheadId: null,
            utcNow: utcNow).Value;
        permission.Id = 99;

        var unitOfWork = Substitute.For<IHrUnitOfWork>();
        unitOfWork.PermissionRequests.GetByIdTrackedAsync(permission.Id, Arg.Any<CancellationToken>())
            .Returns(permission);
        unitOfWork.Opinions.ExistsForPermissionUserAsync(permission.Id, 1, Arg.Any<CancellationToken>())
            .Returns(false);
        unitOfWork.EmployeeBalances.GetByUserIdAsync(permission.UserId, Arg.Any<CancellationToken>())
            .Returns(new EmployeeBalance
            {
                Id = permission.UserId,
                Permission = 9,
                PermissionMax = 10
            });
        unitOfWork.PermissionRequests.CountPendingAsync(permission.UserId, permission.Id, Arg.Any<CancellationToken>())
            .Returns(0);
        unitOfWork.EmployeeBalances.DeductPermissionAsync(permission.UserId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Fail(HrErrors.PermissionInsufficientBalance)));

        var processor = new PermissionOpinionProcessor(unitOfWork);
        var result = await processor.ProcessAsync(
            actorUserId: 1,
            actorRole: "Owner",
            permissionId: permission.Id,
            isApproved: true,
            comment: null,
            utcNow: utcNow,
            cancellationToken: CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        permission.Status.Should().Be(PermissionStatus.Pending);
        await unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }
}

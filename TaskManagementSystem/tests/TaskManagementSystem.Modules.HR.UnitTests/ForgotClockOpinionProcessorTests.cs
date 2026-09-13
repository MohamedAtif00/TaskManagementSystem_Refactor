using FluentAssertions;
using NSubstitute;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.HR.UnitTests;

public sealed class ForgotClockOpinionProcessorTests
{
    private static readonly DateTime UtcNow = new(2026, 9, 7, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task ProcessAsync_WhenTeamLeaderRecordsOpinion_LeavesStatusPending()
    {
        var request = CreatePendingRequest();
        var unitOfWork = Substitute.For<IHrUnitOfWork>();
        unitOfWork.ForgotClockRequests.GetByIdTrackedAsync(1, Arg.Any<CancellationToken>()).Returns(request);
        unitOfWork.Opinions.ExistsForForgotClockUserAsync(1, 2, Arg.Any<CancellationToken>()).Returns(false);

        var processor = new ForgotClockOpinionProcessor(unitOfWork);
        var result = await processor.ProcessAsync(2, "TeamLeader", 1, true, "OK", UtcNow, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(ForgotClockStatus.Pending);
        await unitOfWork.EmployeeBalances.DidNotReceive().DeductPermissionAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_WhenOwnerApproves_UpdatesStatusWithoutBalanceChange()
    {
        var request = CreatePendingRequest();
        var unitOfWork = Substitute.For<IHrUnitOfWork>();
        unitOfWork.ForgotClockRequests.GetByIdTrackedAsync(1, Arg.Any<CancellationToken>()).Returns(request);
        unitOfWork.Opinions.ExistsForForgotClockUserAsync(1, 99, Arg.Any<CancellationToken>()).Returns(false);

        var processor = new ForgotClockOpinionProcessor(unitOfWork);
        var result = await processor.ProcessAsync(99, "Owner", 1, true, null, UtcNow, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(ForgotClockStatus.Approved);
        await unitOfWork.EmployeeBalances.DidNotReceive().DeductWorkFromHomeAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    private static ForgotClockRequest CreatePendingRequest() =>
        ForgotClockRequest.Create(
            1,
            ForgotClockPunchType.ClockIn,
            new DateTime(2026, 9, 5),
            new TimeOnly(9, 0),
            null,
            null,
            null,
            UtcNow).Value;
}

using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave.RequestLeave;
using TaskManagementSystem.Modules.HR.Infrastructure;
using Xunit;

namespace TaskManagementSystem.Modules.HR.UnitTests;

public sealed class RequestLeaveCommandHandlerTests
{
    private static readonly DateTime UtcNow = new(2026, 9, 7, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Handle_WhenBalanceIsSufficient_PersistsPendingLeaveRequest()
    {
        var balance = new EmployeeBalance
        {
            Id = 1,
            AnnualLeave = 0,
            AnnualLeaveMax = 30
        };

        var employeeBalances = Substitute.For<IEmployeeBalanceRepository>();
        employeeBalances.GetByUserIdAsync(1, Arg.Any<CancellationToken>()).Returns(balance);

        var leaveRequests = Substitute.For<ILeaveRequestRepository>();
        leaveRequests.SumPendingWorkingDaysAsync(
                1,
                LeaveType.Annual,
                null,
                Arg.Any<CancellationToken>())
            .Returns(0);

        var unitOfWork = Substitute.For<IHrUnitOfWork>();
        unitOfWork.EmployeeBalances.Returns(employeeBalances);
        unitOfWork.LeaveRequests.Returns(leaveRequests);

        var holidayRepository = Substitute.For<IHolidayRepository>();
        holidayRepository.GetInRangeAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var planner = new LeaveRequestPlanner(
            Options.Create(new LeaveSettingsOptions
            {
                FromNextBalanceStartDate = "01-01",
                FromNextBalanceEndDate = "12-31"
            }),
            new WorkingDayCalculatorService(holidayRepository));

        var medicalStorage = Substitute.For<IMedicalCertificateStorage>();
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(new DateTimeOffset(UtcNow));

        var handler = new RequestLeaveCommandHandler(
            unitOfWork,
            planner,
            medicalStorage,
            timeProvider);

        var result = await handler.Handle(
            new RequestLeaveCommand(
                1,
                "Member",
                LeaveType.Annual,
                new DateTime(2027, 1, 4),
                new DateTime(2027, 1, 6),
                "Vacation",
                null,
                false,
                false,
                null,
                null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.WorkingDays.Should().Be(3);
        result.Value.Status.Should().Be(LeaveStatus.Pending);
        await leaveRequests.Received(1).AddAsync(Arg.Any<LeaveRequest>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenBalanceIsInsufficient_ReturnsInsufficientBalanceError()
    {
        var balance = new EmployeeBalance
        {
            Id = 1,
            AnnualLeave = 28,
            AnnualLeaveMax = 30
        };

        var employeeBalances = Substitute.For<IEmployeeBalanceRepository>();
        employeeBalances.GetByUserIdAsync(1, Arg.Any<CancellationToken>()).Returns(balance);

        var leaveRequests = Substitute.For<ILeaveRequestRepository>();
        leaveRequests.SumPendingWorkingDaysAsync(
                1,
                LeaveType.Annual,
                null,
                Arg.Any<CancellationToken>())
            .Returns(0);

        var unitOfWork = Substitute.For<IHrUnitOfWork>();
        unitOfWork.EmployeeBalances.Returns(employeeBalances);
        unitOfWork.LeaveRequests.Returns(leaveRequests);

        var holidayRepository = Substitute.For<IHolidayRepository>();
        holidayRepository.GetInRangeAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var planner = new LeaveRequestPlanner(
            Options.Create(new LeaveSettingsOptions()),
            new WorkingDayCalculatorService(holidayRepository));
        var medicalStorage = Substitute.For<IMedicalCertificateStorage>();
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(new DateTimeOffset(UtcNow));

        var handler = new RequestLeaveCommandHandler(
            unitOfWork,
            planner,
            medicalStorage,
            timeProvider);

        var result = await handler.Handle(
            new RequestLeaveCommand(
                1,
                "Member",
                LeaveType.Annual,
                new DateTime(2027, 1, 4),
                new DateTime(2027, 1, 6),
                null,
                null,
                false,
                false,
                null,
                null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(HrErrors.InsufficientBalance);
    }
}

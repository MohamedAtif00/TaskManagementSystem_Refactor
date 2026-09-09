using FluentAssertions;
using MediatR;
using NSubstitute;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.Modules.HR.Features.Leave.ApproveLeaveRequest;
using TaskManagementSystem.Modules.HR.Features.Leave.GiveLeaveOpinion;
using Xunit;

namespace TaskManagementSystem.Modules.HR.UnitTests;

public sealed class ApproveLeaveRequestCommandHandlerTests
{
    [Fact]
    public async Task Handle_DelegatesToGiveLeaveOpinionWithOwnerApproval()
    {
        var leaveRequestResult = new LeaveRequestResult(
            10,
            1,
            LeaveType.Annual,
            LeaveStatus.Approved,
            new DateTime(2027, 1, 4),
            new DateTime(2027, 1, 6),
            3,
            null,
            null,
            DateTime.UtcNow);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(
                Arg.Is<GiveLeaveOpinionCommand>(command =>
                    command.ActorUserId == 99 &&
                    command.ActorRole == "Owner" &&
                    command.LeaveRequestId == 10 &&
                    command.IsApproved),
                Arg.Any<CancellationToken>())
            .Returns(TaskManagementSystem.BuildingBlocks.Domain.Result.Ok(leaveRequestResult));

        var handler = new ApproveLeaveRequestCommandHandler(mediator);

        var result = await handler.Handle(new ApproveLeaveRequestCommand(99, 10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(LeaveStatus.Approved);
        await mediator.Received(1).Send(
            Arg.Any<GiveLeaveOpinionCommand>(),
            Arg.Any<CancellationToken>());
    }
}

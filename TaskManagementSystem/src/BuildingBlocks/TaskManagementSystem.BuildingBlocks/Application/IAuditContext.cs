using MediatR;

namespace TaskManagementSystem.BuildingBlocks.Application;

public interface IAuditContext
{
    int? UserId { get; }

    string CorrelationId { get; }
}

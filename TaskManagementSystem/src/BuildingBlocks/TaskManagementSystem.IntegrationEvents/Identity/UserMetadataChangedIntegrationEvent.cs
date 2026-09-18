using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.IntegrationEvents.Identity;

public sealed record UserMetadataChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOn,
    int UserId,
    int? TeamId,
    int? TeamleaderId,
    int RoleId) : IIntegrationEvent;

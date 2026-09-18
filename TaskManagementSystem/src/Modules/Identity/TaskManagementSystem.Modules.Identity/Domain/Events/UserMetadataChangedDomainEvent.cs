using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Identity.Domain.Events;

public sealed record UserMetadataChangedDomainEvent(
    int UserId,
    int? TeamId,
    int? TeamleaderId,
    int RoleId) : DomainEventBase;

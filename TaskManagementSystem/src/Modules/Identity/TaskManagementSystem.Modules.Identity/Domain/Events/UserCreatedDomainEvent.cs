using TaskManagementSystem.BuildingBlocks.Domain;



namespace TaskManagementSystem.Modules.Identity.Domain.Events;



public sealed record UserCreatedDomainEvent(

    int UserId,

    int? TeamId,

    int RoleId) : DomainEventBase;



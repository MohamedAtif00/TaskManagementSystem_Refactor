using TaskManagementSystem.BuildingBlocks.Application;



namespace TaskManagementSystem.IntegrationEvents.Identity;



public sealed record UserCreatedIntegrationEvent(

    Guid Id,

    DateTime OccurredOn,

    int UserId,

    int? TeamId,

    int RoleId) : IIntegrationEvent;



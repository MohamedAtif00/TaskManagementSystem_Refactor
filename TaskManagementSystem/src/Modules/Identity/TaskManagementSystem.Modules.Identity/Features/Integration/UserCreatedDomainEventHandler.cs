using MediatR;

using Microsoft.Extensions.DependencyInjection;

using TaskManagementSystem.BuildingBlocks.Application.Outbox;

using TaskManagementSystem.IntegrationEvents.Identity;

using TaskManagementSystem.Modules.Identity.Domain.Events;



namespace TaskManagementSystem.Modules.Identity.Features.Integration;



internal sealed class UserCreatedDomainEventHandler(

    [FromKeyedServices(IntegrationModuleKeys.Identity)] IOutboxWriter outboxWriter)

    : INotificationHandler<UserCreatedDomainEvent>

{

    public Task Handle(UserCreatedDomainEvent notification, CancellationToken cancellationToken)

    {

        var integrationEvent = new UserCreatedIntegrationEvent(

            Guid.NewGuid(),

            DateTime.UtcNow,

            notification.UserId,

            notification.TeamId,

            notification.RoleId);



        return outboxWriter.AddAsync(integrationEvent, cancellationToken);

    }

}


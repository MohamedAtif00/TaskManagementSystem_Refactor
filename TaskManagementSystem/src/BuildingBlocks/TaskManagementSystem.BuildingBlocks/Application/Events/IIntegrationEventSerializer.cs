namespace TaskManagementSystem.BuildingBlocks.Application.Events;

public interface IIntegrationEventSerializer
{
    string Serialize(IIntegrationEvent integrationEvent);

    IIntegrationEvent Deserialize(string type, string payload);
}

using System.Reflection;
using System.Text.Json;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Application.Events;

namespace TaskManagementSystem.BuildingBlocks.Persistence.Events;

public sealed class IntegrationEventSerializer : IIntegrationEventSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string Serialize(IIntegrationEvent integrationEvent) =>
        JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), SerializerOptions);

    public IIntegrationEvent Deserialize(string type, string payload)
    {
        var eventType = ResolveEventType(type);
        var integrationEvent = JsonSerializer.Deserialize(payload, eventType, SerializerOptions)
            as IIntegrationEvent;

        return integrationEvent ?? throw new InvalidOperationException($"Failed to deserialize integration event type '{type}'.");
    }

    private static Type ResolveEventType(string type)
    {
        var resolved = Type.GetType(type, throwOnError: false);
        if (resolved is not null)
        {
            return resolved;
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            resolved = assembly.GetType(type, throwOnError: false);
            if (resolved is not null)
            {
                return resolved;
            }

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                types = exception.Types.Where(t => t is not null).Cast<Type>().ToArray();
            }

            resolved = types.FirstOrDefault(candidate =>
                string.Equals(candidate.FullName, type, StringComparison.Ordinal)
                || string.Equals(candidate.AssemblyQualifiedName, type, StringComparison.Ordinal));

            if (resolved is not null)
            {
                return resolved;
            }
        }

        throw new InvalidOperationException($"Unknown integration event type '{type}'.");
    }
}

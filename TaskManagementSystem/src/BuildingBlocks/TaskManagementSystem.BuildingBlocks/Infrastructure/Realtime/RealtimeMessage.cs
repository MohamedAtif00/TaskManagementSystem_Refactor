namespace TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;

public sealed record RealtimeMessage(
    string EventName,
    RealtimeDeliveryKind Kind,
    object? Payload);

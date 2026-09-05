using Bogus;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;

namespace TaskManagementSystem.TestCommon.Builders;

public sealed class RealtimeMessageBuilder
{
    private static readonly Faker Faker = new();

    private string _eventName = "test.event";
    private RealtimeDeliveryKind _kind = RealtimeDeliveryKind.Silent;
    private object? _payload = new { Id = Faker.Random.Int(1, 1000) };

    public RealtimeMessageBuilder WithEventName(string eventName)
    {
        _eventName = eventName;
        return this;
    }

    public RealtimeMessageBuilder WithKind(RealtimeDeliveryKind kind)
    {
        _kind = kind;
        return this;
    }

    public RealtimeMessageBuilder WithPayload(object? payload)
    {
        _payload = payload;
        return this;
    }

    public RealtimeMessage Build() => new(_eventName, _kind, _payload);
}

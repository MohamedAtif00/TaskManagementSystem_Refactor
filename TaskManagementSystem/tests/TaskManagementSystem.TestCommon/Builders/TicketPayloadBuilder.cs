using Bogus;

namespace TaskManagementSystem.TestCommon.Builders;

public sealed class TicketPayloadBuilder
{
    private static readonly Faker Faker = new();

    private int _ticketId = Faker.Random.Int(1, 10_000);
    private string _title = Faker.Lorem.Sentence(3);
    private string _status = "InProgress";

    public TicketPayloadBuilder WithTicketId(int ticketId)
    {
        _ticketId = ticketId;
        return this;
    }

    public TicketPayloadBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public TicketPayloadBuilder WithStatus(string status)
    {
        _status = status;
        return this;
    }

    public TicketUpdatePayload Build() => new(_ticketId, _title, _status);
}

public sealed record TicketUpdatePayload(int TicketId, string Title, string Status);

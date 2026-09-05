using Bogus;

namespace TaskManagementSystem.TestCommon.Builders;

public sealed class NotificationPayloadBuilder
{
    private static readonly Faker Faker = new();

    private Guid _notificationId = Guid.NewGuid();
    private string _userId = Faker.Random.Guid().ToString();
    private string _title = Faker.Lorem.Sentence(4);
    private string _body = Faker.Lorem.Paragraph();

    public NotificationPayloadBuilder WithNotificationId(Guid notificationId)
    {
        _notificationId = notificationId;
        return this;
    }

    public NotificationPayloadBuilder WithUserId(string userId)
    {
        _userId = userId;
        return this;
    }

    public NotificationPayloadBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public NotificationPayloadBuilder WithBody(string body)
    {
        _body = body;
        return this;
    }

    public NotificationPayload Build() => new(_notificationId, _userId, _title, _body);
}

public sealed record NotificationPayload(Guid NotificationId, string UserId, string Title, string Body);

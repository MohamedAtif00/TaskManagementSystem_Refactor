using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.BuildingBlocks.Application.Inbox;

namespace TaskManagementSystem.BuildingBlocks.Persistence.Inbox;

public sealed class InboxMessageConfiguration(string schema) : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages", schema);
        builder.HasKey(message => message.Id);
        builder.Property(message => message.ConsumerName).HasMaxLength(256).IsRequired();
        builder.Property(message => message.Type).HasMaxLength(512).IsRequired();
        builder.Property(message => message.Payload).IsRequired();
        builder.Property(message => message.Error).HasMaxLength(4000);
        builder.HasIndex(message => new { message.IntegrationEventId, message.ConsumerName }).IsUnique();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.BuildingBlocks.Application.Outbox;

namespace TaskManagementSystem.BuildingBlocks.Persistence.Outbox;

public sealed class OutboxMessageConfiguration(string schema) : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages", schema);
        builder.HasKey(message => message.Id);
        builder.Property(message => message.Type).HasMaxLength(512).IsRequired();
        builder.Property(message => message.Payload).IsRequired();
        builder.Property(message => message.Error).HasMaxLength(4000);
        builder.HasIndex(message => new { message.ProcessedOnUtc, message.OccurredOnUtc })
            .HasFilter("[ProcessedOnUtc] IS NULL");
    }
}

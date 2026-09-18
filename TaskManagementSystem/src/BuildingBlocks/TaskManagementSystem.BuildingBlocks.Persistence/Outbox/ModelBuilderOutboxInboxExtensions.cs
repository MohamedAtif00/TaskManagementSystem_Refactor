using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence.Inbox;

namespace TaskManagementSystem.BuildingBlocks.Persistence.Outbox;

public static class ModelBuilderOutboxInboxExtensions
{
    public static void ConfigureOutboxInbox(this ModelBuilder modelBuilder, string schema)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration(schema));
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration(schema));
    }
}

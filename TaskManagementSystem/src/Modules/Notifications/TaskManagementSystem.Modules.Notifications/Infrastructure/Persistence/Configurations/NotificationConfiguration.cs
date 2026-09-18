using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Notifications.Domain;

namespace TaskManagementSystem.Modules.Notifications.Infrastructure.Persistence.Configurations;

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> entity)
    {
        entity.ToTable("Notifications", "notifications");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).HasColumnName("Title");
        entity.Property(x => x.Message).HasColumnName("Message");
        entity.Property(x => x.Category).HasColumnName("Category");
        entity.Property(x => x.Type).HasColumnName("Type");
        entity.Property(x => x.Status).HasColumnName("Status");
        entity.Property(x => x.IsRead).HasColumnName("IsRead");
        entity.Property(x => x.HasActions).HasColumnName("HasActions");
        entity.Property(x => x.AdditionalData).HasColumnName("AdditionalData");
        entity.Property(x => x.RelatedEntityId).HasColumnName("RelatedEntityId");
        entity.Property(x => x.CreatedAt).HasColumnName("CreatedAt");
        entity.Property(x => x.UserId).HasColumnName("UserId");
    }
}

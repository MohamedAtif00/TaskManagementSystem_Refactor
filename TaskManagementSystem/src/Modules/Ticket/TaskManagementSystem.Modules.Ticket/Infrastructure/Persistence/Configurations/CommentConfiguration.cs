using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Configurations;

internal sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> entity)
    {
        entity.ToTable("Comments", "ticket");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Content).HasColumnName("Content");
        entity.Property(x => x.CreatedAt).HasColumnName("CreatedAt");
        entity.Property(x => x.Timestamp).HasColumnName("Timestamp");
        entity.Property(x => x.Archived).HasColumnName("Archived");
        entity.Property(x => x.LearningObjectiveId).HasColumnName("LearningObjectiveId");
        entity.Property(x => x.TicketId).HasColumnName("TicketId");
        entity.Property(x => x.UserId).HasColumnName("UserId");
        entity.Property(x => x.ChildId).HasColumnName("ChildId");
    }
}

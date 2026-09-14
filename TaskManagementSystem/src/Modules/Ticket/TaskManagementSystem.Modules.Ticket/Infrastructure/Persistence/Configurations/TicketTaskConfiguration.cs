using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Configurations;

internal sealed class TicketTaskConfiguration : IEntityTypeConfiguration<TicketTask>
{
    public void Configure(EntityTypeBuilder<TicketTask> entity)
    {
        entity.ToTable("Tasks", "ticket");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).HasColumnName("Name");
        entity.Property(x => x.Status).HasColumnName("Status");
        entity.Property(x => x.Priority).HasColumnName("Priority");
        entity.Property(x => x.Duration).HasColumnName("Duration");
        entity.Property(x => x.CreatedAt).HasColumnName("CreatedAt");
        entity.Property(x => x.Pause).HasColumnName("Pause");
        entity.Property(x => x.Attention).HasColumnName("Attention");
        entity.Property(x => x.Flagged).HasColumnName("Flagged");
        entity.Property(x => x.Tl).HasColumnName("TL");
        entity.Property(x => x.IsReview).HasColumnName("IsReview");
        entity.Property(x => x.IsRollback).HasColumnName("IsRollback");
        entity.Property(x => x.RollbackCount).HasColumnName("RollbackCount");
        entity.Property(x => x.Archived).HasColumnName("Archived");
        entity.Property(x => x.LearningObjectiveId).HasColumnName("LearningObjectiveId");
        entity.Property(x => x.StepId).HasColumnName("StepId");
        entity.Property(x => x.UserId).HasColumnName("UserId");
        entity.Property(x => x.TeamId).HasColumnName("TeamId");
        entity.Property(x => x.FromId).HasColumnName("FromId");
    }
}

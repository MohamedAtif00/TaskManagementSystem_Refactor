using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Configurations;

internal sealed class TaskWorkTimeConfiguration : IEntityTypeConfiguration<TaskWorkTime>
{
    public void Configure(EntityTypeBuilder<TaskWorkTime> entity)
    {
        entity.ToTable("TaskWorkTimes", "ticket");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.StartDate).HasColumnName("StartDate");
        entity.Property(x => x.EndDate).HasColumnName("EndDate");
        entity.Property(x => x.Duration).HasColumnName("Duration");
        entity.Property(x => x.EndReason).HasColumnName("EndReason");
        entity.Property(x => x.TaskId).HasColumnName("TaskId");
        entity.Property(x => x.UserId).HasColumnName("UserId");
    }
}

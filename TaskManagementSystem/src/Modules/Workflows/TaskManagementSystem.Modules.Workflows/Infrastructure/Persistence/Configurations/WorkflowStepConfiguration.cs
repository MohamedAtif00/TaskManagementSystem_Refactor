using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Infrastructure.Persistence.Configurations;

internal sealed class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
{
    public void Configure(EntityTypeBuilder<WorkflowStep> entity)
    {
        entity.ToTable("Steps", "workflows");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Order).HasColumnName("Order");
        entity.Property(x => x.Duration).HasColumnName("Duration");
        entity.Property(x => x.Priority).HasColumnName("Priority");
        entity.Property(x => x.Archived).HasColumnName("Archived");
        entity.Property(x => x.NodeId).HasColumnName("NodeId");
        entity.Property(x => x.TicketBankId).HasColumnName("TicketBankId");
    }
}

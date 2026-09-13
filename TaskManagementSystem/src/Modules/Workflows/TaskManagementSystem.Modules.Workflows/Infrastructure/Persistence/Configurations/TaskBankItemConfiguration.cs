using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Infrastructure.Persistence.Configurations;

internal sealed class TaskBankItemConfiguration : IEntityTypeConfiguration<TaskBankItem>
{
    public void Configure(EntityTypeBuilder<TaskBankItem> entity)
    {
        entity.ToTable("TaskBank", "workflows");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).HasColumnName("Name");
        entity.Property(x => x.Duration).HasColumnName("Duration");
        entity.Property(x => x.Type).HasColumnName("Type");
        entity.Property(x => x.Active).HasColumnName("Active");
        entity.Property(x => x.TeamLeaderOnly).HasColumnName("TL");
        entity.Property(x => x.TeamId).HasColumnName("TeamId");
    }
}

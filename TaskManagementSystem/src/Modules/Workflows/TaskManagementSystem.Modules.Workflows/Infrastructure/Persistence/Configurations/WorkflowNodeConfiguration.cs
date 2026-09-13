using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Infrastructure.Persistence.Configurations;

internal sealed class WorkflowNodeConfiguration : IEntityTypeConfiguration<WorkflowNode>
{
    public void Configure(EntityTypeBuilder<WorkflowNode> entity)
    {
        entity.ToTable("Nodes", "workflows");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).HasColumnName("Name");
        entity.Property(x => x.Order).HasColumnName("Order");
        entity.Property(x => x.IsStart).HasColumnName("isStart");
        entity.Property(x => x.IsEnd).HasColumnName("isEnd");
        entity.Property(x => x.Archived).HasColumnName("Archived");
        entity.Property(x => x.SchemaId).HasColumnName("SchemaId");
    }
}

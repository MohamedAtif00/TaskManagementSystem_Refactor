using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Infrastructure.Persistence.Configurations;

internal sealed class WorkflowSchemaConfiguration : IEntityTypeConfiguration<WorkflowSchema>
{
    public void Configure(EntityTypeBuilder<WorkflowSchema> entity)
    {
        entity.ToTable("Schemas", "workflows");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).HasColumnName("Name");
        entity.Property(x => x.Description).HasColumnName("Description");
        entity.Property(x => x.Archived).HasColumnName("Archived");
        entity.Property(x => x.TypeId).HasColumnName("TypeId");
    }
}

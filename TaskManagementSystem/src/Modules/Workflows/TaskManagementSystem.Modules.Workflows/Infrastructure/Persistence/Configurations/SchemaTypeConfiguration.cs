using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Infrastructure.Persistence.Configurations;

internal sealed class SchemaTypeConfiguration : IEntityTypeConfiguration<SchemaType>
{
    public void Configure(EntityTypeBuilder<SchemaType> entity)
    {
        entity.ToTable("SchemaTypes", "workflows");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).HasColumnName("Name");
        entity.Property(x => x.Description).HasColumnName("Description");
    }
}

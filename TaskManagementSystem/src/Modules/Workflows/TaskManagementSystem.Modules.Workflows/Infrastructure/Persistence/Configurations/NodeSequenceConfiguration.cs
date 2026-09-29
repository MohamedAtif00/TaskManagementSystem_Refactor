using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Infrastructure.Persistence.Configurations;

internal sealed class NodeSequenceConfiguration : IEntityTypeConfiguration<NodeSequence>
{
    public void Configure(EntityTypeBuilder<NodeSequence> entity)
    {
        entity.ToTable("NodeSequences", "workflows");
        entity.HasKey(x => new { x.NextId, x.PreviousId });
        entity.Property(x => x.NextId).HasColumnName("NextId");
        entity.Property(x => x.PreviousId).HasColumnName("PreviousId");
    }
}

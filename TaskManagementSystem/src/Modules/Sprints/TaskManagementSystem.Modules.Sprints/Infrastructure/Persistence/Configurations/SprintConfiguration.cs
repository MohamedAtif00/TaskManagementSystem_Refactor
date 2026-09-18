using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Sprints.Domain;

namespace TaskManagementSystem.Modules.Sprints.Infrastructure.Persistence.Configurations;

internal sealed class SprintConfiguration : IEntityTypeConfiguration<Sprint>
{
    public void Configure(EntityTypeBuilder<Sprint> entity)
    {
        entity.ToTable("Sprints", "sprints");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).HasColumnName("Name");
        entity.Property(x => x.Description).HasColumnName("Description");
        entity.Property(x => x.StartDate).HasColumnName("StartDate");
        entity.Property(x => x.EndDate).HasColumnName("EndDate");
        entity.Property(x => x.IsArchived).HasColumnName("IsArchived");
    }
}

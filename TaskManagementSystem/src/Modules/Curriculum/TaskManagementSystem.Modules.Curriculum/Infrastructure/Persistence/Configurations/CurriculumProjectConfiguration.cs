using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence.Configurations;

internal sealed class CurriculumProjectConfiguration : IEntityTypeConfiguration<CurriculumProject>
{
    public void Configure(EntityTypeBuilder<CurriculumProject> entity)
    {
        entity.ToTable("CurriculumProjects", "curriculum");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).HasColumnName("Name");
        entity.Property(x => x.Description).HasColumnName("Description");
        entity.Property(x => x.Archived).HasColumnName("Archived");
        entity.Property(x => x.YearId).HasColumnName("YearId");
    }
}

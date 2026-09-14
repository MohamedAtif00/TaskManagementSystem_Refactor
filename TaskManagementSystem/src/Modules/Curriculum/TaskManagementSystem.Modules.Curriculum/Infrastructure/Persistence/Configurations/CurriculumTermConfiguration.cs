using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence.Configurations;

internal sealed class CurriculumTermConfiguration : IEntityTypeConfiguration<CurriculumTerm>
{
    public void Configure(EntityTypeBuilder<CurriculumTerm> entity)
    {
        entity.ToTable("CurriculumTerms", "curriculum");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).HasColumnName("Name");
        entity.Property(x => x.StartDate).HasColumnName("StartDate");
        entity.Property(x => x.EndDate).HasColumnName("EndDate");
        entity.Property(x => x.Archived).HasColumnName("Archived");
        entity.Property(x => x.ProjectId).HasColumnName("ProjectId");
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence.Configurations;

internal sealed class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> entity)
    {
        entity.ToTable("Subjects", "curriculum");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).HasColumnName("Name");
        entity.Property(x => x.Description).HasColumnName("Description");
        entity.Property(x => x.Status).HasColumnName("Status");
        entity.Property(x => x.Archived).HasColumnName("Archived");
        entity.Property(x => x.ArchivedWithFolder).HasColumnName("ArchivedWithFolder");
        entity.Property(x => x.SubjectGroupId).HasColumnName("SubjectGroupId");
    }
}

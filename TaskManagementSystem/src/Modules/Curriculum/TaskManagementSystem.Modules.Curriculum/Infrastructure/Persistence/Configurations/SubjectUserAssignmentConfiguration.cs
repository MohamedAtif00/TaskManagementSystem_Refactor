using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence.Configurations;

internal sealed class SubjectUserAssignmentConfiguration : IEntityTypeConfiguration<SubjectUserAssignment>
{
    public void Configure(EntityTypeBuilder<SubjectUserAssignment> entity)
    {
        entity.ToTable("SubjectUser", "curriculum");
        entity.HasKey(x => new { x.SubjectsId, x.UsersId });
        entity.Property(x => x.SubjectsId).HasColumnName("SubjectsId");
        entity.Property(x => x.UsersId).HasColumnName("UsersId");
    }
}

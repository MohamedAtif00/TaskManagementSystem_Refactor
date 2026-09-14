using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence.Configurations;

internal sealed class LearningObjectiveConfiguration : IEntityTypeConfiguration<LearningObjective>
{
    public void Configure(EntityTypeBuilder<LearningObjective> entity)
    {
        entity.ToTable("LearningObjectives", "curriculum");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).HasColumnName("Name");
        entity.Property(x => x.Tag).HasColumnName("Tag");
        entity.Property(x => x.Template).HasColumnName("Template");
        entity.Property(x => x.Environment).HasColumnName("Environment");
        entity.Property(x => x.CreateAt).HasColumnName("CreateAt");
        entity.Property(x => x.StartedAt).HasColumnName("StartedAt");
        entity.Property(x => x.DoneAt).HasColumnName("DoneAt");
        entity.Property(x => x.Archived).HasColumnName("Archived");
        entity.Property(x => x.LessonId).HasColumnName("LessonId");
        entity.Property(x => x.SchemaId).HasColumnName("SchemaId");
    }
}

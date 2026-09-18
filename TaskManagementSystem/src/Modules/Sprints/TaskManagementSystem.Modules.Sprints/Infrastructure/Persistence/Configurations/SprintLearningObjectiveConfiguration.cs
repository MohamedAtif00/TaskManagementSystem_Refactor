using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Sprints.Domain;

namespace TaskManagementSystem.Modules.Sprints.Infrastructure.Persistence.Configurations;

internal sealed class SprintLearningObjectiveConfiguration : IEntityTypeConfiguration<SprintLearningObjective>
{
    public void Configure(EntityTypeBuilder<SprintLearningObjective> entity)
    {
        entity.ToTable("SprintLearningObjectives", "sprints");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.SprintId).HasColumnName("SprintId");
        entity.Property(x => x.LearningObjectiveId).HasColumnName("LearningObjectiveId");
    }
}

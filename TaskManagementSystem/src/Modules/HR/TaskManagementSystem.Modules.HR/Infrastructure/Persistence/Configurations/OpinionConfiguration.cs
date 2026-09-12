using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Configurations;

internal sealed class OpinionConfiguration : IEntityTypeConfiguration<Opinion>
{
    public void Configure(EntityTypeBuilder<Opinion> entity)
    {
        entity.ToTable("Opinions", "hr");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Comment);
        entity.HasIndex(x => x.LeaveRequestId);
        entity.HasIndex(x => x.UserId);
    }
}

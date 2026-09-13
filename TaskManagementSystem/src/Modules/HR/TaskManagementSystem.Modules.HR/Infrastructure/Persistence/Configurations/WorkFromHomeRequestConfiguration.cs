using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Configurations;

internal sealed class WorkFromHomeRequestConfiguration : IEntityTypeConfiguration<WorkFromHomeRequest>
{
    public void Configure(EntityTypeBuilder<WorkFromHomeRequest> entity)
    {
        entity.ToTable("WorkFromHomeRequests", "hr");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Status).HasConversion<int>();
        entity.Property(x => x.NoteForManager);
        entity.HasIndex(x => x.UserId);
    }
}

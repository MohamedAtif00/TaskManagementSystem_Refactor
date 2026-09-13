using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Configurations;

internal sealed class PermissionRequestConfiguration : IEntityTypeConfiguration<PermissionRequest>
{
    public void Configure(EntityTypeBuilder<PermissionRequest> entity)
    {
        entity.ToTable("Permissions", "hr");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Type).HasConversion<string>();
        entity.Property(x => x.Status).HasConversion<string>();
        entity.Property(x => x.Reason);
        entity.Property(x => x.FromTime);
        entity.Property(x => x.ToTime);
        entity.HasIndex(x => x.UserId);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Infrastructure.Persistence.Configurations;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> entity)
    {
        entity.ToTable("Permissions", "identity");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Code).HasMaxLength(200);
        entity.Property(x => x.Name).HasMaxLength(200);
        entity.Property(x => x.Description).HasMaxLength(500);
        entity.HasIndex(x => x.Code).IsUnique();
    }
}

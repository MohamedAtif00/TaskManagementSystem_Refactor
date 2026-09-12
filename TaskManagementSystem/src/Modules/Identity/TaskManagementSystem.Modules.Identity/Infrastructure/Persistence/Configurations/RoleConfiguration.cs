using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Infrastructure.Persistence.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> entity)
    {
        entity.ToTable("Roles", "identity");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).ValueGeneratedNever();
        entity.Property(x => x.Name).HasMaxLength(100);
        entity.Property(x => x.Description).HasMaxLength(500);
        entity.HasIndex(x => x.Name).IsUnique();

        entity.HasMany(role => role.Permissions)
            .WithMany(permission => permission.Roles)
            .UsingEntity<Dictionary<string, object>>(
                "RolePermissions",
                right => right
                    .HasOne<Permission>()
                    .WithMany()
                    .HasForeignKey("PermissionId"),
                left => left
                    .HasOne<Role>()
                    .WithMany()
                    .HasForeignKey("RoleId"),
                join =>
                {
                    join.ToTable("RolePermissions", "identity");
                    join.HasKey("RoleId", "PermissionId");
                });
    }
}

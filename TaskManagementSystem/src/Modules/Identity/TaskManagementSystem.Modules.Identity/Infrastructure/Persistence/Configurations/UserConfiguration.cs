using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("Users", "identity");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("Id");
        entity.Property(x => x.Code).HasColumnName("Code").HasMaxLength(6);
        entity.Property(x => x.Name).HasColumnName("Name");
        entity.Property(x => x.HrCode).HasColumnName("HR_code");
        entity.Property(x => x.RoleId).HasColumnName("Role");
        entity.Property(x => x.AccountType).HasColumnName("AccountType");
        entity.Property(x => x.OnBoard).HasColumnName("OnBoard");
        entity.Property(x => x.Archived).HasColumnName("Archived");
        entity.HasIndex(x => x.Code).IsUnique();
        entity.HasOne(x => x.Role)
            .WithMany(role => role.Users)
            .HasForeignKey(x => x.RoleId);
    }
}

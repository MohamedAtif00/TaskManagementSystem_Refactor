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
        entity.Property(x => x.Email).HasColumnName("Email");
        entity.Property(x => x.Phone).HasColumnName("Phone");
        entity.Property(x => x.Title).HasColumnName("Title");
        entity.Property(x => x.RoleId).HasColumnName("Role");
        entity.Property(x => x.AccountType).HasColumnName("AccountType");
        entity.Property(x => x.OnBoard).HasColumnName("OnBoard");
        entity.Property(x => x.Archived).HasColumnName("Archived");
        entity.Property(x => x.TeamId).HasColumnName("TeamId");
        entity.Property(x => x.TeamleaderId).HasColumnName("TeamleaderId");
        entity.Property(x => x.AnnualLeave).HasColumnName("Annual_leave");
        entity.Property(x => x.AnnualLeaveMax).HasColumnName("Annual_leave_MAX");
        entity.Property(x => x.EmergencyLeave).HasColumnName("Emergency_leave");
        entity.Property(x => x.EmergencyLeaveMax).HasColumnName("Emergency_leave_MAX");
        entity.Property(x => x.SickLeave).HasColumnName("Sick_leave");
        entity.Property(x => x.PermissionBalance).HasColumnName("Permission");
        entity.Property(x => x.PermissionMax).HasColumnName("Permission_MAX");
        entity.Property(x => x.WorkFromHome).HasColumnName("WorkFromHome");
        entity.Property(x => x.WorkFromHomeMax).HasColumnName("WorkFromHome_MAX");
        entity.Property(x => x.FromNextBalanceDaysUsed).HasColumnName("FromNextBalanceDaysUsed");
        entity.Property(x => x.OldAnnualBalance).HasColumnName("OldAnnualBalance");
        entity.HasIndex(x => x.Code).IsUnique();
        entity.HasOne(x => x.Role)
            .WithMany(role => role.Users)
            .HasForeignKey(x => x.RoleId);
    }
}

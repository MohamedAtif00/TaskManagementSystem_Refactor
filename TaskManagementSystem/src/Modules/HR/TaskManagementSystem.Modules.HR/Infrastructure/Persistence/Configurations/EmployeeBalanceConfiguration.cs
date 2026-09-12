using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Configurations;

internal sealed class EmployeeBalanceConfiguration : IEntityTypeConfiguration<EmployeeBalanceRecord>
{
    public void Configure(EntityTypeBuilder<EmployeeBalanceRecord> entity)
    {
        entity.ToTable("EmployeeBalances", "hr");
        entity.HasKey(x => x.UserId);
        entity.Property(x => x.UserId).HasColumnName("UserId");
        entity.Property(x => x.TeamId).HasColumnName("TeamId");
        entity.Property(x => x.TeamleaderId).HasColumnName("TeamleaderId");
        entity.Property(x => x.Role).HasColumnName("Role");
        entity.Property(x => x.AnnualLeave).HasColumnName("AnnualLeave");
        entity.Property(x => x.AnnualLeaveMax).HasColumnName("AnnualLeaveMax");
        entity.Property(x => x.EmergencyLeave).HasColumnName("EmergencyLeave");
        entity.Property(x => x.EmergencyLeaveMax).HasColumnName("EmergencyLeaveMax");
        entity.Property(x => x.SickLeave).HasColumnName("SickLeave");
        entity.Property(x => x.Permission).HasColumnName("Permission");
        entity.Property(x => x.PermissionMax).HasColumnName("PermissionMax");
        entity.Property(x => x.WorkFromHome).HasColumnName("WorkFromHome");
        entity.Property(x => x.WorkFromHomeMax).HasColumnName("WorkFromHomeMax");
        entity.Property(x => x.FromNextBalanceDaysUsed).HasColumnName("FromNextBalanceDaysUsed");
        entity.Property(x => x.OldAnnualBalance).HasColumnName("OldAnnualBalance");
    }
}

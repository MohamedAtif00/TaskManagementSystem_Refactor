using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Configurations;

internal sealed class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> entity)
    {
        entity.ToTable("LeaveRequests", "hr");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Type).HasConversion(new LeaveTypeValueConverter());
        entity.Property(x => x.Status).HasConversion<string>();
        entity.Property(x => x.Reason);
        entity.Property(x => x.NoteForManager);
        entity.Property(x => x.MedicalCertificateFileName);
        entity.Property(x => x.MedicalCertificatePath);
        entity.Property(x => x.WorkingDays);
        entity.HasIndex(x => x.UserId);
    }
}

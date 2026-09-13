using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Configurations;

internal sealed class ForgotClockRequestConfiguration : IEntityTypeConfiguration<ForgotClockRequest>
{
    public void Configure(EntityTypeBuilder<ForgotClockRequest> entity)
    {
        entity.ToTable("ForgotClockRequests", "hr");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.PunchType).HasConversion<string>();
        entity.Property(x => x.Status).HasConversion<string>();
        entity.Property(x => x.Reason);
        entity.Property(x => x.IntendedTime);
        entity.HasIndex(x => x.UserId);
    }
}

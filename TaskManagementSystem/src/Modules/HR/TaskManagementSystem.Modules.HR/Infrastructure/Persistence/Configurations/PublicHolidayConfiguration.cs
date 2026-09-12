using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Configurations;

internal sealed class PublicHolidayConfiguration : IEntityTypeConfiguration<PublicHoliday>
{
    public void Configure(EntityTypeBuilder<PublicHoliday> entity)
    {
        entity.ToTable("PublicHolidays", "hr");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).HasMaxLength(200);
        entity.Property(x => x.Description).HasMaxLength(1000);
        entity.HasIndex(x => new { x.StartDate, x.EndDate });
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Configurations;

internal sealed class TicketWorkTimeConfiguration : IEntityTypeConfiguration<TicketWorkTime>
{
    public void Configure(EntityTypeBuilder<TicketWorkTime> entity)
    {
        entity.ToTable("TicketWorkTimes", "ticket");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.StartDate).HasColumnName("StartDate");
        entity.Property(x => x.EndDate).HasColumnName("EndDate");
        entity.Property(x => x.Duration).HasColumnName("Duration");
        entity.Property(x => x.EndReason).HasColumnName("EndReason");
        entity.Property(x => x.TicketId).HasColumnName("TicketId");
        entity.Property(x => x.UserId).HasColumnName("UserId");
    }
}

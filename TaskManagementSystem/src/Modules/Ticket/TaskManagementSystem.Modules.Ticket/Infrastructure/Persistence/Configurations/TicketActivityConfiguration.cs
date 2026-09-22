using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Configurations;

internal sealed class TicketActivityConfiguration : IEntityTypeConfiguration<TicketActivity>
{
    public void Configure(EntityTypeBuilder<TicketActivity> entity)
    {
        entity.ToTable("TicketActivities", "ticket");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Type).HasColumnName("Type");
        entity.Property(x => x.TimeStamp).HasColumnName("TimeStamp");
        entity.Property(x => x.AdditionalInfo).HasColumnName("AdditionalInfo");
        entity.Property(x => x.TicketId).HasColumnName("TicketId");
        entity.Property(x => x.TicketSecondaryId).HasColumnName("TicketSecondaryId");
        entity.Property(x => x.ActorOneId).HasColumnName("ActorOneId");
        entity.Property(x => x.ActorTwoId).HasColumnName("ActorTwoId");
    }
}

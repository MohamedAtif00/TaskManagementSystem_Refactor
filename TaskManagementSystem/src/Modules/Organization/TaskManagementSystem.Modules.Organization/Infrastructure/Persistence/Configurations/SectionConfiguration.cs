using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Organization.Domain;

namespace TaskManagementSystem.Modules.Organization.Infrastructure.Persistence.Configurations;

internal sealed class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> entity)
    {
        entity.ToTable("Sections", "organization");
        entity.HasKey(section => section.Id);
        entity.Property(section => section.Name).IsRequired();
        entity.HasIndex(section => section.HeadId);
        entity.HasMany(section => section.SectionTeams)
            .WithOne(link => link.Section)
            .HasForeignKey(link => link.SectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Organization.Domain;

namespace TaskManagementSystem.Modules.Organization.Infrastructure.Persistence.Configurations;

internal sealed class SectionTeamConfiguration : IEntityTypeConfiguration<SectionTeam>
{
    public void Configure(EntityTypeBuilder<SectionTeam> entity)
    {
        entity.ToTable("SectionTeams", "organization");
        entity.HasKey(link => link.Id);
        entity.HasIndex(link => link.SectionId);
        entity.HasIndex(link => link.TeamId);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Organization.Domain;

namespace TaskManagementSystem.Modules.Organization.Infrastructure.Persistence.Configurations;

internal sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> entity)
    {
        entity.ToTable("Teams", "organization");
        entity.HasKey(team => team.Id);
        entity.Property(team => team.Name).IsRequired();
    }
}

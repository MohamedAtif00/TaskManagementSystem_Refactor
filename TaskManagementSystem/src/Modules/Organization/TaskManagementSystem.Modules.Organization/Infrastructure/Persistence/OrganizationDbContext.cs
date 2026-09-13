using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Modules.Organization.Domain;

namespace TaskManagementSystem.Modules.Organization.Infrastructure.Persistence;

public sealed class OrganizationDbContext(DbContextOptions<OrganizationDbContext> options) : DbContext(options)
{
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Section> Sections => Set<Section>();
    internal DbSet<SectionTeam> SectionTeams => Set<SectionTeam>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrganizationDbContext).Assembly);
}

using Microsoft.EntityFrameworkCore;

namespace TaskManagementSystem.BuildingBlocks.Persistence.Audit;

public sealed class AuditDbContext(DbContextOptions<AuditDbContext> options) : DbContext(options)
{
    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLogEntry>(entity =>
        {
            entity.ToTable("AuditLog", "app");
            entity.HasKey(entry => entry.Id);
            entity.Property(entry => entry.CorrelationId).HasMaxLength(64).IsRequired();
            entity.Property(entry => entry.ActionName).HasMaxLength(256).IsRequired();
            entity.Property(entry => entry.OccurredOnUtc).IsRequired();
            entity.Property(entry => entry.Success).IsRequired();
            entity.HasIndex(entry => entry.OccurredOnUtc);
            entity.HasIndex(entry => entry.UserId);
        });
    }
}

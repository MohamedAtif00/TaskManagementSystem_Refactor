using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Infrastructure.Persistence.ReadModels;

namespace TaskManagementSystem.Modules.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    internal DbSet<TeamReadModel> Teams => Set<TeamReadModel>();
    internal DbSet<NotificationReadModel> Notifications => Set<NotificationReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users", "identity");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Code).HasColumnName("Code").HasMaxLength(6);
            entity.Property(x => x.Name).HasColumnName("Name");
            entity.Property(x => x.HrCode).HasColumnName("HR_code");
            entity.Property(x => x.Role).HasColumnName("Role");
            entity.Property(x => x.AccountType).HasColumnName("AccountType");
            entity.Property(x => x.OnBoard).HasColumnName("OnBoard");
            entity.Property(x => x.Archived).HasColumnName("Archived");
            entity.Property(x => x.TeamId).HasColumnName("TeamId");
            entity.Property(x => x.TeamleaderId).HasColumnName("TeamleaderId");
            entity.Property(x => x.AnnualLeave).HasColumnName("Annual_leave");
            entity.Property(x => x.AnnualLeaveMax).HasColumnName("Annual_leave_MAX");
            entity.Property(x => x.EmergencyLeave).HasColumnName("Emergency_leave");
            entity.Property(x => x.EmergencyLeaveMax).HasColumnName("Emergency_leave_MAX");
            entity.Property(x => x.SickLeave).HasColumnName("Sick_leave");
            entity.Property(x => x.Permission).HasColumnName("Permission");
            entity.Property(x => x.PermissionMax).HasColumnName("Permission_MAX");
            entity.Property(x => x.WorkFromHome).HasColumnName("WorkFromHome");
            entity.Property(x => x.WorkFromHomeMax).HasColumnName("WorkFromHome_MAX");
            entity.Property(x => x.FromNextBalanceDaysUsed).HasColumnName("FromNextBalanceDaysUsed");
            entity.Property(x => x.OldAnnualBalance).HasColumnName("OldAnnualBalance");
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens", "identity");
            entity.HasKey(x => x.Token);
            entity.Property(x => x.Token).HasColumnName("Token").HasMaxLength(450);
            entity.Property(x => x.Created).HasColumnName("Created");
            entity.Property(x => x.Expires).HasColumnName("Expires");
            entity.Property(x => x.Used).HasColumnName("Used");
            entity.Property(x => x.UserId).HasColumnName("UserId");
            entity.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<TeamReadModel>(entity =>
        {
            entity.ToTable("Teams", "organization");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasColumnName("Name");
            entity.Property(x => x.Archived).HasColumnName("Archived");
        });

        modelBuilder.Entity<NotificationReadModel>(entity =>
        {
            entity.ToTable("Notifications", "notifications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.IsRead).HasColumnName("IsRead");
            entity.Property(x => x.UserId).HasColumnName("UserId");
            entity.HasIndex(x => x.UserId);
        });
    }
}

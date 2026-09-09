using Microsoft.EntityFrameworkCore;

using TaskManagementSystem.Modules.HR.Application;

using TaskManagementSystem.Modules.HR.Domain;



namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence;



public sealed class HrDbContext(DbContextOptions<HrDbContext> options) : DbContext(options)

{

    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

    public DbSet<Opinion> Opinions => Set<Opinion>();

    public DbSet<PublicHoliday> PublicHolidays => Set<PublicHoliday>();

    internal DbSet<EmployeeBalanceEntity> EmployeeBalances => Set<EmployeeBalanceEntity>();

    internal DbSet<EmployeeReadModel> Employees => Set<EmployeeReadModel>();

    internal DbSet<SectionTeamReadModel> SectionTeams => Set<SectionTeamReadModel>();
    internal DbSet<SectionReadModel> Sections => Set<SectionReadModel>();



    protected override void OnModelCreating(ModelBuilder modelBuilder)

    {

        modelBuilder.Entity<LeaveRequest>(entity =>

        {

            entity.ToTable("LeaveRequests", "hr");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Type).HasConversion<string>();

            entity.Property(x => x.Status).HasConversion<string>();

            entity.Property(x => x.Reason);

            entity.Property(x => x.NoteForManager);

            entity.Property(x => x.MedicalCertificateFileName);

            entity.Property(x => x.MedicalCertificatePath);

            entity.Property(x => x.WorkingDays);

            entity.HasIndex(x => x.UserId);

        });



        modelBuilder.Entity<Opinion>(entity =>

        {

            entity.ToTable("Opinions", "hr");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Comment);

            entity.HasIndex(x => x.LeaveRequestId);

            entity.HasIndex(x => x.UserId);

        });



        modelBuilder.Entity<PublicHoliday>(entity =>

        {

            entity.ToTable("PublicHolidays", "hr");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name).HasMaxLength(200);

            entity.Property(x => x.Description).HasMaxLength(1000);

            entity.HasIndex(x => new { x.StartDate, x.EndDate });

        });



        modelBuilder.Entity<EmployeeBalanceEntity>(entity =>

        {

            entity.ToTable("Users", "identity");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("Id");

            entity.Property(x => x.TeamId).HasColumnName("TeamId");

            entity.Property(x => x.TeamleaderId).HasColumnName("TeamleaderId");

            entity.Property(x => x.Role).HasColumnName("Role");

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

        });



        modelBuilder.Entity<EmployeeReadModel>(entity =>

        {

            entity.ToTable("Users", "identity");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("Id");

            entity.Property(x => x.Name).HasColumnName("Name");

            entity.Property(x => x.TeamId).HasColumnName("TeamId");

            entity.Property(x => x.TeamleaderId).HasColumnName("TeamleaderId");

            entity.Property(x => x.Role).HasColumnName("Role");

        });



        modelBuilder.Entity<EmployeeBalanceEntity>()

            .HasOne<EmployeeReadModel>()

            .WithOne()

            .HasForeignKey<EmployeeReadModel>(employee => employee.Id);



        modelBuilder.Entity<SectionTeamReadModel>(entity =>

        {

            entity.ToTable("SectionTeams", "organization");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.SectionId).HasColumnName("SectionId");

            entity.Property(x => x.TeamId).HasColumnName("TeamId");

        });



        modelBuilder.Entity<SectionReadModel>(entity =>

        {

            entity.ToTable("Sections", "organization");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.HeadId).HasColumnName("HeadId");

        });

    }

}



internal sealed class EmployeeBalanceEntity

{

    public int Id { get; set; }



    public int? TeamId { get; set; }



    public int? TeamleaderId { get; set; }



    public int Role { get; set; }



    public int AnnualLeave { get; set; }



    public int AnnualLeaveMax { get; set; }



    public int EmergencyLeave { get; set; }



    public int EmergencyLeaveMax { get; set; }



    public int SickLeave { get; set; }



    public int Permission { get; set; }



    public int PermissionMax { get; set; }



    public int WorkFromHome { get; set; }



    public int WorkFromHomeMax { get; set; }



    public int FromNextBalanceDaysUsed { get; set; }



    public int OldAnnualBalance { get; set; }

}



internal sealed class EmployeeReadModel

{

    public int Id { get; set; }



    public string Name { get; set; } = string.Empty;



    public int? TeamId { get; set; }



    public int? TeamleaderId { get; set; }



    public int Role { get; set; }

}



internal sealed class SectionTeamReadModel

{

    public int Id { get; set; }



    public int SectionId { get; set; }



    public int TeamId { get; set; }

}



internal sealed class SectionReadModel

{

    public int Id { get; set; }



    public int HeadId { get; set; }

}



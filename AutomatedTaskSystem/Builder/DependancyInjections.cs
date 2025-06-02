using AutomatedTaskSystem.Interfaces;
using AutomatedTaskSystem.Services;
using AutomatedTaskSystem.Services.AuthService;
using AutomatedTaskSystem.Services.GroupService;
using AutomatedTaskSystem.Services.LearningObjectiveService;
using AutomatedTaskSystem.Services.ProjectAssignmentService;
using AutomatedTaskSystem.Services.ProjectService;
using AutomatedTaskSystem.Services.SchemaService;
using AutomatedTaskSystem.Services.SectionService;
using AutomatedTaskSystem.Services.TaskService;
using AutomatedTaskSystem.Services.TokenService;
using AutomatedTaskSystem.Services.UnitService;
using AutomatedTaskSystem.Services.UserService;
using AutomatedTaskSystem.Services.YearService;
using AutomatedTaskSystem.Services.ReportService;
using AutomatedTaskSystem.Services.DashboardService;
using AutomatedTaskSystem.Services.RollbackService;
using AutomatedTaskSystem.Services.UserTask;
using AutomatedTaskSystem.Services.Sprint;
using AutomatedTaskSystem.Services.Leave;
using AutomatedTaskSystem.Services.Permission;
using AutomatedTaskSystem.Seeding;
using AutomatedTaskSystem.Services.Email;
using AutomatedTaskSystem.Services.Notification;
using AutomatedTaskSystem.Services.Log;

namespace AutomatedTaskSystem.Builder.DependancyInjections;

public static class DependancyInjections
{
    public static void Inject(WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IUserTaskService, UserTaskService>();
        builder.Services.AddScoped<IRollbackService, RollbackService>();
        builder.Services.AddScoped<IDashboardService, DashboardService>();
        builder.Services.AddScoped<IEncryptionService, EncryptionService>();
        builder.Services.AddScoped<IReportService, ReportService>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IProjectService, ProjectService>();
        builder.Services.AddScoped<IProjectAssignmentService, ProjectAssignmentService>();
        builder.Services.AddScoped<IUnitService, UnitService>();
        builder.Services.AddScoped<ILearningObjectiveService, LearningObjectiveService>();
        builder.Services.AddScoped<IGroupService, GroupService>();
        builder.Services.AddScoped<ISectionService, SectionService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ITaskService, TaskService>();
        builder.Services.AddScoped<ISchemaService, SchemaService>();
        builder.Services.AddScoped<IYearService, YearService>();
        builder.Services.AddScoped<ISprintService, SprintService>();
        builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
        builder.Services.AddScoped<IPermissionService, PermissionService>();


        // seeder and email sender
        builder.Services.AddScoped<DataSeeder>();
        builder.Services.AddTransient<IEmailService, EmailService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();
        builder.Services.AddScoped<ILogService, LogService>();
    }
}

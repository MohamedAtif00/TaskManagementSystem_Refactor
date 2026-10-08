using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Database;
using TaskManagementSystem.Api.Endpoints;
using TaskManagementSystem.Api.Endpoints.Auth;
using TaskManagementSystem.Api.Endpoints.Identity;
using TaskManagementSystem.Api.Endpoints.Organization;
using TaskManagementSystem.Api.Endpoints.Curriculum;
using TaskManagementSystem.Api.Endpoints.Notifications;
using TaskManagementSystem.Api.Endpoints.Analytics;
using TaskManagementSystem.Api.Endpoints.Sprints;
using TaskManagementSystem.Api.Endpoints.Ticket;
using TaskManagementSystem.Api.Endpoints.Workflows;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.BuildingBlocks.Persistence.Events;
using TaskManagementSystem.IntegrationEvents.Ticket;
using TaskManagementSystem.Modules.HR.Features.Leave.RequestLeave;
using TaskManagementSystem.Modules.HR.Infrastructure;
using TaskManagementSystem.Modules.Identity.Features.Authenticate;
using TaskManagementSystem.Modules.Identity.Infrastructure;
using TaskManagementSystem.Modules.Organization.Features.Teams.CreateTeam;
using TaskManagementSystem.Modules.Organization.Infrastructure;
using TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.CreateAcademicYear;
using TaskManagementSystem.Modules.Curriculum.Infrastructure;
using TaskManagementSystem.Modules.Notifications.Features.Notifications.ListMyNotifications;
using TaskManagementSystem.Modules.Notifications.Infrastructure;
using TaskManagementSystem.Modules.Analytics.Features.GetSubjectOverview;
using TaskManagementSystem.Modules.Analytics.Infrastructure;
using TaskManagementSystem.Modules.Sprints.Features.Sprints.CreateSprint;
using TaskManagementSystem.Modules.Sprints.Infrastructure;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.CreateTicket;
using TaskManagementSystem.Modules.Ticket.Infrastructure;
using TaskManagementSystem.Modules.Workflows.Features.Schemas.CreateSchema;
using TaskManagementSystem.Modules.Workflows.Infrastructure;

var promoteOnly = false;
var builderArgs = new List<string>(args.Length);
foreach (var arg in args)
{
    if (string.Equals(arg, "--promote-database", StringComparison.OrdinalIgnoreCase))
    {
        promoteOnly = true;
        continue;
    }

    builderArgs.Add(arg);
}

var builder = WebApplication.CreateBuilder(builderArgs.ToArray());

AppContext.SetSwitch(
    "Switch.Microsoft.Data.SqlClient.UseManagedNetworkingOnWindows",
    !builder.Environment.IsDevelopment());

DatabaseVersioning.Log("API process started. BaseDirectory=" + AppContext.BaseDirectory);

if (!DatabaseVersioning.Enabled || builder.Environment.IsEnvironment("Testing"))
{
    DatabaseVersioning.Log("Skipped. Versioning is disabled for this process.");
    if (promoteOnly)
    {
        Environment.ExitCode = 1;
        return;
    }
}
else
{
    var currentConnection = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(currentConnection))
    {
        DatabaseVersioning.Log("Skipped. DefaultConnection is empty.");
        if (promoteOnly)
        {
            Environment.ExitCode = 1;
            return;
        }
    }
    else
    {
        try
        {
            var scriptsRoot = Path.Combine(AppContext.BaseDirectory, "DatabaseScripts");
            var promoted = DatabaseVersionBootstrap.PromoteIfVersioned(currentConnection, scriptsRoot);
            if (promoted is null)
            {
                var configured = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(currentConnection);
                DatabaseVersioning.Log(
                    "Skipped. Database name '" + configured.InitialCatalog + "' is not SystemAdminDB_Test_vN.");
                if (promoteOnly)
                {
                    Environment.ExitCode = 1;
                    return;
                }
            }
            else
            {
                builder.Configuration["ConnectionStrings:DefaultConnection"] = promoted;
                var created = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(promoted);
                DatabaseVersioning.Log("Using database " + created.InitialCatalog + ".");
                if (promoteOnly)
                {
                    DatabaseVersioning.Log("Promotion finished. Exiting without starting the website.");
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            DatabaseVersioning.Log(ex.ToString());
            throw;
        }
    }
}

builder.AddObservability();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
builder.Services.AddScoped<IAuditContext, AuditContext>();
builder.Services.AddIdentityModule(builder.Configuration, builder.Environment);
builder.Services.AddHrModule(builder.Configuration, builder.Environment);
builder.Services.AddOrganizationModule(builder.Configuration, builder.Environment);
builder.Services.AddWorkflowsModule(builder.Configuration, builder.Environment);
builder.Services.AddCurriculumModule(builder.Configuration, builder.Environment);
builder.Services.AddSprintsModule(builder.Configuration, builder.Environment);
builder.Services.AddAnalyticsModule(builder.Configuration, builder.Environment);
builder.Services.AddTicketModule(builder.Configuration, builder.Environment);
builder.Services.AddNotificationsModule(builder.Configuration, builder.Environment);
builder.Services.AddAuditLog(builder.Configuration, builder.Environment);
builder.Services.AddIntegrationEvents();
builder.Services.AddBuildingBlocks(
    typeof(Program).Assembly,
    typeof(Entity).Assembly,
    typeof(AuthenticateCommand).Assembly,
    typeof(RequestLeaveCommand).Assembly,
    typeof(CreateTeamCommand).Assembly,
    typeof(CreateSchemaCommand).Assembly,
    typeof(CreateAcademicYearCommand).Assembly,
    typeof(CreateSprintCommand).Assembly,
    typeof(GetSubjectOverviewQuery).Assembly,
    typeof(CreateTicketCommand).Assembly,
    typeof(ListMyNotificationsQuery).Assembly,
    typeof(TicketAssignedIntegrationEvent).Assembly);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddFrontendCors(builder.Configuration);
builder.Services.AddApidogCors();
builder.Services.AddRealtime();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors(AuthenticationExtensions.CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapOpenApiEndpoints();
app.MapAuthEndpoints();
app.MapIdentityEndpoints();
app.MapHrEndpoints();
app.MapOrganizationEndpoints();
app.MapWorkflowsEndpoints();
app.MapCurriculumEndpoints();
app.MapSprintsEndpoints();
app.MapAnalyticsEndpoints();
app.MapTicketEndpoints();
app.MapNotificationsEndpoints();
app.MapRealtimeHub();

app.Run();

public partial class Program;

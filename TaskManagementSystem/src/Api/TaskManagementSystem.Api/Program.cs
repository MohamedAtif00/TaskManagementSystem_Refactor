using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Endpoints;
using TaskManagementSystem.Api.Endpoints.Auth;
using TaskManagementSystem.Api.Endpoints.Identity;
using TaskManagementSystem.Api.Endpoints.Organization;
using TaskManagementSystem.Api.Endpoints.Curriculum;
using TaskManagementSystem.Api.Endpoints.Notifications;
using TaskManagementSystem.Api.Endpoints.Sprints;
using TaskManagementSystem.Api.Endpoints.Ticket;
using TaskManagementSystem.Api.Endpoints.Workflows;
using TaskManagementSystem.Api.Infrastructure;
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
using TaskManagementSystem.Modules.Sprints.Features.Sprints.CreateSprint;
using TaskManagementSystem.Modules.Sprints.Infrastructure;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.CreateTicket;
using TaskManagementSystem.Modules.Ticket.Infrastructure;
using TaskManagementSystem.Modules.Workflows.Features.Schemas.CreateSchema;
using TaskManagementSystem.Modules.Workflows.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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
app.MapTicketEndpoints();
app.MapNotificationsEndpoints();
app.MapRealtimeHub();

app.Run();

public partial class Program;

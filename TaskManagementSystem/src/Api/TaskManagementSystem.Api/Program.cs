using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Endpoints;
using TaskManagementSystem.Api.Endpoints.Organization;
using TaskManagementSystem.Api.Endpoints.Workflows;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave.RequestLeave;
using TaskManagementSystem.Modules.HR.Infrastructure;
using TaskManagementSystem.Modules.Identity.Features.Authenticate;
using TaskManagementSystem.Modules.Identity.Infrastructure;
using TaskManagementSystem.Modules.Organization.Features.Teams.CreateTeam;
using TaskManagementSystem.Modules.Organization.Infrastructure;
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
builder.Services.AddAuditLog(builder.Configuration, builder.Environment);
builder.Services.AddBuildingBlocks(
    typeof(Program).Assembly,
    typeof(Entity).Assembly,
    typeof(AuthenticateCommand).Assembly,
    typeof(RequestLeaveCommand).Assembly,
    typeof(CreateTeamCommand).Assembly,
    typeof(CreateSchemaCommand).Assembly);
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
app.MapRealtimeHub();

app.Run();

public partial class Program;

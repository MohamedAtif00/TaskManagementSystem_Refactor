using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Endpoints;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Features.Authenticate;
using TaskManagementSystem.Modules.Identity.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
builder.Services.AddScoped<IAuditContext, AuditContext>();
builder.Services.AddIdentityModule(builder.Configuration, builder.Environment);
builder.Services.AddAuditLog(builder.Configuration, builder.Environment);
builder.Services.AddBuildingBlocks(
    typeof(Program).Assembly,
    typeof(Entity).Assembly,
    typeof(AuthenticateCommand).Assembly);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddFrontendCors(builder.Configuration);
builder.Services.AddRealtime();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors(AuthenticationExtensions.CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapAuthEndpoints();
app.MapRealtimeHub();

app.Run();

public partial class Program;

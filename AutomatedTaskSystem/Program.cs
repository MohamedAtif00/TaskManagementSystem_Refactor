global using Microsoft.EntityFrameworkCore;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Interfaces;
using AutomatedTaskSystem.Services;
using AutomatedTaskSystem.Services.AuthService;
using AutomatedTaskSystem.Services.GroupService;
using AutomatedTaskSystem.Services.PathService;
using AutomatedTaskSystem.Services.SectionService;
using AutomatedTaskSystem.Services.TaskService;
using AutomatedTaskSystem.Services.UserService;
using AutomatedTaskSystem.Services.LearningObjectiveService;
using AutomatedTaskSystem.Services.ProjectAssignmentService;
using AutomatedTaskSystem.Services.ProjectService;
using AutomatedTaskSystem.Services.UnitService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AutomatedTaskSystem.Services.TokenService;
using AutomatedTaskSystem.Services.SchemaService;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
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
builder.Services.AddScoped<IPathService, PathService>();
builder.Services.AddScoped<ISchemaService, SchemaService>();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration.GetSection("AppSetting:Token").Value)
            ),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
builder.Services.AddDbContext<DataContext>(opts =>
{
    string ConnString = builder.Configuration.GetConnectionString("DefaultConnection");
    opts.UseSqlServer(ConnString);
    opts.EnableDetailedErrors(true);
});
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: MyAllowSpecificOrigins,
        policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()
    );
});

var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();

app.Run();

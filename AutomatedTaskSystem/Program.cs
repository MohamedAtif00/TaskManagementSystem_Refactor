global using Microsoft.EntityFrameworkCore;
using AutomatedTaskSystem.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AutomatedTaskSystem.Builder.DependancyInjections;
using System.Text;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
DependancyInjections.Inject(builder);
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
    opts.UseSqlServer(
        ConnString,
        options =>
        {
            options.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: System.TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null
            );
        }
    );
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

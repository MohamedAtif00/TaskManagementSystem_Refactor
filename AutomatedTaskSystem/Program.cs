global using Microsoft.EntityFrameworkCore;
using AutomatedTaskSystem.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AutomatedTaskSystem.Builder.DependancyInjections;
using System.Text;
using Microsoft.OpenApi.Models;
using AutomatedTaskSystem.Hub;
using System.Security.Claims;
using AutomatedTaskSystem.Services;
using AutomatedTaskSystem.Middlewares;
using System;
using AutomatedTaskSystem.Seeding;
using AutomatedTaskSystem.Dtos;
using AutomatedTaskSystem.Converters;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
    //.AddJsonOptions(op =>
    // {
    //     op.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
    //});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Automated Task System API", Version = "1.0" });
    c.CustomSchemaIds(type => type.FullName);
});
// Add email settings configuration
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

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
            ValidateAudience = false,
            NameClaimType = ClaimTypes.NameIdentifier // ?? this line is VERY important!
        };
        opts.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // ?? Match your hub route if needed (example: "/userhub")
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/userhub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
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

//builder.Services.AddSingleton<ILogService>(provider =>
//    new FileLogService(Path.Combine(AppContext.BaseDirectory, "Logs")));

builder.Services.AddSignalR();
builder.Services.AddSingleton<UserConnectionService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: MyAllowSpecificOrigins,
        policy =>
            policy
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithOrigins("http://localhost:3000")
                .AllowCredentials()
    );
});




builder.Services.AddHttpClient("SSRSProxy").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AllowAutoRedirect = false
});


using (var serviceScope = builder.Services.BuildServiceProvider().CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<DataContext>();
    context.Database.Migrate();
}


var app = builder.Build();
app.UseCors(MyAllowSpecificOrigins);

app.UseMiddleware<ErrorLoggingMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Automated Task System API V1");

    });
}

app.UseDeveloperExceptionPage();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Automated Task System API V1");

});


// 👇 Call seeding logic here
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.Seed();
}


app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<UserHub>("/userhub");


app.Run();
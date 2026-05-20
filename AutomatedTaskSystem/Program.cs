global using Microsoft.EntityFrameworkCore;
using AutomatedTaskSystem.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AutomatedTaskSystem.Builder.DependancyInjections;
using System.Text;
//using Microsoft.OpenApi.Model;
using AutomatedTaskSystem.Hub;
using System.Security.Claims;
using AutomatedTaskSystem.Services;
using AutomatedTaskSystem.Middlewares;
using System;
using AutomatedTaskSystem.Seeding;
using AutomatedTaskSystem.Dtos;
using AutomatedTaskSystem.Converters;
using AutomatedTaskSystem.Services.Email;
using AutomatedTaskSystem.Models.Configs;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

try
{
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
    builder.Services.Configure<EmailRecipientSettings>(
    builder.Configuration.GetSection("EmailRecipients"));
    builder.Services.Configure<LeaveSettings>(builder.Configuration.GetSection(LeaveSettings.SectionName));

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

    builder.Services.AddHostedService<MonthlyDatabaseOperationWorker>();

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
                   .WithOrigins("http://localhost:3000",
                         "http://stdigital.stp.local",
                         "http://localhost:8081",
                         "https://localhost",      // Add this
                         "https://localhost:443", // Keep this
                         "https://ats.stp.local") // Keep this
                    .AllowCredentials()
        );
    });




    builder.Services.AddHttpClient("SSRSProxy").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        AllowAutoRedirect = false
    });


    var app = builder.Build();

    // 👇 Static files and CORS should come early in the pipeline
    app.UseStaticFiles();
    app.UseCors(MyAllowSpecificOrigins);

    // 👇 Exception handling should be very early in the pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error"); // You should implement this endpoint
    }

    // 👇 Our custom error logging middleware (after system exception handlers)
    app.UseMiddleware<ErrorLoggingMiddleware>();

    // 👇 Swagger should only be in development (unless you want it in production)
    //if (app.Environment.IsDevelopment())
    //{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Automated Task System API V1");
    });
    //}

    // 👇 Authentication/Authorization should come before endpoints
    app.UseAuthentication();
    app.UseAuthorization();

    // 👇 Apply migrations, repair schema drift, then seed
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        await dbContext.Database.MigrateAsync();
        await SubjectSchemaRepair.ApplyAsync(dbContext);
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.Seed();
    }

    // 👇 Endpoint routing
    app.MapControllers();
    app.MapHub<UserHub>("/userhub");

    app.Run();
}
catch (Exception ex)
{
    var errorLog = Path.Combine(Directory.GetCurrentDirectory(), "ErrorLogs");
    Directory.CreateDirectory(errorLog);

    var logPath = Path.Combine(errorLog, $"startup_error_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}.txt");

    var content = new StringBuilder();
    content.AppendLine($"Startup Error - {DateTime.UtcNow:O}");
    content.AppendLine(ex.ToString());

    File.WriteAllText(logPath, content.ToString());

    throw; // Let the app crash as normal after logging
}
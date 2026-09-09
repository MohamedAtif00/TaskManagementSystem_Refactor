using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Configuration;

public static class AuthenticationExtensions
{
    public const string CorsPolicyName = "FrontendCors";
    public const string ApidogCorsPolicyName = "ApidogCors";

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var secret = configuration.GetSection("AppSetting:Token").Value
            ?? throw new InvalidOperationException("AppSetting:Token is not configured.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    NameClaimType = ClaimTypes.NameIdentifier
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/realtime"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(nameof(UserRole.Owner), policy => policy.RequireRole(nameof(UserRole.Owner)));
            options.AddPolicy(nameof(UserRole.ProjectManger), policy =>
                policy.RequireRole(nameof(UserRole.ProjectManger), nameof(UserRole.Owner)));
            options.AddPolicy(nameof(UserRole.SectionHead), policy =>
                policy.RequireRole(
                    nameof(UserRole.SectionHead),
                    nameof(UserRole.ProjectManger),
                    nameof(UserRole.Owner)));
            options.AddPolicy(nameof(UserRole.TeamLeader), policy =>
                policy.RequireRole(
                    nameof(UserRole.TeamLeader),
                    nameof(UserRole.SectionHead),
                    nameof(UserRole.ProjectManger),
                    nameof(UserRole.Owner)));
            options.AddPolicy(nameof(UserRole.Member), policy =>
                policy.RequireRole(
                    nameof(UserRole.Member),
                    nameof(UserRole.TeamLeader),
                    nameof(UserRole.SectionHead),
                    nameof(UserRole.ProjectManger),
                    nameof(UserRole.Owner)));
        });
        return services;
    }

    public static IServiceCollection AddFrontendCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins =
            configuration.GetSection("Cors:AllowedOrigins")
                .Get<string[]>()
            ??
            [
                "http://localhost:3000",
                "http://localhost:4200",
                "https://localhost:4200",
                "http://stdigital.stp.local",
                "http://localhost:8081",
                "https://localhost",
                "https://localhost:443",
                "https://localhost:61172",
                "http://localhost:61173",
                "https://ats.stp.local"
            ];

        services.AddCors(options =>
        {
            options.AddPolicy(
                CorsPolicyName,
                policy => policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins(allowedOrigins)
                    .AllowCredentials());
        });

        return services;
    }

    public static IServiceCollection AddApidogCors(
        this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(
                ApidogCorsPolicyName,
                policy => policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin());
        });

        return services;
    }
}

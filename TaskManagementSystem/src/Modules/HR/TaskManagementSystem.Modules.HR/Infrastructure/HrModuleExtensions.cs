using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskManagementSystem.BuildingBlocks.Application.Inbox;
using TaskManagementSystem.BuildingBlocks.Persistence.Events;
using TaskManagementSystem.BuildingBlocks.Persistence.Inbox;
using TaskManagementSystem.BuildingBlocks.Persistence.Data;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;
using TaskManagementSystem.Modules.HR.Infrastructure.Storage;

namespace TaskManagementSystem.Modules.HR.Infrastructure;

public static class HrModuleExtensions
{
    public const string TestingDatabaseName = "HrTests";

    public static IServiceCollection AddHrModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.Configure<LeaveSettingsOptions>(configuration.GetSection(LeaveSettingsOptions.SectionName));
        services.AddSqlConnectionFactory(configuration);
        services.AddScoped<IInboxGuard, EfInboxGuard<HrDbContext>>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(HrTransactionBehavior<,>));
        services.AddScoped<LeaveRequestSearchQueries>();
        services.AddScoped<PermissionRequestSearchQueries>();
        services.AddScoped<WorkFromHomeRequestSearchQueries>();
        services.AddScoped<ForgotClockRequestSearchQueries>();
        services.AddScoped<OrgLookupQueries>();

        services.AddDbContext<HrDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
            }

            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IHrUnitOfWork, HrUnitOfWork>();
        services.AddScoped<IHolidayRepository, HolidayRepository>();
        services.AddScoped<IWorkingDayCalculator, WorkingDayCalculatorService>();
        services.AddScoped<IMedicalCertificateStorage, LocalMedicalCertificateStorage>();
        services.AddScoped<LeaveRequestPlanner>();
        services.AddScoped<LeaveOpinionProcessor>();
        services.AddScoped<PermissionOpinionProcessor>();
        services.AddScoped<WorkFromHomeOpinionProcessor>();
        services.AddScoped<ForgotClockOpinionProcessor>();

        return services;
    }
}

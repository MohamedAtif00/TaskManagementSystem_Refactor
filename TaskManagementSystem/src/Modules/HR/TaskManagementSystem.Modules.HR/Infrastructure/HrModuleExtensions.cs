using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence;
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

        services.AddDbContext<HrDbContext>(options =>
        {
            if (environment.IsEnvironment("Testing"))
            {
                options.UseInMemoryDatabase(TestingDatabaseName);
            }
            else
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

                options.UseSqlServer(connectionString);
            }
        });

        services.AddScoped<IHrUnitOfWork, HrUnitOfWork>();
        services.AddScoped<IHolidayRepository, HolidayRepository>();
        services.AddScoped<IWorkingDayCalculator, WorkingDayCalculatorService>();
        services.AddScoped<IMedicalCertificateStorage, LocalMedicalCertificateStorage>();
        services.AddScoped<LeaveRequestPlanner>();
        services.AddScoped<LeaveOpinionProcessor>();

        return services;
    }
}

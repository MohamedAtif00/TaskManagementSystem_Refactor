using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskManagementSystem.BuildingBlocks.Application.Inbox;
using TaskManagementSystem.BuildingBlocks.Application.Outbox;
using TaskManagementSystem.BuildingBlocks.Persistence.Events;
using TaskManagementSystem.BuildingBlocks.Persistence.Inbox;
using TaskManagementSystem.BuildingBlocks.Persistence.Outbox;
using TaskManagementSystem.BuildingBlocks.Persistence.Data;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence;
using TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure;

public static class TicketModuleExtensions
{
    public static IServiceCollection AddTicketModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSqlConnectionFactory(configuration);
        services.AddScoped<ILearningObjectiveLookup, LearningObjectiveLookupQueries>();
        services.AddScoped<ITaskBankLookup, TaskBankLookupQueries>();
        services.AddScoped<IWorkflowStepLookup, WorkflowStepLookupQueries>();
        services.AddScoped<IIdentityUserLookup, IdentityUserLookupQueries>();
        services.AddScoped<IOrganizationTeamLookup, OrganizationTeamLookupQueries>();
        services.AddScoped<SubjectTicketsQueries>();
        services.AddScoped<SprintTicketsQueries>();
        services.AddScoped<TicketStatsQueries>();
        services.AddScoped<IOutboxWriter, EfOutboxWriter<TicketDbContext>>();
        services.AddScoped<IInboxGuard, EfInboxGuard<TicketDbContext>>();
        services.AddOutboxProcessor("ticket", environment);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TicketTransactionBehavior<,>));

        services.AddDbContext<TicketDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
            }

            options.UseSqlServer(connectionString);
        });

        services.AddScoped<ITicketUnitOfWork, TicketUnitOfWork>();

        return services;
    }
}

using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace TaskManagementSystem.BuildingBlocks.Infrastructure.Observability;

/// <summary>
/// BCL ActivitySource and Meter names. Modules use these; the API OpenTelemetry SDK listens and exports.
/// </summary>
public static class Telemetry
{
    public const string ServiceName = "TaskManagementSystem";

    public const string ActivitySourcePrefix = "TaskManagementSystem";

    public static readonly ModuleTelemetry Api = new("TaskManagementSystem.Api");
    public static readonly ModuleTelemetry Mediator = new("TaskManagementSystem.Mediator");
    public static readonly ModuleTelemetry Identity = new("TaskManagementSystem.Identity");
    public static readonly ModuleTelemetry Organization = new("TaskManagementSystem.Organization");
    public static readonly ModuleTelemetry Workflows = new("TaskManagementSystem.Workflows");
    public static readonly ModuleTelemetry Curriculum = new("TaskManagementSystem.Curriculum");
    public static readonly ModuleTelemetry Ticket = new("TaskManagementSystem.Ticket");
    public static readonly ModuleTelemetry Sprints = new("TaskManagementSystem.Sprints");
    public static readonly ModuleTelemetry Notifications = new("TaskManagementSystem.Notifications");
    public static readonly ModuleTelemetry HR = new("TaskManagementSystem.HR");

    public static IReadOnlyList<ModuleTelemetry> All { get; } =
    [
        Api,
        Mediator,
        Identity,
        Organization,
        Workflows,
        Curriculum,
        Ticket,
        Sprints,
        Notifications,
        HR
    ];

    public static IEnumerable<string> ActivitySourceNames => All.Select(module => module.ActivitySource.Name);

    public static IEnumerable<string> MeterNames => All.Select(module => module.Meter.Name);
}

public sealed class ModuleTelemetry
{
    public ModuleTelemetry(string name)
    {
        ActivitySource = new ActivitySource(name);
        Meter = new Meter(name);
    }

    public ActivitySource ActivitySource { get; }

    public Meter Meter { get; }
}

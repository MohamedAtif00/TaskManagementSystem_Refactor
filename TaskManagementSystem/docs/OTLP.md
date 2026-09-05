# OTLP and OpenTelemetry

How this solution emits and exports telemetry. Architecture overview: [ARCHITECTURE.md](ARCHITECTURE.md).

**Last updated:** 2026-09-05

## Signals

The API host pipeline covers all three OpenTelemetry signals:

| Signal | Source |
|---|---|
| Traces | ASP.NET Core, HttpClient, module `ActivitySource`s, **MediatR `TracingBehavior`** |
| Metrics | ASP.NET Core, HttpClient, runtime, module `Meter`s, **MediatR counters/histograms** |
| Logs | `ILogger` via `Logging.AddOpenTelemetry` (formatted message + scopes) |

## Where it lives

| Piece | Path |
|---|---|
| SDK wiring | `src/Api/TaskManagementSystem.Api/Configuration/OpenTelemetryExtensions.cs` |
| Options | `src/Api/TaskManagementSystem.Api/Configuration/OpenTelemetryOptions.cs` |
| Config | `src/Api/TaskManagementSystem.Api/appsettings.json` (`OpenTelemetry` section) |
| Named sources | `src/BuildingBlocks/.../Observability/Telemetry.cs` |
| MediatR tracing | `src/BuildingBlocks/.../Behaviors/TracingBehavior.cs` |

The API is the only project that references OpenTelemetry NuGet packages. BuildingBlocks and modules use BCL `ActivitySource`, `Meter`, and `ILogger` only. Architecture tests fail if a module or BuildingBlocks depends on `OpenTelemetry`.

## Enable OTLP export

Export is **off** when the endpoint is empty. Nothing leaves the process until you set an absolute URI.

In `appsettings.json` (or environment-specific files):

```json
{
  "OpenTelemetry": {
    "ServiceName": "TaskManagementSystem",
    "OtlpEndpoint": "http://localhost:4317"
  }
}
```

Or set the standard environment variable (used if config is empty):

```text
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
```

`http://localhost:4317` is the usual gRPC OTLP port for Aspire dashboard, Jaeger, Grafana Alloy, or the OpenTelemetry Collector.

Leave `OtlpEndpoint` as `""` for local runs with no collector.

## MediatR observability (automatic)

Every `ICommand` / `IQuery` dispatched through MediatR is traced by `TracingBehavior` without per-handler code:

| Signal | Detail |
|---|---|
| ActivitySource | `TaskManagementSystem.Mediator` |
| Span name | Request type name (e.g. `AuthenticateCommand`) |
| Tags | `mediatr.request_name`, `mediatr.kind` (`command`/`query`), `mediatr.module` |
| Counter | `tms.mediatr.requests` (tags: kind, module, success) |
| Histogram | `tms.mediatr.duration_ms` |

`LoggingBehavior` adds scope fields: `TraceId`, `RequestKind`, `ModuleName`, `RequestName`.

## Command audit log (not OTLP)

Successful and failed **commands** are also persisted to SQL (`app.AuditLog`) by `AuditBehavior`. See [ARCHITECTURE.md](ARCHITECTURE.md) audit section. Queries are traced but not audited.

## Emit from a module

Do not add OpenTelemetry packages to a module. Start an activity on the module source:

```csharp
using var activity = Telemetry.Ticket.ActivitySource.StartActivity("Ticket.Move");
activity?.SetTag("ticket.id", ticketId);

_logger.LogInformation("Ticket {TicketId} moved", ticketId);
```

Sources and meters (all prefixed `TaskManagementSystem.`):

- Api, **Mediator**, Identity, Organization, Workflows, Curriculum, Ticket, Sprints, Notifications, HR

The host registers `TaskManagementSystem.*` plus each name explicitly.

## Instrumentation already on

- ASP.NET Core requests
- HttpClient outgoing calls
- Runtime metrics (GC, thread pool, and related)
- MediatR commands and queries (via `TracingBehavior`)

EF Core instrumentation is not added yet.

## Architecture tests

`tests/TaskManagementSystem.ArchitectureTests/ObservabilityBoundaryTests.cs`

- BuildingBlocks must not depend on `OpenTelemetry`
- Domain modules must not depend on `OpenTelemetry`
- API must reference OpenTelemetry

## How to update this doc

When you add a collector, a new `ActivitySource`, or extra instrumentation (for example EF Core), update this file and set **Last updated**. Keep module-boundary rules in [ARCHITECTURE.md](ARCHITECTURE.md).

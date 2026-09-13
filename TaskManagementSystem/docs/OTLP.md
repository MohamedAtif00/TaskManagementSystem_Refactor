# OTLP and OpenTelemetry

How this solution emits and exports telemetry. Architecture overview: [ARCHITECTURE.md](ARCHITECTURE.md).

**Last updated:** 2026-09-12

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

Development export is enabled via [`appsettings.Development.json`](../src/Api/TaskManagementSystem.Api/appsettings.Development.json) (`OtlpEndpoint: http://localhost:4317`).

## Local Aspire dashboard (recommended)

Start the dashboard collector:

```bash
docker compose -f docker-compose.observability.yml up -d
```

| Endpoint | URL |
|---|---|
| Dashboard UI | http://localhost:18888 |
| OTLP gRPC ingest | http://localhost:4317 |

Then run the API in Development:

```bash
dotnet run --project src/Api/TaskManagementSystem.Api
```

What to verify:

- **Traces** — ASP.NET Core requests and MediatR command/query spans
- **Metrics** — `tms.mediatr.requests`, `tms.mediatr.duration_ms`, runtime metrics
- **Logs** — structured `ILogger` output exported via OpenTelemetry

Stop the dashboard:

```bash
docker compose -f docker-compose.observability.yml down
```

## HTTP boundary logging

[`ApiExceptionHandler.cs`](../src/Api/TaskManagementSystem.Api/Infrastructure/ApiExceptionHandler.cs) logs at the API edge:

| Case | Level | When |
|---|---|---|
| FluentValidation / invalid input | Warning | `ValidationException` → 400 `validation_failed` |
| Unexpected failure | Error | Unhandled exception → 500 `unexpected_error` |

Validation logging stays at the HTTP boundary only (not in `ValidationBehavior`) to avoid duplicate noise. MediatR handler failures are still logged by `LoggingBehavior` with the same `TraceId` scope.

Example invalid login body → check dashboard **Logs** for `Validation failed for POST /auth/login` with `ValidationCode=validation_failed`.

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

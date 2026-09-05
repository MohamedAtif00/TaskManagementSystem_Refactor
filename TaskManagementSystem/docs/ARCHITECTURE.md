# TaskManagementSystem architecture



Living snapshot of the new solution. Update this file as the refactor grows.



**Last updated:** 2026-09-05



This is a **modular monolith** rewrite of the existing Automated Task System (ATS). Target framework is **.NET 10**. The old app under `../AutomatedTaskSystem` stays as the source of behavior until features are ported.



Solution file: [`TaskManagementSystem.slnx`](../TaskManagementSystem.slnx)



Related: [OTLP / OpenTelemetry](OTLP.md), [Database / DbUp](DATABASE.md)



## What is built today



The solution has **real cross-cutting infrastructure** and the **Identity auth slice** (code-only login). Other product features (tickets, leave, etc.) are not ported yet.



| Layer | Status |

|---|---|

| Solution + 8 module projects | Done — folder structure only |

| BuildingBlocks (DDD + CQRS) | Done — domain primitives, MediatR pipeline |

| API host | Done — observability, realtime hub, JWT auth, Minimal API `/auth/*` endpoints |

| Identity module (auth) | Done — login, refresh, logout, about-me (legacy-compatible) |

| Other module business logic | Not started |

| Persistence (DbUp + SQL scripts) | Done — per-module schemas, DatabaseMigrator, initial DDL |

| EF Core / repositories | Done — Identity `IdentityDbContext` + repositories |

| Frontend | Not started |

| Automated tests | Done — 45 tests across unit, integration, and architecture |



Suggested next slice: **Organization** (Teams/Sections CRUD) or **Ticket** execution.



## Authentication (Identity)



Legacy-compatible auth preserved for the existing frontend:



| Endpoint | Purpose |

|---|---|

| `POST /auth/login` | `{ code }` → JWT in body + `refreshToken` HttpOnly cookie |

| `POST /auth/refresh-token` | Rotate refresh token + new JWT |

| `POST /auth/logout` | Invalidate refresh token |

| `POST /auth/about-me` | Current user profile (`.RequireAuthorization()`) |



- **Code-only login** — 6-character unique `Users.Code`, no password

- **JWT claims** — `"Id"`, `ClaimTypes.Role`, `ClaimTypes.NameIdentifier` (same as old ATS)

- **Response envelope** — `{ data, error, message }`

- **`about-me.group`** — team name from `organization.Teams` (property kept for frontend compat; Groups removed)

- **Config** — `AppSetting:Token` signing key in `appsettings.json`

- **Auth at API boundary** — `.RequireAuthorization()` on minimal API routes; `ICurrentUserAccessor` reads JWT claims and passes explicit `UserId` into MediatR. Module handlers never use `IHttpContextAccessor`.

- **HTTP style** — auth routes are **Minimal API** in [`Endpoints/AuthEndpoints.cs`](../src/Api/TaskManagementSystem.Api/Endpoints/AuthEndpoints.cs). Future modules: `Endpoints/{Module}Endpoints.cs` + `Map{Module}Endpoints()`.

- **Role policies** — registered in `AuthenticationExtensions` for `.RequireAuthorization("TeamLeader")` on future endpoints

- **Endpoints are happy-path only** — auth handlers extract HTTP inputs, dispatch MediatR, and map success to `Results.Ok(...)`. No per-route try/catch.

- **Legacy error mapping** — [`Infrastructure/LegacyExceptionHandler.cs`](../src/Api/TaskManagementSystem.Api/Infrastructure/LegacyExceptionHandler.cs) implements `IExceptionHandler` and maps Identity application exceptions to legacy `{ error, message }` JSON with the correct status codes (`InvalidLoginCodeException` → 404, `InvalidRefreshTokenException` / `UserNotFoundException` → 400). Registered via `AddExceptionHandler<LegacyExceptionHandler>()` and `UseExceptionHandler()` in `Program.cs`. Unhandled exceptions fall through to the default 500 response. Future modules should throw application exceptions; extend the handler (or a shared base type) as new legacy routes are added.



### Protected endpoint pattern

```csharp
group.MapPost("/about-me", AboutMeAsync).RequireAuthorization();

private static async Task<IResult> AboutMeAsync(
    IMediator mediator,
    ICurrentUserAccessor currentUser,
    CancellationToken ct)
{
    var userId = currentUser.GetRequiredUserId();
    var result = await mediator.Send(new AboutMeQuery(userId), ct);
    return Results.Ok(...);
}
```




## Layout



```

src/

  Api/TaskManagementSystem.Api              Thin host (SignalR + OpenTelemetry SDK + Minimal API endpoints)
    Endpoints/                              Route groups (e.g. AuthEndpoints)

  Database/
    DatabaseMigrator/                         DbUp console runner
    TaskManagementSystem.Database/            SQL scripts + Structure per schema

  BuildingBlocks/TaskManagementSystem.BuildingBlocks

    Domain/                                   Entity, ValueObject, rules, domain events

    Application/                              ICommand/IQuery, MediatR behaviors, AddBuildingBlocks

    Infrastructure/Realtime/                  IRealtimePublisher port

    Infrastructure/Observability/             Telemetry names (BCL only)

  Modules/<Name>/TaskManagementSystem.Modules.<Name>

    Domain/                                   (empty — placeholders)

    Features/                                 (empty — vertical slice folders)

    Infrastructure/                           (empty)

    Contracts/                                Realtime factories where defined

tests/

  TaskManagementSystem.TestCommon             Shared builders, MediatR samples, WebApplicationFactory

  TaskManagementSystem.BuildingBlocks.UnitTests

  TaskManagementSystem.Modules.UnitTests

  TaskManagementSystem.Api.IntegrationTests

  TaskManagementSystem.ArchitectureTests

docs/

  ARCHITECTURE.md                             This file

  OTLP.md                                     OpenTelemetry export details

```



The API references every module. Each module references BuildingBlocks only. **Modules do not reference each other.**



```

API

 ├── Identity, Organization, Workflows

 ├── Curriculum, Ticket, Sprints

 ├── Notifications, HR

 └── BuildingBlocks (via modules and a direct reference)



Each module --> BuildingBlocks

```



## API host



[`Program.cs`](../src/Api/TaskManagementSystem.Api/Program.cs) wires three concerns:



```csharp

builder.AddObservability();                  // traces, metrics, logs (OTLP optional)

builder.Services.AddBuildingBlocks(

    typeof(Program).Assembly,

    typeof(Entity).Assembly);                // MediatR + pipeline behaviors

builder.Services.AddRealtime();              // SignalR hub + IRealtimePublisher adapter



app.MapAuthEndpoints();                      // POST /auth/login, /auth/about-me, etc.
app.MapRealtimeHub();                        // single WebSocket at /realtime

```



`public partial class Program;` is exposed for `WebApplicationFactory` integration tests.



Auth is exposed via **Minimal API** at `/auth/*` ([`AuthEndpoints.cs`](../src/Api/TaskManagementSystem.Api/Endpoints/AuthEndpoints.cs)). Other product HTTP endpoints are not ported yet.



## BuildingBlocks



Shared kernel used by every module. Third-party packages allowed here: **MediatR**, **FluentValidation**. No SignalR or OpenTelemetry packages.



### Domain (`Domain/`)



| Type | Role |

|---|---|

| `Entity` | Identity equality, `DomainEvents` list, `CheckRule(IBusinessRule)` |

| `ValueObject` | Component-based equality via `GetEqualityComponents()` |

| `IAggregateRoot` | Marker for aggregate roots |

| `IBusinessRule` | `IsBroken()`, `Message` |

| `BusinessRuleValidationException` | Thrown when `Entity.CheckRule` finds a broken rule |

| `IDomainEvent` / `DomainEventBase` | `Id`, `OccurredOn` on every domain event |



Broken business rules **throw** (Grzybek-style), they do not return result objects.



### Application (`Application/`)



| Type | Role |

|---|---|

| `ICommand` / `ICommand<TResult>` | MediatR `IRequest` markers for writes |

| `IQuery<TResult>` | MediatR `IRequest<TResult>` marker for reads |

| `IIntegrationEvent` | Contract for future module-to-module events (type only) |

| `IUnitOfWork` | `CommitAsync` port; EF modules implement via `EfUnitOfWork<TContext>` in BuildingBlocks.Persistence |

| `LoggingBehavior` | Logs request name + success/failure via `ILogger` (includes trace/kind/module scope) |
| `TracingBehavior` | OpenTelemetry spans + metrics for every command/query via `Telemetry.Mediator` |
| `AuditBehavior` | Persists command audit rows (`app.AuditLog`) — commands only, no payload |
| `ValidationBehavior` | Runs FluentValidation validators; throws `ValidationException` |

| `AddBuildingBlocks(assemblies…)` | Registers MediatR, both pipeline behaviors, and validators |



Commands and queries are dispatched through MediatR. Module handlers will implement `IRequestHandler<>` in `Features/` slices when product logic is ported.

### Persistence (EF Core)

| Type | Location | Role |
|---|---|---|
| `GenericRepository<TEntity, TContext>` | `BuildingBlocks.Persistence` | Shared read/write EF helpers (`FindReadOnlyAsync`, `FindTrackedAsync`, `AddEntityAsync`, `ExistsReadOnlyAsync`) |
| `EfUnitOfWork<TContext>` | `BuildingBlocks.Persistence` | `IUnitOfWork` over `SaveChangesAsync`; exposes protected `DbContext` |
| `LazyRepositoryFactory` | `BuildingBlocks.Persistence` | `Lazy<T>` with `ExecutionAndPublication` for repo properties on module UoW |
| Module UoW | `Modules.{Name}.Application` | e.g. `IIdentityUnitOfWork` — **sole handler entry point** for repositories |
| Narrow repository ports | `Modules.{Name}.Application` | Use-case methods only (`IUserRepository`, `IAboutMeRepository`) — not registered individually in DI |
| Repository implementations | `Modules.{Name}.Infrastructure/Persistence` | Extend `GenericRepository`; wired lazily inside module UoW |

Handlers inject **`IIdentityUnitOfWork`** (or future `IOrganizationUnitOfWork`) — not individual repositories or `DbContext`. Commands call `unitOfWork.{Repo}` then `unitOfWork.CommitAsync()`. Queries call read repositories on the same UoW without commit.

**Identity example:**

```csharp
public interface IIdentityUnitOfWork : IUnitOfWork
{
    IUserRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IAboutMeRepository AboutMe { get; }
}
```

Cross-table reads use read repositories on the UoW (e.g. `IAboutMeRepository`) — not separate read services.

**Future module template (e.g. Organization Teams/Sections):**

```
Application/IOrganizationUnitOfWork.cs     // extends IUnitOfWork, exposes ITeamRepository Teams { get; }
Application/ITeamRepository.cs             // use-case methods
Infrastructure/OrganizationUnitOfWork.cs   // Lazy repo properties, single DbContext
Infrastructure/TeamRepository.cs           // extends GenericRepository<Team, OrganizationDbContext>
```

No generic `Update`/`Delete` on repositories — load tracked entities, mutate via domain methods, then `CommitAsync`.

### Command audit log

| Piece | Location | Notes |
|---|---|---|
| `IAuditContext` | `BuildingBlocks.Application` | `UserId` + `CorrelationId` ports |
| `AuditContext` | `Api/Infrastructure` | Reads JWT `"Id"` claim + `Activity.Current.TraceId` |
| `IAuditStore` / `AuditEntry` | `BuildingBlocks.Application` | Append-only audit port |
| `EfAuditStore` / `AuditDbContext` | `BuildingBlocks.Persistence/Audit` | Maps to `app.AuditLog` |
| `AuditBehavior` | MediatR pipeline | Commands only; stores action name, not payload |

Migration: `012_app_AuditLog.sql`. Queries are traced/logged but not audited.



### Infrastructure ports



| Port | Location | Implementation |

|---|---|---|

| `IRealtimePublisher` | `Infrastructure/Realtime/` | `SignalRRealtimePublisher` in API |

| `Telemetry` (ActivitySource/Meter names) | `Infrastructure/Observability/` | BCL only; SDK wired in API |



## Module folders



Every module uses the same shape:



| Folder | Role |

|---|---|

| `Domain/` | Aggregates, value objects, business rules |

| `Features/` | Vertical slices (one folder per use case: command, query, handler, validator) |

| `Infrastructure/` | Persistence, module composition root |

| `Contracts/` | Public facade, integration events, realtime message factories |



### Modules and feature slices



| Module | Planned slices | Code today |

|---|---|---|

| Identity | Authenticate, RefreshToken, AboutMe, Logout | Done — MediatR slices + EF Core + JWT |

| Organization | **Teams**, **Sections** (no Groups) | Project shell only |

| Workflows | Schemas, Nodes, Steps, TaskBank | Project shell only |

| Curriculum | Projects, Subjects, Units, Lessons, LearningObjectives | Project shell only |

| Ticket | Tickets, Comments, Rollback, WorkTime | `Contracts/TicketRealtime` (silent sync) |

| Sprints | Sprints, SprintLearningObjectives, Analytics | Project shell only |

| Notifications | Notifications (visible alerts only) | `Contracts/NotificationRealtime` (visible alerts) |

| HR | Leave, Permissions, WorkFromHome | Project shell only |



Organization is **team-only** at the org-unit level: use Teams and Sections. Do not reintroduce Groups as an org concept. SignalR `JoinGroup` is a **connection group**, not an org Group.



The task execution module is named **Ticket** (not Task) to avoid clashing with `System.Threading.Tasks.Task`.



## Rules



- Domain modules must not reference **SignalR** or **OpenTelemetry** packages.

- The API is the only place that hosts SignalR (`RealtimeHub`) and the OpenTelemetry SDK.

- Notifications must not depend on Ticket types. Silent ticket payloads stay in Ticket contracts.

- Architecture tests in `tests/TaskManagementSystem.ArchitectureTests` enforce these boundaries.



## Realtime



One WebSocket: **`/realtime`**.



| Kind | Meaning | Example |

|---|---|---|

| Silent | Patch UI with no toast/inbox | `ticket.updated` via `TicketRealtime.SilentUpdate()` |

| Notification | User should notice (toast, badge, inbox) | `notification.created` via `NotificationRealtime.Visible()` |



**Port:** `IRealtimePublisher` in BuildingBlocks (`PublishToUserAsync` / `PublishToGroupAsync`).



**Adapter:** `SignalRRealtimePublisher` in the API injects `IHubContext<RealtimeHub>`.



**Hub:** `RealtimeHub` exposes `JoinGroup` / `LeaveGroup` for connection groups.



Domain code injects `IRealtimePublisher`, never `IHubContext`. Silent content sync does **not** go through the Notifications module.



## Observability



Traces, metrics, and logs are configured on the host via `AddObservability()`. Module code uses BCL `ActivitySource` / `Meter` / `ILogger` from `Telemetry` in BuildingBlocks. Export is OTLP when an endpoint is set. See [OTLP.md](OTLP.md).



## Testing



34 automated tests (all passing). Shared packages and versions live in [`tests/Directory.Build.props`](../tests/Directory.Build.props).



| Project | Type | Tests | Scope |

|---|---|---|---|

| `BuildingBlocks.UnitTests` | Unit | 19 | Entity rules/events, ValueObject equality, pipeline behaviors, `AddBuildingBlocks` DI |

| `Modules.UnitTests` | Unit | 2 | `TicketRealtime`, `NotificationRealtime` contract factories |

| `Api.IntegrationTests` | Integration | 6 | Host startup, MediatR pipeline, SignalR hub join/leave, group publish delivery |

| `ArchitectureTests` | Architecture | 7 | SignalR/OpenTelemetry boundaries, Notifications↛Ticket, hub location |

| `TestCommon` | Shared | — | Bogus builders, sample MediatR commands, `TmsWebApplicationFactory` |



**Stack:** xUnit, FluentAssertions, NSubstitute, Bogus, `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.AspNetCore.SignalR.Client`.



**Naming:** `Method_Scenario_ExpectedResult`.



**TestCommon builders:** `TicketPayloadBuilder`, `NotificationPayloadBuilder`, `RealtimeMessageBuilder`.



**TestCommon MediatR samples:** `PingCommand`, `ValidatedCommand` (+ validator), `FailingCommand` — used by integration tests to prove the pipeline.



**Run all tests:**



```powershell

dotnet test TaskManagementSystem.slnx

```



### Future module test pattern



When a module gains domain logic and handlers:



1. Add `TaskManagementSystem.Modules.{Name}.UnitTests` — domain rules + handlers with mocked `IUnitOfWork`/repos.

2. Optionally add `{Name}.IntegrationTests` — EF InMemory or Testcontainers, module composition root.

3. Reuse `TestCommon` builders; extend `TmsWebApplicationFactory` to register the module.



## Status summary



### Implemented



- Solution structure: API, BuildingBlocks, 8 modules

- DDD domain primitives (`Entity`, `ValueObject`, business rules, domain events)

- CQRS markers and MediatR pipeline (logging + FluentValidation)

- `AddBuildingBlocks()` host registration

- Realtime port/adapter (`IRealtimePublisher` → SignalR hub at `/realtime`)

- Ticket and Notifications realtime contract factories

- OpenTelemetry host pipeline (OTLP export optional)

- Architecture boundary tests

- Full testing foundation (unit + integration + shared TestCommon)

- DbUp database layer: per-module SQL schemas, `DatabaseMigrator`, initial DDL (Groups removed; TeamId on TaskBank/Tasks)

- Identity auth: code-only login, JWT + refresh cookies, legacy `/auth/*` contract



### Not built yet



- Module aggregates and handlers outside Identity

- HTTP endpoints outside auth

- EF Core for modules other than Identity

- Module facades (`ITicketModule.ExecuteCommandAsync`, etc.)

- Integration event bus, outbox/inbox

- Publishing silent/visible events from real domain handlers

- Port of ATS product behavior (~25 controllers, workflow engine)

- Frontend



## How to update this doc



When you add a module, slice, or cross-cutting rule:



1. Update the module table, **What is built today**, or **Status summary**.

2. Set **Last updated**.

3. If the change is observability/OTLP, edit [OTLP.md](OTLP.md) instead of duplicating it here.



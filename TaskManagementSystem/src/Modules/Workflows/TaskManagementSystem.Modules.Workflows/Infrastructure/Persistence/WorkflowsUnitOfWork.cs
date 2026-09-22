using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.BuildingBlocks.Persistence.Events;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Infrastructure.Persistence;

internal sealed class WorkflowsUnitOfWork(
    WorkflowsDbContext context,
    IDomainEventDispatcher domainEventDispatcher)
    : UnitOfWork<WorkflowsDbContext>(context, domainEventDispatcher), IWorkflowsUnitOfWork
{
    private readonly Lazy<SchemaRepository> _schemas =
        LazyRepositoryFactory.Create(() => new SchemaRepository(context));

    private readonly Lazy<SchemaTypeRepository> _schemaTypes =
        LazyRepositoryFactory.Create(() => new SchemaTypeRepository(context));

    private readonly Lazy<TicketBankRepository> _taskBank =
        LazyRepositoryFactory.Create(() => new TicketBankRepository(context));

    private readonly Lazy<NodeRepository> _nodes =
        LazyRepositoryFactory.Create(() => new NodeRepository(context));

    private readonly Lazy<StepRepository> _steps =
        LazyRepositoryFactory.Create(() => new StepRepository(context));

    public ISchemaRepository Schemas => _schemas.Value;

    public ISchemaTypeRepository SchemaTypes => _schemaTypes.Value;

    public ITicketBankRepository TicketBank => _taskBank.Value;

    public INodeRepository Nodes => _nodes.Value;

    public IStepRepository Steps => _steps.Value;
}

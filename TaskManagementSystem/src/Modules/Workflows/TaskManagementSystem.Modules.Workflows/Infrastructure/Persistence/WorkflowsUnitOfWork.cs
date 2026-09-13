using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Infrastructure.Persistence;

internal sealed class WorkflowsUnitOfWork(WorkflowsDbContext context)
    : UnitOfWork<WorkflowsDbContext>(context), IWorkflowsUnitOfWork
{
    private readonly Lazy<SchemaRepository> _schemas =
        LazyRepositoryFactory.Create(() => new SchemaRepository(context));

    private readonly Lazy<SchemaTypeRepository> _schemaTypes =
        LazyRepositoryFactory.Create(() => new SchemaTypeRepository(context));

    private readonly Lazy<TaskBankRepository> _taskBank =
        LazyRepositoryFactory.Create(() => new TaskBankRepository(context));

    private readonly Lazy<NodeRepository> _nodes =
        LazyRepositoryFactory.Create(() => new NodeRepository(context));

    private readonly Lazy<StepRepository> _steps =
        LazyRepositoryFactory.Create(() => new StepRepository(context));

    public ISchemaRepository Schemas => _schemas.Value;

    public ISchemaTypeRepository SchemaTypes => _schemaTypes.Value;

    public ITaskBankRepository TaskBank => _taskBank.Value;

    public INodeRepository Nodes => _nodes.Value;

    public IStepRepository Steps => _steps.Value;
}

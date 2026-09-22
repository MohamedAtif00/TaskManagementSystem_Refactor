using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.Modules.Workflows.Application;

public interface IWorkflowsUnitOfWork : IUnitOfWork
{
    ISchemaRepository Schemas { get; }

    ISchemaTypeRepository SchemaTypes { get; }

    ITicketBankRepository TicketBank { get; }

    INodeRepository Nodes { get; }

    IStepRepository Steps { get; }
}

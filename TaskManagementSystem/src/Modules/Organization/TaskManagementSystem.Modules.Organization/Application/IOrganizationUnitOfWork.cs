using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.Modules.Organization.Application;

public interface IOrganizationUnitOfWork : IUnitOfWork
{
    ITeamRepository Teams { get; }

    ISectionRepository Sections { get; }
}

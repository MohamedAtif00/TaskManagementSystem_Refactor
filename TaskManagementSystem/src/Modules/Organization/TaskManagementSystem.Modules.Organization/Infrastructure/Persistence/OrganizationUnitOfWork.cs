using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.BuildingBlocks.Persistence.Events;
using TaskManagementSystem.Modules.Organization.Application;

namespace TaskManagementSystem.Modules.Organization.Infrastructure.Persistence;

internal sealed class OrganizationUnitOfWork(
    OrganizationDbContext context,
    IDomainEventDispatcher domainEventDispatcher)
    : UnitOfWork<OrganizationDbContext>(context, domainEventDispatcher), IOrganizationUnitOfWork
{
    private readonly Lazy<TeamRepository> _teams =
        LazyRepositoryFactory.Create(() => new TeamRepository(context));

    private readonly Lazy<SectionRepository> _sections =
        LazyRepositoryFactory.Create(() => new SectionRepository(context));

    public ITeamRepository Teams => _teams.Value;

    public ISectionRepository Sections => _sections.Value;
}

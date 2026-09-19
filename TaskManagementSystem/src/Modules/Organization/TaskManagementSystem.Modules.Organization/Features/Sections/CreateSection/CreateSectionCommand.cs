using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Organization.Application;
using TaskManagementSystem.Modules.Organization.Domain;
using TaskManagementSystem.Modules.Organization.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Organization.Features.Sections.CreateSection;

public sealed record CreateSectionCommand(
    string Name,
    int HeadId,
    IReadOnlyList<int> TeamIds) : ICommand<Result<SectionDetailResult>>;


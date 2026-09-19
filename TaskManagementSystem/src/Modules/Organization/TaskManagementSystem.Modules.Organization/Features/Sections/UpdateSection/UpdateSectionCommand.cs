using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Organization.Application;
using TaskManagementSystem.Modules.Organization.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Organization.Features.Sections.UpdateSection;

public sealed record UpdateSectionCommand(
    int SectionId,
    string Name,
    int HeadId,
    IReadOnlyList<int> TeamIds) : ICommand<Result<SectionDetailResult>>;


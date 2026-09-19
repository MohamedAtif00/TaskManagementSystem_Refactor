using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Organization.Application;

namespace TaskManagementSystem.Modules.Organization.Features.Sections.ArchiveSection;

public sealed record ArchiveSectionCommand(int SectionId) : ICommand<Result<NoValue>>;


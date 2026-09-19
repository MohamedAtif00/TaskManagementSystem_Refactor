using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Organization.Application;

namespace TaskManagementSystem.Modules.Organization.Features.Sections.ListSections;

public sealed record ListSectionsQuery : IQuery<Result<IReadOnlyList<SectionListItemResult>>>;


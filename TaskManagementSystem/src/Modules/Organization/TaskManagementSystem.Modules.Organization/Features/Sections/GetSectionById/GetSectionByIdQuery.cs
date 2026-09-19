using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Organization.Application;
using TaskManagementSystem.Modules.Organization.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Organization.Features.Sections.GetSectionById;

public sealed record GetSectionByIdQuery(int SectionId) : IQuery<Result<SectionDetailResult>>;


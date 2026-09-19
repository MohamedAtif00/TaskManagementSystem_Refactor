using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.GetAcademicYearById;

public sealed record GetAcademicYearByIdQuery(int Id) : IQuery<Result<AcademicYearDetailResult>>;


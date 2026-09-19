using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.UpdateAcademicYear;

public sealed record UpdateAcademicYearCommand(int Id, string Name, string? Description) : ICommand<Result<AcademicYearDetailResult>>;


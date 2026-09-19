using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.CreateAcademicYear;

public sealed record CreateAcademicYearCommand(string Name, string? Description) : ICommand<Result<AcademicYearDetailResult>>;


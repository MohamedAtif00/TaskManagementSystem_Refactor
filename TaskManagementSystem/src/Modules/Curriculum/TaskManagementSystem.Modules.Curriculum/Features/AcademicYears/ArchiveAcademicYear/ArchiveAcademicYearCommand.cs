using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.ArchiveAcademicYear;

public sealed record ArchiveAcademicYearCommand(int Id) : ICommand<Result<NoValue>>;


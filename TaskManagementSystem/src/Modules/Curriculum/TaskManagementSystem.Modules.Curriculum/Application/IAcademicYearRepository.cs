using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Application;

public interface IAcademicYearRepository
{
    Task<AcademicYear?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AcademicYear?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AcademicYear>> ListActiveAsync(CancellationToken cancellationToken = default);
    Task AddAsync(AcademicYear academicYear, CancellationToken cancellationToken = default);
}

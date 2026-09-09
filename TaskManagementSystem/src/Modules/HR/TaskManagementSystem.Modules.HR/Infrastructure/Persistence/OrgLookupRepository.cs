using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence;

internal sealed class OrgLookupRepository(HrDbContext context) : IOrgLookupRepository
{
    public async Task<int?> GetSectionHeadIdForTeamAsync(int teamId, CancellationToken cancellationToken = default)
    {
        var sectionId = await context.SectionTeams.AsNoTracking()
            .Where(link => link.TeamId == teamId)
            .Select(link => link.SectionId)
            .FirstOrDefaultAsync(cancellationToken);

        if (sectionId == 0)
        {
            return null;
        }

        return await context.Sections.AsNoTracking()
            .Where(section => section.Id == sectionId)
            .Select(section => (int?)section.HeadId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetTeamIdsForSectionHeadAsync(
        int sectionHeadUserId,
        CancellationToken cancellationToken = default)
    {
        var sectionIds = await context.Sections.AsNoTracking()
            .Where(section => section.HeadId == sectionHeadUserId)
            .Select(section => section.Id)
            .ToListAsync(cancellationToken);

        if (sectionIds.Count == 0)
        {
            return [];
        }

        return await context.SectionTeams.AsNoTracking()
            .Where(link => sectionIds.Contains(link.SectionId))
            .Select(link => link.TeamId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}

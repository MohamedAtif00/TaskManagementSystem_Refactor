using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.HR.Infrastructure;

internal static class HrRequestContextHelper
{
    public static async Task<(int? TeamleaderId, int? SectionheadId)> ResolveApproversAsync(
        IHrUnitOfWork unitOfWork,
        OrgLookupQueries orgLookupQueries,
        int userId,
        string requesterRole,
        CancellationToken cancellationToken)
    {
        var balance = await unitOfWork.EmployeeBalances.GetByUserIdAsync(userId, cancellationToken);
        if (balance is null)
        {
            return (null, null);
        }

        int? sectionHeadId = null;
        if (requesterRole == "TeamLeader" && balance.TeamId.HasValue)
        {
            sectionHeadId = await orgLookupQueries.GetSectionHeadIdForTeamAsync(balance.TeamId.Value, cancellationToken);
        }

        return (balance.TeamleaderId, sectionHeadId);
    }
}

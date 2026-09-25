using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Modules.HR.Features.Leave.GetBalances;
using TaskManagementSystem.Modules.HR.Features.Leave.ListMemberBalances;

namespace TaskManagementSystem.Api.Endpoints.HR.Leave;

public static class GetBalancesEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder leave)
    {
        leave.MapGet("/balances", HandleAsync).RequirePermissionCode(PermissionCodes.HrLeave.Read);
        return leave;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken,
        int? page = null,
        int? pageSize = null)
    {
        if (page.HasValue || pageSize.HasValue)
        {
            var pagedResult = await mediator.Send(new ListMemberBalancesQuery(page, pageSize), cancellationToken);
            return pagedResult.ToHttpResult(pageResult => Results.Ok(new MemberBalanceListPageResponse
            {
                Items = pageResult.Items.Select(item => new MemberBalanceListItemResponse
                {
                    UserId = item.UserId,
                    Code = item.Code,
                    Name = item.Name,
                    AnnualLeave = item.AnnualLeave,
                    AnnualLeaveMax = item.AnnualLeaveMax,
                    EmergencyLeave = item.EmergencyLeave,
                    EmergencyLeaveMax = item.EmergencyLeaveMax,
                    SickLeave = item.SickLeave,
                    Permission = item.Permission,
                    PermissionMax = item.PermissionMax,
                    WorkFromHome = item.WorkFromHome,
                    WorkFromHomeMax = item.WorkFromHomeMax,
                    FromNextBalanceDaysUsed = item.FromNextBalanceDaysUsed
                }).ToList(),
                Page = pageResult.Page,
                PageSize = pageResult.PageSize,
                TotalCount = pageResult.TotalCount
            }));
        }

        var userId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();
        var result = await mediator.Send(new GetBalancesQuery(userId, userId, role), cancellationToken);

        return result.ToHttpResult(balances => Results.Ok(LeaveBalancesMapping.Map(balances)));
    }
}

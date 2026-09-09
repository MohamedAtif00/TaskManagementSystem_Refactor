using MediatR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Leave.GetMedicalCertificate;

namespace TaskManagementSystem.Api.Endpoints.HR.Leave;

public static class GetMedicalCertificateEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder leave)
    {
        leave.MapGet("/leave-requests/{id:int}/medical-certificate", HandleAsync).RequireAuthorization();
        return leave;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();
        var result = await mediator.Send(
            new GetMedicalCertificateQuery(userId, role, id),
            cancellationToken);

        return result.ToHttpResult(file =>
            Results.File(file.Content, file.ContentType, file.FileName));
    }
}

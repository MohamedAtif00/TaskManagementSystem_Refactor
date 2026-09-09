using MediatR;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave.RequestLeave;

namespace TaskManagementSystem.Api.Endpoints.HR.Leave;

public static class RequestLeaveEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder leave)
    {
        leave.MapPost("/leave-requests", HandleAsync).RequireAuthorization().DisableAntiforgery();
        return leave;
    }

    private static async Task<IResult> HandleAsync(
        HttpRequest httpRequest,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();

        LeaveType type;
        DateTime startDate;
        DateTime endDate;
        string? reason;
        string? noteForManager;
        var confirmFromNext = false;
        Stream? medicalContent = null;
        string? medicalFileName = null;

        if (httpRequest.HasFormContentType)
        {
            var form = await httpRequest.ReadFormAsync(cancellationToken);
            type = LeaveRequestMapping.ParseLeaveType(form["type"].ToString()) ?? LeaveType.Sick;
            startDate = DateTime.Parse(form["startDate"].ToString());
            endDate = DateTime.Parse(form["endDate"].ToString());
            reason = form["reason"].ToString();
            noteForManager = form["noteForManager"].ToString();
            confirmFromNext = bool.TryParse(form["confirmFromNextBalance"].ToString(), out var confirmed) && confirmed;

            var file = form.Files.GetFile("medicalCertificate");
            if (file is not null)
            {
                medicalContent = file.OpenReadStream();
                medicalFileName = file.FileName;
            }
        }
        else
        {
            var request = await httpRequest.ReadFromJsonAsync<CreateLeaveRequest>(cancellationToken);
            if (request is null)
            {
                return Results.BadRequest();
            }

            type = LeaveRequestMapping.ParseLeaveType(request.Type) ?? LeaveType.Annual;
            startDate = request.StartDate;
            endDate = request.EndDate;
            reason = request.Reason;
            noteForManager = request.NoteForManager;
            confirmFromNext = request.ConfirmFromNextBalance;
        }

        var result = await mediator.Send(
            new RequestLeaveCommand(
                userId,
                role,
                type,
                startDate,
                endDate,
                reason,
                noteForManager,
                confirmFromNext,
                medicalContent is not null,
                medicalContent,
                medicalFileName),
            cancellationToken);

        return result.ToHttpResult(leaveRequest =>
            Results.Created($"/hr/leave/leave-requests/{leaveRequest.Id}", LeaveRequestMapping.MapLeaveRequest(leaveRequest)));
    }
}

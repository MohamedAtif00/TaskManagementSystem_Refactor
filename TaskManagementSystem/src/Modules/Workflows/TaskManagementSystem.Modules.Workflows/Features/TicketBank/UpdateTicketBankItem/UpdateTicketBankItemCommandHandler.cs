using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Features.TicketBank.UpdateTicketBankItem;

public sealed class UpdateTicketBankItemCommandHandler(
    IWorkflowsUnitOfWork unitOfWork,
    IOrganizationTeamLookup organizationTeamLookup)
    : IRequestHandler<UpdateTicketBankItemCommand, Result<TicketBankListItemResult>>
{
    public async Task<Result<TicketBankListItemResult>> Handle(
        UpdateTicketBankItemCommand request,
        CancellationToken cancellationToken)
    {
        var item = await unitOfWork.TicketBank.GetByIdTrackedAsync(request.TicketBankId, cancellationToken);
        if (item is null)
        {
            return Result.Fail<TicketBankListItemResult>(WorkflowsErrors.TicketBankNotFound);
        }

        if (!await organizationTeamLookup.ActiveTeamExistsAsync(request.TeamId, cancellationToken))
        {
            return Result.Fail<TicketBankListItemResult>(WorkflowsErrors.TeamInvalid);
        }

        var updateResult = item.Update(
            request.Name,
            request.Duration,
            request.Type,
            request.TeamLeaderOnly,
            request.TeamId);
        if (!updateResult.IsSuccess)
        {
            return Result.Fail<TicketBankListItemResult>(updateResult.Error);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(TicketBankListItemResult.From(item));
    }
}


using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Features.TicketBank.CreateTicketBankItem;

public sealed class CreateTicketBankItemCommandHandler(
    IWorkflowsUnitOfWork unitOfWork,
    IOrganizationTeamLookup organizationTeamLookup)
    : IRequestHandler<CreateTicketBankItemCommand, Result<TicketBankListItemResult>>
{
    public async Task<Result<TicketBankListItemResult>> Handle(
        CreateTicketBankItemCommand request,
        CancellationToken cancellationToken)
    {
        if (!await organizationTeamLookup.ActiveTeamExistsAsync(request.TeamId, cancellationToken))
        {
            return Result.Fail<TicketBankListItemResult>(WorkflowsErrors.TeamInvalid);
        }

        var createResult = TicketBankItem.Create(
            request.Name,
            request.Duration,
            request.Type,
            request.TeamLeaderOnly,
            request.TeamId);
        if (!createResult.IsSuccess)
        {
            return Result.Fail<TicketBankListItemResult>(createResult.Error);
        }

        await unitOfWork.TicketBank.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(TicketBankListItemResult.From(createResult.Value));
    }
}


using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome;

namespace TaskManagementSystem.Api.Endpoints.HR.WorkFromHome;

internal static class WorkFromHomeMapping
{
    internal static WorkFromHomeStatus? ParseWorkFromHomeStatus(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : Enum.TryParse<WorkFromHomeStatus>(value, true, out var status)
                ? status
                : null;

    internal static WorkFromHomeRequestResponse MapWorkFromHome(WorkFromHomeRequestResult request) =>
        new()
        {
            Id = request.Id,
            UserId = request.UserId,
            Date = request.Date,
            Status = request.Status.ToString(),
            NoteForManager = request.NoteForManager,
            DateCreated = request.DateCreated,
            Opinions = request.Opinions?.Select(opinion => new OpinionResponse
            {
                Id = opinion.Id,
                UserId = opinion.UserId,
                IsApproved = opinion.IsApproved,
                Comment = opinion.Comment,
                CreatedAt = opinion.CreatedAt
            }).ToList()
        };
}

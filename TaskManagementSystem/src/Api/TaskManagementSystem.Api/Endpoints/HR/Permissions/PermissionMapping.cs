using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Features.Permissions;

namespace TaskManagementSystem.Api.Endpoints.HR.Permissions;

internal static class PermissionMapping
{
    internal static PermissionType? ParsePermissionType(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : Enum.TryParse<PermissionType>(value, true, out var permissionType)
                ? permissionType
                : null;

    internal static PermissionStatus? ParsePermissionStatus(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : Enum.TryParse<PermissionStatus>(value, true, out var status)
                ? status
                : null;

    internal static PermissionRequestResponse MapPermission(PermissionRequestResult permission) =>
        new()
        {
            Id = permission.Id,
            UserId = permission.UserId,
            Type = permission.Type.ToString(),
            Status = permission.Status.ToString(),
            PermissionDate = permission.PermissionDate,
            FromTime = permission.FromTime.ToString("HH:mm"),
            ToTime = permission.ToTime.ToString("HH:mm"),
            Reason = permission.Reason,
            CreatedAt = permission.CreatedAt,
            UpdatedAt = permission.UpdatedAt,
            Opinions = permission.Opinions?.Select(opinion => new OpinionResponse
            {
                Id = opinion.Id,
                UserId = opinion.UserId,
                IsApproved = opinion.IsApproved,
                Comment = opinion.Comment,
                CreatedAt = opinion.CreatedAt
            }).ToList()
        };
}

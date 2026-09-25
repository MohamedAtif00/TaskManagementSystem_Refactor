using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave;

namespace TaskManagementSystem.Modules.HR.Features.Permissions;

public sealed record PermissionRequestResult(
    int Id,
    int UserId,
    PermissionType Type,
    PermissionStatus Status,
    DateTime PermissionDate,
    TimeOnly FromTime,
    TimeOnly ToTime,
    string? Reason,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<OpinionResult>? Opinions = null)
{
    public static PermissionRequestResult From(
        PermissionRequest permission,
        IReadOnlyList<OpinionResult>? opinions = null) =>
        new(
            permission.Id,
            permission.UserId,
            permission.Type,
            permission.Status,
            permission.PermissionDate,
            permission.FromTime,
            permission.ToTime,
            permission.Reason,
            permission.CreatedAt,
            permission.UpdatedAt,
            opinions);
}

public sealed record PermissionRequestListResult(
    IReadOnlyList<PermissionRequestResult> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record BulkPermissionOpinionResult(
    int Succeeded,
    int Failed,
    IReadOnlyList<int> FailedPermissionIds);

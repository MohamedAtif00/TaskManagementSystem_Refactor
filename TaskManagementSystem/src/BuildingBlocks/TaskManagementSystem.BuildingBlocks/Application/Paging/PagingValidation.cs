using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.BuildingBlocks.Application.Paging;

public static class PagingValidation
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static Result<(int Page, int PageSize, long Skip)> Resolve(
        int? page,
        int? pageSize)
    {
        if (page is < 1)
        {
            return Result.Fail<(int Page, int PageSize, long Skip)>(
                new ResultError("invalid_page", "Page must be greater than or equal to 1."));
        }

        if (pageSize is < 1)
        {
            return Result.Fail<(int Page, int PageSize, long Skip)>(
                new ResultError("invalid_page_size", "Page size must be greater than or equal to 1."));
        }

        if (pageSize is > MaxPageSize)
        {
            return Result.Fail<(int Page, int PageSize, long Skip)>(
                new ResultError("invalid_page_size", $"Page size must be less than or equal to {MaxPageSize}."));
        }

        var resolvedPage = page ?? 1;
        var resolvedPageSize = pageSize ?? DefaultPageSize;
        var skip = (long)(resolvedPage - 1) * resolvedPageSize;
        if (skip > int.MaxValue)
        {
            return Result.Fail<(int Page, int PageSize, long Skip)>(
                new ResultError("invalid_page", "The requested page is too large."));
        }

        return Result.Ok((resolvedPage, resolvedPageSize, skip));
    }
}

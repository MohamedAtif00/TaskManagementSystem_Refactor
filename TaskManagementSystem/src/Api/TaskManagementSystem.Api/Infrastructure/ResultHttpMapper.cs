using TaskManagementSystem.BuildingBlocks.Domain;
using HttpResult = Microsoft.AspNetCore.Http.IResult;

namespace TaskManagementSystem.Api.Infrastructure;

public static class ResultHttpMapper
{
    public static HttpResult ToHttpResult<T>(
        this Result<T> result,
        Func<T, HttpResult> onSuccess)
    {
        if (result.IsSuccess)
        {
            return onSuccess(result.Value);
        }

        return ToProblemResult(result.Error);
    }

    public static HttpResult ToProblemResult(ResultError error) =>
        Results.Problem(
            detail: error.Message,
            statusCode: MapStatusCode(error.Code),
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });

    internal static int MapStatusCode(string code) =>
        code switch
        {
            "invalid_login_code" => StatusCodes.Status404NotFound,
            "leave_request_not_found" => StatusCodes.Status404NotFound,
            "user_not_found" => StatusCodes.Status404NotFound,
            "leave_medical_not_found" => StatusCodes.Status404NotFound,
            "holiday_not_found" => StatusCodes.Status404NotFound,
            "leave_opinion_not_authorized" => StatusCodes.Status403Forbidden,
            "permission_not_found" => StatusCodes.Status404NotFound,
            "role_not_found" => StatusCodes.Status404NotFound,
            "cannot_delete_system_permission" => StatusCodes.Status400BadRequest,
            "cannot_delete_system_role" => StatusCodes.Status400BadRequest,
            "duplicate_permission_code" => StatusCodes.Status409Conflict,
            "duplicate_role_name" => StatusCodes.Status409Conflict,
            "role_in_use" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };
}

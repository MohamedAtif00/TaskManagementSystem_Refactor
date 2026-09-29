using HttpResult = Microsoft.AspNetCore.Http.IResult;

namespace TaskManagementSystem.Api.Infrastructure;

public sealed record ApiFailureBody(
    bool Success,
    string Message,
    string? Code = null,
    IReadOnlyDictionary<string, string[]>? Errors = null)
{
    public static ApiFailureBody Create(
        string message,
        string? code = null,
        IReadOnlyDictionary<string, string[]>? errors = null) =>
        new(false, message, code, errors is { Count: > 0 } ? errors : null);
}

public static class ApiFailureResponse
{
    public static HttpResult Ok(string message, string? code = null, IReadOnlyDictionary<string, string[]>? errors = null) =>
        Results.Ok(ApiFailureBody.Create(message, code, errors));
}

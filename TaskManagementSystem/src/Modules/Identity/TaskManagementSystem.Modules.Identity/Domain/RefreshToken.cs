using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Identity.Domain;

public sealed class RefreshToken : Entity
{
    private RefreshToken()
    {
    }

    public string Token { get; internal set; } = string.Empty;
    public DateTime Created { get; internal init; }
    public DateTime Expires { get; internal set; }
    public bool Used { get; internal set; }
    public int UserId { get; internal set; }

    public static RefreshToken Create(string token, DateTime created, DateTime expires, int userId) =>
        new()
        {
            Token = token,
            Created = created,
            Expires = expires,
            UserId = userId,
            Used = false
        };

    public bool IsExpired(DateTime utcNow) => utcNow >= Expires;

    public Result<NoValue> CanRotate(DateTime utcNow) =>
        Used || IsExpired(utcNow)
            ? Result.Fail<NoValue>(new ResultError("invalid_refresh_token", "Token expired"))
            : Result.Ok();

    public void MarkUsed() => Used = true;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Token;
    }
}

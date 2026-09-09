using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Identity.Domain;

public sealed class LoginCode : ValueObject
{
    public string Value { get; }

    private LoginCode(string value) => Value = value;

    public static Result<LoginCode> TryCreate(string raw)
    {
        var normalized = raw.Trim().ToUpperInvariant();
        if (normalized.Length != 6)
        {
            return Result.Fail<LoginCode>(new ResultError("invalid_login_code", "Invalid code"));
        }

        return Result.Ok(new LoginCode(normalized));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}

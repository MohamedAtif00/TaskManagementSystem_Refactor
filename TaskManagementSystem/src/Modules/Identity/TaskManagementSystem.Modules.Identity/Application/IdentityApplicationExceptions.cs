namespace TaskManagementSystem.Modules.Identity.Application;

public sealed class InvalidLoginCodeException : Exception
{
    public InvalidLoginCodeException()
        : base("Invalid code")
    {
    }
}

public sealed class InvalidRefreshTokenException : Exception
{
    public InvalidRefreshTokenException(string message)
        : base(message)
    {
    }
}

public sealed class UserNotFoundException : Exception
{
    public UserNotFoundException()
        : base("Invalid token")
    {
    }
}

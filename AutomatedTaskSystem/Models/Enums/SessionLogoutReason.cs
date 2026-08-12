namespace AutomatedTaskSystem.Models.Enums;

public enum SessionLogoutReason
{
    Manual = 0,
    TokenExpired = 1,
    InactivityTimeout = 2,
    ForcedByAdmin = 3,
    ReplacedByNewLogin = 4
}

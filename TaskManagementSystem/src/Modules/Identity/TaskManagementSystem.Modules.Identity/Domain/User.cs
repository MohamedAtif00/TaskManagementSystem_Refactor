using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Domain.Rules;

namespace TaskManagementSystem.Modules.Identity.Domain;

public sealed class User : Entity, IAggregateRoot
{
    private User()
    {
    }

    internal static User CreateForPersistence() => new();

    public int Id { get; internal set; }
    public string Code { get; internal set; } = string.Empty;
    public string Name { get; internal set; } = string.Empty;
    public string HrCode { get; internal set; } = string.Empty;
    public UserRole Role { get; internal set; } = UserRole.Member;
    public AccountType AccountType { get; internal set; } = AccountType.Internal;
    public bool OnBoard { get; internal set; }
    public bool Archived { get; internal set; }
    public int? TeamId { get; internal set; }
    public int? TeamleaderId { get; internal set; }
    public int AnnualLeave { get; internal set; }
    public int AnnualLeaveMax { get; internal set; }
    public int EmergencyLeave { get; internal set; }
    public int EmergencyLeaveMax { get; internal set; }
    public int SickLeave { get; internal set; }
    public int Permission { get; internal set; }
    public int PermissionMax { get; internal set; }
    public int WorkFromHome { get; internal set; }
    public int WorkFromHomeMax { get; internal set; }
    public int FromNextBalanceDaysUsed { get; internal set; }
    public int OldAnnualBalance { get; internal set; }

    public void EnsureCanAuthenticate()
    {
        CheckRule(new UserMustNotBeArchivedRule(Archived));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}

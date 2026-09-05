using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Identity.Domain.Rules;

internal sealed class UserMustNotBeArchivedRule(bool archived) : IBusinessRule
{
    public string Message => "User is archived.";

    public bool IsBroken() => archived;
}

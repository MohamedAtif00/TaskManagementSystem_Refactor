using TaskManagementSystem.BuildingBlocks.Domain;

using TaskManagementSystem.Modules.Identity.Domain.Events;



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

    public string? Email { get; internal set; }

    public string? Phone { get; internal set; }

    public string? Title { get; internal set; }

    public int RoleId { get; internal set; } = (int)UserRole.Member;

    public Role? Role { get; internal set; }

    public AccountType AccountType { get; internal set; } = AccountType.Internal;

    public bool OnBoard { get; internal set; }

    public bool Archived { get; internal set; }

    public int? TeamId { get; internal set; }



    public string RoleName => Role?.Name ?? ((UserRole)RoleId).ToString();



    public IReadOnlyCollection<string> PermissionCodes =>

        Role?.Permissions.Select(permission => permission.Code).ToList() ?? [];



    public static Result<User> Create(

        string code,

        string name,

        string hrCode,

        string? email,

        string? phone,

        string? title,

        int roleId,

        AccountType accountType,

        int? teamId)

    {

        var normalizedName = name.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))

        {

            return Result.Fail<User>(new ResultError("invalid_user_name", "Name is required."));

        }



        var normalizedHrCode = hrCode.Trim();

        if (string.IsNullOrWhiteSpace(normalizedHrCode))

        {

            return Result.Fail<User>(new ResultError("invalid_hr_code", "HR code is required."));

        }



        if (string.IsNullOrWhiteSpace(code) || code.Length != 6)

        {

            return Result.Fail<User>(new ResultError("invalid_user_code", "User code must be 6 characters."));

        }



        var normalizedEmail = NormalizeEmail(email);

        if (roleId != (int)UserRole.Owner && teamId is null)

        {

            return Result.Fail<User>(new ResultError("team_required", "Team is required for non-owner roles."));

        }



        if (roleId == (int)UserRole.Owner && teamId is not null)

        {

            return Result.Fail<User>(new ResultError("team_not_allowed", "Owner cannot be assigned to a team."));

        }



        return Result.Ok(new User

        {

            Code = code,

            Name = normalizedName,

            HrCode = normalizedHrCode,

            Email = normalizedEmail,

            Phone = NormalizeOptional(phone),

            Title = NormalizeOptional(title),

            RoleId = roleId,

            AccountType = accountType,

            TeamId = teamId,

            OnBoard = false,

            Archived = false

        });

    }



    public Result<NoValue> UpdateProfile(

        string name,

        string hrCode,

        string? email,

        string? phone,

        string? title,

        int roleId,

        AccountType accountType,

        int? teamId)

    {

        var normalizedName = name.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))

        {

            return Result.Fail<NoValue>(new ResultError("invalid_user_name", "Name is required."));

        }



        var normalizedHrCode = hrCode.Trim();

        if (string.IsNullOrWhiteSpace(normalizedHrCode))

        {

            return Result.Fail<NoValue>(new ResultError("invalid_hr_code", "HR code is required."));

        }



        if (roleId != (int)UserRole.Owner && teamId is null)

        {

            return Result.Fail<NoValue>(new ResultError("team_required", "Team is required for non-owner roles."));

        }



        if (roleId == (int)UserRole.Owner && teamId is not null)

        {

            return Result.Fail<NoValue>(new ResultError("team_not_allowed", "Owner cannot be assigned to a team."));

        }



        Name = normalizedName;

        HrCode = normalizedHrCode;

        Email = NormalizeEmail(email);

        Phone = NormalizeOptional(phone);

        Title = NormalizeOptional(title);

        RoleId = roleId;

        AccountType = accountType;

        TeamId = teamId;

        AddDomainEvent(new UserMetadataChangedDomainEvent(Id, teamId, roleId));

        return Result.Ok();

    }



    public void NotifyCreated()

    {

        AddDomainEvent(new UserCreatedDomainEvent(

            Id,

            TeamId,

            RoleId));

    }



    public void AssignRole(int roleId) => RoleId = roleId;



    public void Archive() => Archived = true;



    public Result<NoValue> CanAuthenticate() =>

        Archived

            ? Result.Fail<NoValue>(new ResultError("user_archived", "User is archived."))

            : Result.Ok();



    protected override IEnumerable<object?> GetEqualityComponents()

    {

        yield return Id;

    }



    private static string? NormalizeEmail(string? email) =>

        string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();



    private static string? NormalizeOptional(string? value) =>

        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

}



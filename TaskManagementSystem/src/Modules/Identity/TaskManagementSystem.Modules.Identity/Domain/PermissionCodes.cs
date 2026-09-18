namespace TaskManagementSystem.Modules.Identity.Domain;

public static class PermissionCodes
{
    public const string Read = "read";
    public const string Create = "create";
    public const string Update = "update";
    public const string Delete = "delete";
    public const string Manage = "manage";

    public static class Organization
    {
        public const string Prefix = "organization";
        public const string Read = $"{Prefix}.{PermissionCodes.Read}";
        public const string Create = $"{Prefix}.{PermissionCodes.Create}";
        public const string Update = $"{Prefix}.{PermissionCodes.Update}";
        public const string Delete = $"{Prefix}.{PermissionCodes.Delete}";
        public const string Manage = $"{Prefix}.{PermissionCodes.Manage}";
    }

    public static class Workflows
    {
        public const string Prefix = "workflows";
        public const string Read = $"{Prefix}.{PermissionCodes.Read}";
        public const string Create = $"{Prefix}.{PermissionCodes.Create}";
        public const string Update = $"{Prefix}.{PermissionCodes.Update}";
        public const string Delete = $"{Prefix}.{PermissionCodes.Delete}";
        public const string Manage = $"{Prefix}.{PermissionCodes.Manage}";
    }

    public static class Curriculum
    {
        public const string Prefix = "curriculum";
        public const string Read = $"{Prefix}.{PermissionCodes.Read}";
        public const string Create = $"{Prefix}.{PermissionCodes.Create}";
        public const string Update = $"{Prefix}.{PermissionCodes.Update}";
        public const string Delete = $"{Prefix}.{PermissionCodes.Delete}";
        public const string Manage = $"{Prefix}.{PermissionCodes.Manage}";
    }

    public static class Sprints
    {
        public const string Prefix = "sprints";
        public const string Read = $"{Prefix}.{PermissionCodes.Read}";
        public const string Create = $"{Prefix}.{PermissionCodes.Create}";
        public const string Update = $"{Prefix}.{PermissionCodes.Update}";
        public const string Delete = $"{Prefix}.{PermissionCodes.Delete}";
        public const string Manage = $"{Prefix}.{PermissionCodes.Manage}";
    }

    public static class Tickets
    {
        public const string Prefix = "tickets";
        public const string Read = $"{Prefix}.{PermissionCodes.Read}";
        public const string Create = $"{Prefix}.{PermissionCodes.Create}";
        public const string Update = $"{Prefix}.{PermissionCodes.Update}";
        public const string Delete = $"{Prefix}.{PermissionCodes.Delete}";
        public const string Manage = $"{Prefix}.{PermissionCodes.Manage}";
    }

    public static class Notifications
    {
        public const string Prefix = "notifications";
        public const string Read = $"{Prefix}.{PermissionCodes.Read}";
        public const string Update = $"{Prefix}.{PermissionCodes.Update}";
        public const string Manage = $"{Prefix}.{PermissionCodes.Manage}";
    }

    public static class HrLeave
    {
        public const string Prefix = "hr.leave";
        public const string Read = $"{Prefix}.{PermissionCodes.Read}";
        public const string Create = $"{Prefix}.{PermissionCodes.Create}";
        public const string Update = $"{Prefix}.{PermissionCodes.Update}";
        public const string Delete = $"{Prefix}.{PermissionCodes.Delete}";
        public const string Manage = $"{Prefix}.{PermissionCodes.Manage}";
    }

    public static class HrHolidays
    {
        public const string Prefix = "hr.holidays";
        public const string Read = $"{Prefix}.{PermissionCodes.Read}";
        public const string Create = $"{Prefix}.{PermissionCodes.Create}";
        public const string Update = $"{Prefix}.{PermissionCodes.Update}";
        public const string Delete = $"{Prefix}.{PermissionCodes.Delete}";
        public const string Manage = $"{Prefix}.{PermissionCodes.Manage}";
    }

    public static class HrTimeoff
    {
        public const string Prefix = "hr.timeoff";
        public const string Read = $"{Prefix}.{PermissionCodes.Read}";
        public const string Create = $"{Prefix}.{PermissionCodes.Create}";
        public const string Update = $"{Prefix}.{PermissionCodes.Update}";
        public const string Delete = $"{Prefix}.{PermissionCodes.Delete}";
        public const string Manage = $"{Prefix}.{PermissionCodes.Manage}";
    }

    public static class HrWorkFromHome
    {
        public const string Prefix = "hr.workfromhome";
        public const string Read = $"{Prefix}.{PermissionCodes.Read}";
        public const string Create = $"{Prefix}.{PermissionCodes.Create}";
        public const string Update = $"{Prefix}.{PermissionCodes.Update}";
        public const string Delete = $"{Prefix}.{PermissionCodes.Delete}";
        public const string Manage = $"{Prefix}.{PermissionCodes.Manage}";
    }

    public static class HrForgotClock
    {
        public const string Prefix = "hr.forgotclock";
        public const string Read = $"{Prefix}.{PermissionCodes.Read}";
        public const string Create = $"{Prefix}.{PermissionCodes.Create}";
        public const string Update = $"{Prefix}.{PermissionCodes.Update}";
        public const string Delete = $"{Prefix}.{PermissionCodes.Delete}";
        public const string Manage = $"{Prefix}.{PermissionCodes.Manage}";
    }

    public static class IdentityUsers
    {
        public const string Prefix = "identity.users";
        public const string Read = $"{Prefix}.{PermissionCodes.Read}";
        public const string Create = $"{Prefix}.{PermissionCodes.Create}";
        public const string Update = $"{Prefix}.{PermissionCodes.Update}";
        public const string Delete = $"{Prefix}.{PermissionCodes.Delete}";
        public const string Manage = $"{Prefix}.{PermissionCodes.Manage}";
    }

    public static class IdentityRoles
    {
        public const string Prefix = "identity.roles";
        public const string Read = $"{Prefix}.{PermissionCodes.Read}";
        public const string Create = $"{Prefix}.{PermissionCodes.Create}";
        public const string Update = $"{Prefix}.{PermissionCodes.Update}";
        public const string Delete = $"{Prefix}.{PermissionCodes.Delete}";
        public const string Manage = $"{Prefix}.{PermissionCodes.Manage}";
    }

    public static readonly IReadOnlyList<string> ModulePrefixes =
    [
        Organization.Prefix,
        Workflows.Prefix,
        Curriculum.Prefix,
        Sprints.Prefix,
        Tickets.Prefix,
        Notifications.Prefix,
        HrLeave.Prefix,
        HrHolidays.Prefix,
        HrTimeoff.Prefix,
        HrWorkFromHome.Prefix,
        HrForgotClock.Prefix,
        IdentityUsers.Prefix,
        IdentityRoles.Prefix
    ];

    public static readonly IReadOnlyList<string> All =
    [
        Organization.Read, Organization.Create, Organization.Update, Organization.Delete, Organization.Manage,
        Workflows.Read, Workflows.Create, Workflows.Update, Workflows.Delete, Workflows.Manage,
        Curriculum.Read, Curriculum.Create, Curriculum.Update, Curriculum.Delete, Curriculum.Manage,
        Sprints.Read, Sprints.Create, Sprints.Update, Sprints.Delete, Sprints.Manage,
        Tickets.Read, Tickets.Create, Tickets.Update, Tickets.Delete, Tickets.Manage,
        Notifications.Read, Notifications.Update, Notifications.Manage,
        HrLeave.Read, HrLeave.Create, HrLeave.Update, HrLeave.Delete, HrLeave.Manage,
        HrHolidays.Read, HrHolidays.Create, HrHolidays.Update, HrHolidays.Delete, HrHolidays.Manage,
        HrTimeoff.Read, HrTimeoff.Create, HrTimeoff.Update, HrTimeoff.Delete, HrTimeoff.Manage,
        HrWorkFromHome.Read, HrWorkFromHome.Create, HrWorkFromHome.Update, HrWorkFromHome.Delete, HrWorkFromHome.Manage,
        HrForgotClock.Read, HrForgotClock.Create, HrForgotClock.Update, HrForgotClock.Delete, HrForgotClock.Manage,
        IdentityUsers.Read, IdentityUsers.Create, IdentityUsers.Update, IdentityUsers.Delete, IdentityUsers.Manage,
        IdentityRoles.Read, IdentityRoles.Create, IdentityRoles.Update, IdentityRoles.Delete, IdentityRoles.Manage
    ];

    public static readonly IReadOnlyList<string> ManageCodes =
        All.Where(code => code.EndsWith($".{Manage}", StringComparison.Ordinal)).ToArray();

    public static string GetManageCode(string permissionCode)
    {
        var lastDot = permissionCode.LastIndexOf('.');
        if (lastDot <= 0)
        {
            return permissionCode;
        }

        return $"{permissionCode[..lastDot]}.{Manage}";
    }

    public static bool IsSatisfiedBy(string heldPermissionCode, string requiredPermissionCode)
    {
        if (string.Equals(heldPermissionCode, requiredPermissionCode, StringComparison.Ordinal))
        {
            return true;
        }

        return string.Equals(heldPermissionCode, GetManageCode(requiredPermissionCode), StringComparison.Ordinal);
    }
}

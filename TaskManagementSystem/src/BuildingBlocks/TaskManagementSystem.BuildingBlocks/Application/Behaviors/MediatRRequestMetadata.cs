using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.BuildingBlocks.Application.Behaviors;

internal static class MediatRRequestMetadata
{
    private const string ModuleNamespacePrefix = "TaskManagementSystem.Modules.";

    public static string GetRequestName(Type requestType) => requestType.Name;

    public static string GetKind(Type requestType) =>
        IsCommand(requestType) ? "command" :
        IsQuery(requestType) ? "query" :
        "request";

    public static string GetModuleName(Type requestType)
    {
        var namespaceName = requestType.Namespace ?? string.Empty;
        if (!namespaceName.StartsWith(ModuleNamespacePrefix, StringComparison.Ordinal))
        {
            return "Unknown";
        }

        var remainder = namespaceName[ModuleNamespacePrefix.Length..];
        var separatorIndex = remainder.IndexOf('.');
        return separatorIndex < 0 ? remainder : remainder[..separatorIndex];
    }

    public static bool IsCommand(Type requestType) =>
        typeof(ICommand).IsAssignableFrom(requestType) ||
        requestType.GetInterfaces().Any(
            @interface => @interface.IsGenericType &&
                          @interface.GetGenericTypeDefinition() == typeof(ICommand<>));

    public static bool IsQuery(Type requestType) =>
        requestType.GetInterfaces().Any(
            @interface => @interface.IsGenericType &&
                          @interface.GetGenericTypeDefinition() == typeof(IQuery<>));
}

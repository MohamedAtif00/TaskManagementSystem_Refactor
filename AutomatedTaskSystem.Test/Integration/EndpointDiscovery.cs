using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;

namespace AutomatedTaskSystem.Test.Integration;

public sealed record DiscoveredEndpoint(
    string HttpMethod,
    string Path,
    bool RequiresAuth,
    string ControllerName,
    string ActionName,
    bool SkipSmokeTest = false);

public static class EndpointDiscovery
{
    private static readonly string[] HttpVerbs = ["GET", "POST", "PUT", "PATCH", "DELETE"];

    public static IReadOnlyList<DiscoveredEndpoint> DiscoverAll()
    {
        var endpoints = new List<DiscoveredEndpoint>();
        var assembly = typeof(Program).Assembly;

        foreach (var controller in assembly.GetTypes()
                     .Where(t => !t.IsAbstract && typeof(ControllerBase).IsAssignableFrom(t)))
        {
            var controllerRoute = controller.GetCustomAttribute<RouteAttribute>()?.Template ?? "[controller]";
            var controllerRequiresAuth = controller.GetCustomAttribute<AuthorizeAttribute>() is not null;
            var controllerName = controller.Name.Replace("Controller", "", StringComparison.Ordinal);

            foreach (var method in controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                foreach (var httpAttr in method.GetCustomAttributes().OfType<HttpMethodAttribute>())
                {
                    var httpMethod = httpAttr.HttpMethods.FirstOrDefault();
                    if (httpMethod is null || !HttpVerbs.Contains(httpMethod, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var requiresAuth = controllerRequiresAuth
                                       || method.GetCustomAttribute<AuthorizeAttribute>() is not null;
                    if (method.GetCustomAttribute<AllowAnonymousAttribute>() is not null)
                    {
                        requiresAuth = false;
                    }

                    var templates = httpAttr.Template is null
                        ? new[] { string.Empty }
                        : new[] { httpAttr.Template };

                    foreach (var template in templates)
                    {
                        var path = BuildPath(controllerRoute, template, controllerName);
                        path = AppendQueryParameters(path, method);
                        // Dapper/raw SQL analytics and SSE streams are not supported on the in-memory test database.
                        var skip = path.Contains("/stream", StringComparison.OrdinalIgnoreCase)
                                   || path.Contains("/analytics/", StringComparison.OrdinalIgnoreCase);

                        endpoints.Add(new DiscoveredEndpoint(
                            httpMethod.ToUpperInvariant(),
                            path,
                            requiresAuth,
                            controller.Name,
                            method.Name,
                            skip));
                    }
                }
            }
        }

        return endpoints
            .GroupBy(e => (e.HttpMethod, e.Path, e.RequiresAuth))
            .Select(g => g.First())
            .OrderBy(e => e.Path)
            .ThenBy(e => e.HttpMethod)
            .ToList();
    }

    internal static string BuildPath(string controllerRoute, string? methodTemplate, string controllerName)
    {
        var route = string.IsNullOrEmpty(methodTemplate)
            ? controllerRoute
            : CombineRoutes(controllerRoute, methodTemplate);

        route = route.Replace("[controller]", controllerName, StringComparison.OrdinalIgnoreCase);
        route = SubstituteRouteParameters(route);

        if (!route.StartsWith('/'))
        {
            route = "/" + route;
        }

        return Regex.Replace(route, "/+", "/");
    }

    private static string CombineRoutes(string controllerRoute, string methodTemplate)
    {
        if (methodTemplate.StartsWith('/'))
        {
            return methodTemplate.TrimStart('/');
        }

        if (string.IsNullOrEmpty(controllerRoute))
        {
            return methodTemplate;
        }

        return $"{controllerRoute.TrimEnd('/')}/{methodTemplate.TrimStart('/')}";
    }

    private const string PlaceholderId = "99999";

    private static string SubstituteRouteParameters(string route)
    {
        route = Regex.Replace(route, @"\{[^}:?]+\}", PlaceholderId);
        route = Regex.Replace(route, @"\{[^}:]+\?}", PlaceholderId);
        return route;
    }

    private static string AppendQueryParameters(string path, MethodInfo method)
    {
        var queryParams = method.GetParameters()
            .Where(p => p.GetCustomAttribute<FromQueryAttribute>() is not null)
            .Select(p => $"{p.Name}={PlaceholderId}");

        var query = string.Join("&", queryParams);
        return string.IsNullOrEmpty(query) ? path : $"{path}?{query}";
    }
}

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using TaskManagementSystem.TestCommon.Integration;
using Xunit;
using Xunit.Abstractions;

namespace TaskManagementSystem.Api.IntegrationTests;

[Trait("Category", "TimingReport")]
public sealed class QueryTimingReportTests(TmsWebApplicationFactory factory, ITestOutputHelper output)
    : IClassFixture<TmsWebApplicationFactory>
{
    private static readonly Dictionary<string, string> PathParamVariables = new(StringComparer.OrdinalIgnoreCase)
    {
        ["/curriculum/years/{id}"] = "yearId",
        ["/curriculum/projects/{id}"] = "projectId",
        ["/curriculum/terms/{id}"] = "termId",
        ["/curriculum/subject-groups/{id}"] = "subjectGroupId",
        ["/curriculum/subjects/{id}"] = "subjectId",
        ["/curriculum/units/{id}"] = "unitId",
        ["/curriculum/lessons/{id}"] = "lessonId",
        ["/curriculum/learning-objectives/{id}"] = "loId",
        ["/workflows/schemas/{id}"] = "schemaId",
        ["/workflows/nodes/{id}"] = "nodeId",
        ["/workflows/steps/{id}"] = "stepId",
        ["/organization/teams/{id}"] = "teamId",
        ["/organization/sections/{id}"] = "sectionId",
        ["/identity/users/{id}"] = "userId",
        ["/identity/permissions/{id}"] = "permissionId",
        ["/identity/roles/{id}"] = "roleId",
        ["/sprints/{id}"] = "sprintId",
        ["/tickets/{id}"] = "ticketId",
        ["/notifications/{id}"] = "notificationId",
        ["/hr/leave/leave-requests/{id}"] = "leaveRequestId",
        ["/hr/permissions/{id}"] = "hrPermissionId",
        ["/hr/work-from-home/{id}"] = "wfhRequestId",
        ["/hr/forgot-clock/{id}"] = "forgotClockId",
        ["/hr/holidays/{id}"] = "holidayId"
    };

    [Fact]
    public async Task GenerateGetQueryTimingReport_WritesCsv()
    {
        await factory.SeedTestUserAsync();
        var client = await factory.CreateAuthenticatedClientAsync();

        var variables = await BootstrapIdsAsync(client);
        var getOperations = LoadGetOperations();
        var results = new List<QueryTimingResult>();

        foreach (var operation in getOperations)
        {
            var path = ResolvePath(operation.Template, variables);
            var query = BuildQueryString(operation.Template);
            var url = string.IsNullOrEmpty(query) ? path : $"{path}?{query}";

            var stopwatch = Stopwatch.StartNew();
            using var response = await client.GetAsync(url);
            stopwatch.Stop();

            results.Add(new QueryTimingResult(
                operation.OperationId,
                operation.Template,
                url,
                (int)response.StatusCode,
                stopwatch.ElapsedMilliseconds));

            output.WriteLine($"{response.StatusCode} {stopwatch.ElapsedMilliseconds,5} ms  GET {url}");
        }

        var reportPath = Path.Combine(AppContext.BaseDirectory, "query-timing-report.csv");
        await WriteCsvAsync(reportPath, results);

        output.WriteLine($"Wrote {results.Count} rows to {reportPath}");
        Assert.NotEmpty(results);
    }

    private static async Task<Dictionary<string, string>> BootstrapIdsAsync(HttpClient client)
    {
        var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        await TryExtractFirstIdAsync(client, "/organization/teams", variables, "teamId");
        await TryExtractFirstIdAsync(client, "/organization/sections", variables, "sectionId");
        await TryExtractFirstIdAsync(client, "/workflows/schemas", variables, "schemaId");
        await TryExtractFirstIdAsync(client, "/curriculum/years", variables, "yearId");
        await TryExtractFirstIdAsync(client, "/sprints", variables, "sprintId");
        await TryExtractFirstIdAsync(client, "/identity/users", variables, "userId");
        await TryExtractFirstIdAsync(client, "/notifications?page=1&pageSize=20", variables, "notificationId");
        await TryExtractFirstIdAsync(client, "/hr/leave/leave-requests", variables, "leaveRequestId");
        await TryExtractFirstIdAsync(client, "/hr/permissions", variables, "hrPermissionId");
        await TryExtractFirstIdAsync(client, "/hr/work-from-home", variables, "wfhRequestId");
        await TryExtractFirstIdAsync(client, "/hr/forgot-clock", variables, "forgotClockId");

        if (variables.TryGetValue("yearId", out var yearId))
        {
            await TryExtractFirstIdAsync(client, $"/curriculum/years/{yearId}/projects", variables, "projectId");
        }

        if (variables.TryGetValue("schemaId", out var schemaId))
        {
            await TryExtractFirstIdAsync(client, $"/workflows/schemas/{schemaId}/nodes", variables, "nodeId");
        }

        if (variables.TryGetValue("projectId", out var projectId))
        {
            await TryExtractFirstIdAsync(client, $"/curriculum/projects/{projectId}/terms", variables, "termId");
        }

        if (variables.TryGetValue("nodeId", out var nodeId))
        {
            await TryExtractFirstIdAsync(client, $"/workflows/nodes/{nodeId}/steps", variables, "stepId");
        }

        if (variables.TryGetValue("termId", out var termId))
        {
            await TryExtractFirstIdAsync(client, $"/curriculum/terms/{termId}/subject-groups", variables, "subjectGroupId");
        }

        if (variables.TryGetValue("subjectGroupId", out var subjectGroupId))
        {
            await TryExtractFirstIdAsync(client, $"/curriculum/subject-groups/{subjectGroupId}/subjects", variables, "subjectId");
        }

        if (variables.TryGetValue("subjectId", out var subjectId))
        {
            await TryExtractFirstIdAsync(client, $"/curriculum/subjects/{subjectId}/units", variables, "unitId");
        }

        if (variables.TryGetValue("unitId", out var unitId))
        {
            await TryExtractFirstIdAsync(client, $"/curriculum/units/{unitId}/lessons", variables, "lessonId");
        }

        if (variables.TryGetValue("lessonId", out var lessonId))
        {
            await TryExtractFirstIdAsync(client, $"/curriculum/lessons/{lessonId}/learning-objectives", variables, "loId");
        }

        if (variables.TryGetValue("loId", out var loId))
        {
            await TryExtractFirstIdAsync(client, $"/learning-objectives/{loId}/tickets", variables, "ticketId");
        }

        return variables;
    }

    private static async Task TryExtractFirstIdAsync(
        HttpClient client,
        string path,
        Dictionary<string, string> variables,
        string variableName)
    {
        using var response = await client.GetAsync(path);
        if (!response.IsSuccessStatusCode)
        {
            return;
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var id = ExtractFirstId(document.RootElement);
        if (id.HasValue)
        {
            variables[variableName] = id.Value.ToString();
        }
    }

    private static int? ExtractFirstId(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var id = ExtractFirstId(item);
                if (id.HasValue)
                {
                    return id;
                }
            }

            return null;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("id", out var idProperty) && idProperty.TryGetInt32(out var id))
            {
                return id;
            }

            if (element.TryGetProperty("items", out var items))
            {
                return ExtractFirstId(items);
            }
        }

        return null;
    }

    private static IReadOnlyList<GetOperation> LoadGetOperations()
    {
        var openApiPath = ResolveOpenApiPath();
        var json = File.ReadAllText(openApiPath);
        var root = JsonNode.Parse(json)!.AsObject();
        var paths = root["paths"]!.AsObject();
        var operations = new List<GetOperation>();

        foreach (var pathEntry in paths)
        {
            if (pathEntry.Key.StartsWith("/openapi/", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (pathEntry.Value?["get"] is not JsonObject getOperation)
            {
                continue;
            }

            operations.Add(new GetOperation(
                pathEntry.Key,
                getOperation["operationId"]?.GetValue<string>() ?? string.Empty));
        }

        return operations.OrderBy(operation => operation.Template, StringComparer.Ordinal).ToList();
    }

    private static string ResolveOpenApiPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "src",
                "Api",
                "TaskManagementSystem.Api",
                "openapi",
                "tms-openapi.json");

            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not locate tms-openapi.json from test output directory.");
    }

    private static string ResolvePath(string template, IReadOnlyDictionary<string, string> variables)
    {
        var resolved = template;

        if (PathParamVariables.TryGetValue(template, out var idVariable)
            && variables.TryGetValue(idVariable, out var idValue))
        {
            resolved = resolved.Replace("{id}", idValue, StringComparison.Ordinal);
        }

        foreach (var pair in variables)
        {
            resolved = resolved.Replace($"{{{pair.Key}}}", pair.Value, StringComparison.OrdinalIgnoreCase);
        }

        return resolved;
    }

    private static string BuildQueryString(string template) => template switch
    {
        "/sprints" => "archived=false",
        "/notifications" => "page=1&pageSize=20",
        _ => string.Empty
    };

    private static async Task WriteCsvAsync(string path, IReadOnlyList<QueryTimingResult> results)
    {
        await using var writer = new StreamWriter(path);
        await writer.WriteLineAsync("operationId,template,url,status,elapsedMs");

        foreach (var result in results.OrderByDescending(r => r.ElapsedMs))
        {
            await writer.WriteLineAsync(
                $"{EscapeCsv(result.OperationId)},{EscapeCsv(result.Template)},{EscapeCsv(result.Url)},{result.StatusCode},{result.ElapsedMs}");
        }
    }

    private static string EscapeCsv(string value) =>
        value.Contains('"') || value.Contains(',')
            ? $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\""
            : value;

    private sealed record GetOperation(string Template, string OperationId);

    private sealed record QueryTimingResult(
        string OperationId,
        string Template,
        string Url,
        int StatusCode,
        long ElapsedMs);
}

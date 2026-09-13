namespace TaskManagementSystem.Api.Endpoints;

public static class OpenApiEndpoints
{
    public static WebApplication MapOpenApiEndpoints(this WebApplication app)
    {
        app.MapGet("/openapi/v1.json", () => ServeOpenApi("tms-openapi.json")).AllowAnonymous();
        app.MapGet("/openapi/hr/v1.json", () => ServeOpenApi("hr-openapi.json")).AllowAnonymous();
        app.MapGet("/openapi/forgot-clock/v1.json", () => ServeOpenApi("forgot-clock-openapi.json")).AllowAnonymous();

        return app;
    }

    private static IResult ServeOpenApi(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "openapi", fileName);
        return Results.File(path, "application/json");
    }
}

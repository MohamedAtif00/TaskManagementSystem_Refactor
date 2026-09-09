namespace TaskManagementSystem.Api.Endpoints;

public static class OpenApiEndpoints
{
    public static WebApplication MapOpenApiEndpoints(this WebApplication app)
    {
        app.MapGet("/openapi/v1.json", () =>
        {
            var path = Path.Combine(AppContext.BaseDirectory, "openapi", "hr-openapi.json");
            return Results.File(path, "application/json");
        }).AllowAnonymous();

        return app;
    }
}

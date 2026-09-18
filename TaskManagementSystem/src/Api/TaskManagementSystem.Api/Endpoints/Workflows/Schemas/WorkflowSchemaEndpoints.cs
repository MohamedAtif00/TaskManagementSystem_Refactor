namespace TaskManagementSystem.Api.Endpoints.Workflows.Schemas;

public static class WorkflowSchemaEndpoints
{
    public static RouteGroupBuilder MapWorkflowSchemaEndpoints(this RouteGroupBuilder group)
    {
        var schemas = group.MapGroup("/schemas");

        ListSchemasEndpoint.Map(schemas);
        GetSchemaByIdEndpoint.Map(schemas);
        CreateSchemaEndpoint.Map(schemas);
        UpdateSchemaEndpoint.Map(schemas);
        ArchiveSchemaEndpoint.Map(schemas);
        ListNodesBySchemaEndpoint.Map(schemas);
        CreateNodeEndpoint.Map(schemas);

        return group;
    }
}

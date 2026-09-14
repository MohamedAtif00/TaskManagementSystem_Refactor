namespace TaskManagementSystem.Modules.Curriculum.Application;

public interface IWorkflowSchemaLookup
{
    Task<bool> ActiveSchemaExistsAsync(int schemaId, CancellationToken cancellationToken = default);
}

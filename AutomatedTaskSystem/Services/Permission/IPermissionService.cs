using AutomatedTaskSystem.Dtos.PermissionDtos;

namespace AutomatedTaskSystem.Services.Permission
{
    public interface IPermissionService
    {
        Task<bool> AddPermissionAsync(CreatePermissionDto permission);
        Task<bool> DeletePermissionAsync(int id);
        Task<List<Models.Permission>> GetAllPermissionsAsync();
        Task<Models.Permission?> GetPermissionByIdAsync(int id);
        Task<bool> UpdatePermissionAsync(UpdatePermissionDto dto);
    }
}
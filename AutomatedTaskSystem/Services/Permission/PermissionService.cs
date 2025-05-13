using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.PermissionDtos;
using AutomatedTaskSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AutomatedTaskSystem.Services.Permission
{
    public class PermissionService : IPermissionService
    {
        private readonly DataContext _context;

        public PermissionService(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> AddPermissionAsync(CreatePermissionDto permission)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == permission.UserId);
                if (user == null) return false;

                // Custom "organization month" range
                var today = DateTime.UtcNow;
                DateTime periodStart, periodEnd;

                if (today.Day >= 21)
                {
                    periodStart = new DateTime(today.Year, today.Month, 21);
                    periodEnd = periodStart.AddMonths(1).AddDays(-1); // Until 20th of next month
                }
                else
                {
                    periodEnd = new DateTime(today.Year, today.Month, 20);
                    periodStart = periodEnd.AddMonths(-1).AddDays(1); // From 21st of previous month
                }

                var totalPermissions = await _context.Permissions
                    .Where(p => p.UserId == permission.UserId &&
                                p.CreatedAt >= periodStart &&
                                p.CreatedAt <= periodEnd)
                    .CountAsync();

                if (totalPermissions >= 2)
                    return false;

                var newPermission = new Models.Permission
                {
                    UserId = permission.UserId,
                    Type = permission.Type,
                    Reason = permission.Reason,
                    CreatedAt = DateTime.UtcNow,
                    PermissionDate = DateTime.Parse( permission.PermissionDate) // Add the PermissionDate to the entity
                };

                await _context.Permissions.AddAsync(newPermission);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log ex.Message here if using a logger
                return false;
            }
        }



        public async Task<List<Models.Permission>> GetAllPermissionsAsync()
        {
            return await _context.Permissions
                .Include(p => p.User) // Optional: include user info
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Models.Permission?> GetPermissionByIdAsync(int id)
        {
            return await _context.Permissions
                .Include(p => p.User) // Optional
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> UpdatePermissionAsync(UpdatePermissionDto dto)
        {
            try
            {
                var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == dto.Id);
                if (permission == null) return false;

                permission.Type = dto.Type;
                permission.Reason = dto.Reason;
                permission.UpdatedAt = DateTime.UtcNow;

                _context.Permissions.Update(permission);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log ex.Message
                return false;
            }
        }

        public async Task<bool> DeletePermissionAsync(int id)
        {
            try
            {
                var permission = await _context.Permissions.FindAsync(id);
                if (permission == null) return false;

                _context.Permissions.Remove(permission);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log ex.Message
                return false;
            }
        }
    }
}

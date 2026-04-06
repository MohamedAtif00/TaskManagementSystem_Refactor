using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;

namespace AutomatedTaskSystem.Services.ProjectAssignmentService;

public interface IProjectAssignmentService
{
	Task<ResponseService<List<User>>> GetAssignedUsersForProject(int Pid);
	Task<ResponseService<List<User>>> GetUnassignedUsersForProject(int Pid);
	Task<ResponseService<List<User>>> AssignUsersToProject(int Pid, List<int> userIds);
	Task<ResponseService<List<User>>> UnassignUsersToProject(int Pid, List<int> userIds);
	Task<ResponseService<List<Models.Project>>> ProjectsAssignedToUser(int Uid);
}
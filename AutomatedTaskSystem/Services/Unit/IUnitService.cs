using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
//using Project = AutomatedTaskSystem.Models.Project;

namespace AutomatedTaskSystem.Services.UnitService;

public interface IUnitService
{
	Task<ResponseService<Unit>> CreateUnit(string Name, Models.Subject subject);
}
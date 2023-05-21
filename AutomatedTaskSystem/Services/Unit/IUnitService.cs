using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;

namespace AutomatedTaskSystem.Services.UnitService;

public interface IUnitService
{
	Task<ResponseService<Unit>> CreateUnit(string Name, Project Project);
}
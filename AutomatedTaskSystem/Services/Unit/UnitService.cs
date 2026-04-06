using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;

namespace AutomatedTaskSystem.Services.UnitService;

public class UnitService : IUnitService
{
	private readonly DataContext _context;

	public UnitService(DataContext context)
	{
		_context = context;
	}
	public async Task<ResponseService<Unit>> CreateUnit(string Name, Models.Project Project)
	{
		var newUnit = new Unit
		{
			Archived = false,
			Name = Name,
			Project = Project,
			ProjectId = Project.Id
		};

		_context.Units.Add(newUnit);
		Project.Units.Add(newUnit);
		await _context.SaveChangesAsync();

		return new ResponseService<Unit>
		{
			Data = newUnit,
			Error = false,
			Message = "Unit created"
		};
	}
}
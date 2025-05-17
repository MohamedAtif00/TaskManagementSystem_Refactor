using AutomatedTaskSystem.DTO;
using Microsoft.AspNetCore.Mvc;
using AutomatedTaskSystem.Services.GroupService;
using AutomatedTaskSystem.Services.ResponseService;

namespace AutomatedTaskSystem.Controllers
{
	[Route("groups")]
	[ApiController]
	public class GroupsController : ControllerBase
	{
		private readonly IGroupService _groupService;

		public GroupsController(IGroupService groupService)
		{
			_groupService = groupService;
		}
		// GET:
		// Get single group info
		[HttpGet("{id}")]
		public async Task<ActionResult<ResponseService<Responses.GroupDTO>>> FindGroup(int id)
		=> await _groupService.FindGroup(id);

		// PATCH:
		// Edit Group
		[HttpPatch("{id}")]
		public async Task<ActionResult<ResponseService<Responses.GroupDTO>>> EditGroup(int id, Requests.GroupDTO req)
		=> await _groupService.EditGroup(id, req.Name, req.ColorCode);
		// POST:
		// Create new Group
		[HttpPost]
		public async Task<ActionResult<ResponseService<Responses.GroupDTO>>> CreateGroup(Requests.GroupDTO req)
		=> await _groupService.CreateGroup(req.Name, req.ColorCode);
		// GET:
		// Fetch All Groups
		[HttpGet]
		public async Task<ActionResult<ResponseService<List<Responses.GroupDTO>>>> GetGroups()
		=> await _groupService.GetAllGroups();
		// GET:
		// Fetch All Groups
		[HttpGet("mini")]
		public async Task<ActionResult<ResponseService<List<Responses.IDName>>>> GetGroupsMinified()
		=> await _groupService.GetAllGroupsSimple();
		// GET:
		// Fetch Teamleader For Group
		[HttpGet("GetTeamLeader/{id}")]
		public async Task<ActionResult<ResponseService<List<Responses.IDName>>>> GetTeamLeaderForGroup(int id)
			=> await _groupService.GetTmForGroup(id);
		
		
	}
}

using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.GroupService;

public interface IGroupService
{
	Task<ActionResult<ResponseService<Responses.GroupDTO>>> FindGroup(int Id);
	Task<ActionResult<ResponseService<Responses.GroupDTO>>> CreateGroup(string Name, string ColorCode, Section Section);
	Task<ActionResult<ResponseService<Responses.GroupDTO>>> CreateGroup(string Name, string ColorCode);
	Task<ActionResult<ResponseService<Responses.GroupDTO>>> EditGroup(int Id, string Name, string ColorCode);
	Task<ActionResult<ResponseService<List<Responses.IDName>>>> GetAllGroupsSimple();
	Task<ActionResult<ResponseService<List<Responses.GroupDTO>>>> GetAllGroups();
    Task<ActionResult<ResponseService<List<Responses.IDName>>>> GetTmForGroup(int id);
}

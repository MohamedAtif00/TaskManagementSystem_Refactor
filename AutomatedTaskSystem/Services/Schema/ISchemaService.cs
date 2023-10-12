using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Dtos.Schema;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.SchemaService;

public interface ISchemaService
{
	Task<ActionResult<ResponseService<Responses.SchemaDTO>>> CreateSchema(
		string Name,
		string Description,
		int? TypeId
	);
	Task<ActionResult<ResponseService<List<Responses.IDName>>>> GetSchemaTypes();
	Task<ActionResult<ResponseService<Responses.SchemaDTO>>> GetSchema(int id);
	Task<ActionResult<ResponseService<Responses.SchemaDTO>>> EditSchema(
		int id,
		string Name,
		string Description,
		int? TypeId
	);
	Task<ActionResult<ResponseService<Responses.SchemaDTO>>> DuplicateSchema(int id);
	Task<ActionResult<BaseResponseService>> DeleteSchema(int id);
	Task<ActionResult<ResponseService<List<GetNodePointDto>>>> GetSchemaPoints(int id);
}

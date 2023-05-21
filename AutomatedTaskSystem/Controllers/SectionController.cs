using AutomatedTaskSystem.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutomatedTaskSystem.Services.SectionService;
using AutomatedTaskSystem.Services.UserService;
using AutomatedTaskSystem.Services.GroupService;
using AutomatedTaskSystem.Services.ResponseService;

namespace AutomatedTaskSystem.Controllers
{
    [Route("sections")]
    [ApiController]
    public class SectionController : ControllerBase
    {
        private readonly ISectionService _sectionService;

        public SectionController(
            ISectionService sectionService,
            IUserService userService,
            IGroupService groupService
        )
        {
            _sectionService = sectionService;
        }

        // GET:
        // Get all Sections
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ResponseService<List<Responses.IDName>>>> GetSections() =>
            await _sectionService.GetSections();

        // POST:
        // Create Section
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ResponseService<Responses.IDName>>> CreateSection(
            Requests.SectionDTO req
        ) => await _sectionService.CreateSection(req.Name, req.HeadId, req.Groups);

        // GET:
        // Get details
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ResponseService<Responses.SectionDTO>>> GetSection(int id) =>
            await _sectionService.GetSection(id);
    }
}

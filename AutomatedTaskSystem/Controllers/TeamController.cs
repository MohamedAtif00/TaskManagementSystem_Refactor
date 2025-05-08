using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutomatedTaskSystem.Controllers
{
    [Route("teams")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly DataContext _context;

        public TeamsController(DataContext context)
        {
            _context = context;
        }

        // GET: /teams/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Responses.TeamDTO>> GetTeam(int id)
        {
            var team = await _context.Teams
                .Where(t => t.Id == id)
                .Include(t => t.Users)
                .FirstOrDefaultAsync();

            if (team == null)
            {
                return NotFound(new Responses.BadRequestsDTO("Team not found"));
            }

            var users = await _context.Users
                .Where(u => u.TeamId == team.Id && !u.Archived)
                .Include(u => u.Group)
                .Select(u => new Responses.TeamUser
                {
                    Id = u.Id,
                    Name = u.Name,
                    Group = u.Group.Name
                })
                .ToListAsync();

            return new Responses.TeamDTO
            {
                Id = team.Id,
                Name = team.Name,
                Users = users
            };
        }

        // GET: /teams
        [HttpGet]
        public async Task<ActionResult<List<Responses.MiniTeamDTO>>> GetTeams()
        {
            var teams = await _context.Teams
                .Where(t => !t.Archived)
                .Include(t => t.Users)
                .Select(t => new Responses.MiniTeamDTO
                {
                    Id = t.Id,
                    Name = t.Name,
                    Members = t.Users.Count
                })
                .ToListAsync();

            return teams;
        }

        // POST: /teams
        [HttpPost]
        public async Task<ActionResult<Responses.MiniTeamDTO>> AddTeam(Requests.TeamDTO req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return BadRequest(new Responses.BadRequestsDTO("Team name is required"));

            var newTeam = new Team { Name = req.Name, Archived = false };

            _context.Teams.Add(newTeam);
            await _context.SaveChangesAsync();

            return new Responses.MiniTeamDTO
            {
                Id = newTeam.Id,
                Name = newTeam.Name,
                Members = 0
            };
        }

        // PATCH: /teams/{id}
        [HttpPatch("{id}")]
        public async Task<ActionResult<Responses.MiniTeamDTO>> EditTeam(int id, Requests.TeamDTO req)
        {
            var team = await _context.Teams
                .Include(t => t.Users)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
                return NotFound(new Responses.BadRequestsDTO("Team not found"));

            if (!string.IsNullOrWhiteSpace(req.Name))
                team.Name = req.Name;

            await _context.SaveChangesAsync();

            return new Responses.MiniTeamDTO
            {
                Id = team.Id,
                Name = team.Name,
                Members = team.Users.Count
            };
        }

        // POST: /teams/{id}/assign
        [HttpPost("{id}/assign")]
        public async Task<ActionResult<Responses.TeamDTO>> AssignUsers(int id, Requests.TeamAssignmentDTO req)
        {
            if (req.UserIds == null || req.UserIds.Count == 0)
                return BadRequest(new Responses.BadRequestsDTO("No user IDs provided"));

            var team = await _context.Teams
                .Include(t => t.Users)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
                return NotFound(new Responses.BadRequestsDTO("Team not found"));

            foreach (var userId in req.UserIds)
            {
                var user = await _context.Users
                    .Where(u => u.Id == userId && !u.Archived)
                    .FirstOrDefaultAsync();

                if (user == null)
                    return NotFound(new Responses.BadRequestsDTO($"User with ID {userId} not found"));

                if (user.TeamId != id)
                {
                    user.Team = team;
                    team.Users.Add(user);
                }
            }

            await _context.SaveChangesAsync();

            var users = await _context.Users
                .Where(u => u.TeamId == team.Id && !u.Archived)
                .Include(u => u.Group)
                .Select(u => new Responses.TeamUser
                {
                    Id = u.Id,
                    Name = u.Name,
                    Group = u.Group.Name
                })
                .ToListAsync();

            return new Responses.TeamDTO
            {
                Id = team.Id,
                Name = team.Name,
                Users = users
            };
        }

        // POST: /teams/{id}/unassign
        [HttpPost("{id}/unassign")]
        public async Task<ActionResult<Responses.TeamDTO>> UnassignUsers(int id, Requests.TeamAssignmentDTO req)
        {
            if (req.UserIds == null || req.UserIds.Count == 0)
                return BadRequest(new Responses.BadRequestsDTO("No user IDs provided"));

            var team = await _context.Teams
                .Include(t => t.Users)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
                return NotFound(new Responses.BadRequestsDTO("Team not found"));

            foreach (var userId in req.UserIds)
            {
                var user = team.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                    return NotFound(new Responses.BadRequestsDTO($"User with ID {userId} not found"));

                user.Team = null;
                team.Users.Remove(user);
            }

            await _context.SaveChangesAsync();

            var users = await _context.Users
                .Where(u => u.TeamId == team.Id && !u.Archived)
                .Include(u => u.Group)
                .Select(u => new Responses.TeamUser
                {
                    Id = u.Id,
                    Name = u.Name,
                    Group = u.Group.Name
                })
                .ToListAsync();

            return new Responses.TeamDTO
            {
                Id = team.Id,
                Name = team.Name,
                Users = users
            };
        }

        // DELETE: /teams/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<Responses.SuccessDTO>> ArchiveTeam(int id)
        {
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
                return NotFound(new Responses.BadRequestsDTO("Team not found"));

            team.Archived = true;
            await _context.SaveChangesAsync();

            return new Responses.SuccessDTO("Team archived successfully");
        }
    }
}

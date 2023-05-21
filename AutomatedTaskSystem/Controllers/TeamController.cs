using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using Microsoft.AspNetCore.Mvc;

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
		// GET:
		// Get one team
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

			var users = new List<Responses.TeamUser> { };

			for (int i = 0; i < team.Users.Count; i++)
			{
				var user = team.Users[i];

				var group = await _context.Groups
					.Where(g => g.Id == user.GroupId)
					.FirstOrDefaultAsync();

				users.Add(new Responses.TeamUser
				{
					Group = group!.Name,
					Id = user.Id,
					Name = user.Name
				});
			}

			var res = new Responses.TeamDTO
			{
				Id = team.Id,
				Name = team.Name,
				Users = users
			};

			return res;
		}
		// GET:
		// Get all teams
		public async Task<ActionResult<List<Responses.MiniTeamDTO>>> GetTeams()
		{
			var res = new List<Responses.MiniTeamDTO> { };

			var teams = await _context.Teams
				.Where(t => !t.Archived)
				.Include(t => t.Users)
				.ToListAsync();

			teams.ForEach(t => res.Add(new Responses.MiniTeamDTO
			{
				Id = t.Id,
				Members = t.Users.Count,
				Name = t.Name
			}));

			return res;
		}
		// POST:
		// Add team
		[HttpPost]
		public async Task<ActionResult<Responses.MiniTeamDTO>> AddTeam(Requests.TeamDTO req)
		{
			var newTeam = new Team
			{
				Name = req.Name,
				Archived = false
			};

			_context.Teams.Add(newTeam);
			await _context.SaveChangesAsync();


			return new Responses.MiniTeamDTO
			{
				Id = newTeam.Id,
				Name = newTeam.Name,
				Members = 0
			};
		}
		// PATCH:
		// Edit team
		[HttpPatch("{id}")]
		public async Task<ActionResult<Responses.MiniTeamDTO>> EditTeam(int id, Requests.TeamDTO req)
		{
			var team = await _context.Teams
				.Where(t => t.Id == id)
				.Include(t => t.Users)
				.FirstOrDefaultAsync();

			if (team == null)
			{
				return NotFound(new Responses.BadRequestsDTO("Team not found"));
			}

			if (req.Name != "")
				team.Name = req.Name;

			await _context.SaveChangesAsync();

			return new Responses.MiniTeamDTO
			{
				Id = team.Id,
				Members = team.Users.Count,
				Name = team.Name
			};
		}
		// POST:
		// Assign user to team
		[HttpPost("{id}/assign")]
		public async Task<ActionResult<Responses.TeamDTO>> AssignUsers(int id, Requests.TeamAssignmentDTO req)
		{
			var team = await _context.Teams
				.Where(t => t.Id == id)
				.Include(t => t.Users)
				.FirstOrDefaultAsync();

			if (team == null)
			{
				return NotFound(new Responses.BadRequestsDTO("Team not found"));
			}

			for (int i = 0; i < req.UserIds.Count; i++)
			{
				var user = await _context.Users
					.Where(u => u.Id == req.UserIds[i])
					.FirstOrDefaultAsync();

				if (user == null)
				{
					return NotFound(new Responses.BadRequestsDTO($"User of id:{req.UserIds[i]} is not found"));
				}

				user.Team = team;
				user.TeamId = team.Id;
				team.Users.Add(user);
			}

			await _context.SaveChangesAsync();

			var users = new List<Responses.TeamUser> { };

			for (int i = 0; i < team.Users.Count; i++)
			{
				var user = team.Users[i];
				var group = await _context.Groups
					.Where(g => g.Id == user.GroupId)
					.FirstOrDefaultAsync();
				users.Add(new Responses.TeamUser
				{
					Group = group!.Name,
					Id = user.Id,
					Name = user.Name
				});
			}

			return new Responses.TeamDTO
			{
				Id = team.Id,
				Name = team.Name,
				Users = users
			};
		}
		// POST:
		// Unassign user from team
		[HttpPost("{id}/unassign")]
		public async Task<ActionResult<Responses.TeamDTO>> UnassignUsers(int id, Requests.TeamAssignmentDTO req)
		{
			var team = await _context.Teams
				.Where(t => t.Id == id)
				.Include(t => t.Users)
				.FirstOrDefaultAsync();

			if (team == null)
			{
				return NotFound(new Responses.BadRequestsDTO("Team not found"));
			}

			for (int i = 0; i < req.UserIds.Count; i++)
			{
				var user = team.Users.Find(u => u.Id == req.UserIds[i]);

				if (user == null)
				{
					return NotFound(new Responses.BadRequestsDTO($"User of id:{req.UserIds[i]} is not found"));
				}

				user.Team = null;
				user.TeamId = null;
				team.Users.Remove(user);
			}

			await _context.SaveChangesAsync();

			var users = new List<Responses.TeamUser> { };

			for (int i = 0; i < team.Users.Count; i++)
			{
				var user = team.Users[i];
				var group = await _context.Groups
					.Where(g => g.Id == user.GroupId)
					.FirstOrDefaultAsync();
				users.Add(new Responses.TeamUser
				{
					Group = group!.Name,
					Id = user.Id,
					Name = user.Name
				});
			}

			return new Responses.TeamDTO
			{
				Id = team.Id,
				Name = team.Name,
				Users = users
			};
		}
		// DELETE:
		// Archive team
		[HttpDelete("{id}")]
		public async Task<ActionResult<Responses.SuccessDTO>> ArchiveTeam(int id)
		{
			var team = await _context.Teams
				.Where(t => t.Id == id)
				.FirstOrDefaultAsync();
			if (team == null)
				return NotFound(new Responses.BadRequestsDTO("Team not found"));

			team.Archived = true;
			await _context.SaveChangesAsync();
			return new Responses.SuccessDTO("Team is not archived");
		}
	}
}
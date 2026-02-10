using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.UserService;
using Microsoft.EntityFrameworkCore;

namespace AutomatedTaskSystem.Seeding
{
    public class DataSeeder
    {
        private readonly IUserService _userService;
        private readonly DataContext _context;

        public DataSeeder(IUserService userService, DataContext context)
        {
            _userService = userService;
            _context = context;
        }

        public async System.Threading.Tasks.Task Seed() {

            // Check if the database is empty
	            var isEmpty = !_context.Users.Any(x => x.Role == Models.Enums.UserRole.UserRoleEnum.Owner);
            // If it is, create a new user
            if (isEmpty)
            {
	                Requests.UserDTO owner = new Requests.UserDTO
	                {
	                    Name = "Eman Khalil",
	                    GroupId = null,
	                    Role = Models.Enums.UserRole.UserRoleEnum.Owner,
                    TeamleaderId = null,
                    Title = "Owner",
                    HrCode = "3333",
                    Email = "emanKhalil",
                    Phone = "123456789",
                    AccountType = Models.Enums.AccountTypeEnum.Internal,

                    Vacation = new Responses.VacationDto
                    {
                        Annual = 0,
                        Annual_MAX = 30,
                        Emergency = 0,
                        Emergency_MAX = 5,
                        Sick = 0
                    },

                };
                // Create the user
                var result = await _userService.CreateUser(owner);
            }

            // Seed demo sprint with no learning objectives for testing empty state UI
            await SeedEmptyLOSprintAsync();
        }

        /// <summary>
        /// Seeds a demo sprint with no associated learning objectives.
        /// This is used to test the empty state UI in the sprint overview charts.
        /// When a sprint has no learning objectives, the UI should display:
        /// - "No learning objectives available" in the Project Tags section
        /// - "No learning objectives in this sprint" in the LO Summary chart
        /// </summary>
        private async System.Threading.Tasks.Task SeedEmptyLOSprintAsync()
        {
            const string demoSprintName = "Demo Sprint - No Learning Objectives";

            // Check if the demo sprint already exists
            var existingSprint = await _context.Sprints
                .FirstOrDefaultAsync(s => s.Name == demoSprintName);

            if (existingSprint != null)
            {
                // Demo sprint already exists, skip seeding
                return;
            }

            // Create a sprint with no learning objectives
            var demoSprint = new Sprint
            {
                Name = demoSprintName,
                Description = "This is a demo sprint created to test the empty state UI. It has no associated learning objectives, so the overview charts should display appropriate empty state messages.",
                StartDate = DateTime.Now.AddDays(-7),
                EndDate = DateTime.Now.AddDays(14),
                IsArchived = false,
                SprintLearningObjectives = new List<SprintLearningObjective>() // Empty - no LOs
            };

            _context.Sprints.Add(demoSprint);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[DataSeeder] Created demo sprint '{demoSprintName}' (ID: {demoSprint.Id}) with no learning objectives for empty state UI testing.");
        }
    }
}

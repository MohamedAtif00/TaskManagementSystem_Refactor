using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Services.UserService;

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

        public async Task Seed() {

            // Check if the database is empty
            var isEmpty = !_context.Users.Any(x => x.Role == Models.Enums.UserRole.UserRoleEnum.Owner);
            // If it is, create a new user
            if (isEmpty)
            { 
                Requests.UserDTO owner = new Requests.UserDTO
                {
                    Name = "Eman Khalil",
                    GroupId = 1,
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

            // You can add more seeding logic here as needed

        }
    }
}

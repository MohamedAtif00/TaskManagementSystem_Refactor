using AutomatedTaskSystem.Models.Enums;
using AutomatedTaskSystem.Models.Enums.UserRole;
using static AutomatedTaskSystem.DTO.Responses;

namespace AutomatedTaskSystem.DTO
{
    public static partial class Responses
    {
        public class UserDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public IDName Group { get; set; } = new IDName { };
            public UserRoleEnum Role { get; set; } = UserRoleEnum.Member;
            public string Code { get; set; } = "";
            public string HrCode { get; set; } = "";
            public string? Email { get; set; }
            public AccountTypeEnum AccountType { get; set; } = AccountTypeEnum.Internal;
            public VacationDto Vacation { get; set; } = new VacationDto { };
        }
        public class UserAddedDTO
        {
            public string Code { get; set; } = "";
            public UserDTO User { get; set; } = new UserDTO { };
        }

        public class VacationDto
        {

            public int Annual { get; set; }
            public int Sick { get; set; }
            public int Emergency { get; set; }
        }
    }

    public static partial class Requests
    {
        public class UserDTO
        {
            public string Name { get; set; } = "";
            public int GroupId { get; set; }
            public UserRoleEnum Role { get; set; } = UserRoleEnum.Member;
            public string Code { get; set; } = "";
            public int? Teamleader { get; set; }
            public string HrCode { get; set; } = "";
            public string? Email { get; set; }
            public AccountTypeEnum AccountType { get; set; } = AccountTypeEnum.Internal;
            public VacationDto Vacation { get; set; } = new VacationDto { };
        }
    }
}

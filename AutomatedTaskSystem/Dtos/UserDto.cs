using AutomatedTaskSystem.Models;
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
            public bool Archived { get; set; } = false;
            public string Code { get; set; } = "";
            public bool OnBoard { get; set; } = false;
            public string Name { get; set; } = "";
            public IDName? Group { get; set; } 
            public AccountTypeEnum AccountType { get; set; } = AccountTypeEnum.Internal;
            public int Annual_leave_MAX { get; set; }
            public int Annual_leave { get; set; } = 0;
            public int Sick_leave { get; set; } = 0;
            public int Emergency_leave_MAX { get; set; }
            public int Emergency_leave { get; set; } = 0;
            public int Permission_MAX { get; set; }
            public int Permission { get; set; } = 0;
            public string HrCode { get; set; } = "";
            public string? Email { get; set; }
            public string? Phone { get; set; }
            public string? Title { get; set; }
            public bool? IsAchived { get; set; }
            public int? TeamleaderId { get; set; }
            public UserDTO? Teamleader { get; set; }
            public int? GroupId { get; set; }
            public UserRoleEnum Role { get; set; } = UserRoleEnum.Member;
            public VacationDto Vacation { get; set; } = new VacationDto { };
        }
        public class UserAddedDTO
        {
            public string Code { get; set; } = "";
            public UserDTO User { get; set; } = new UserDTO { };
        }

        public class VacationDto
        {
            public int Annual_MAX { get; set; }
            public int Annual { get; set; }
            public int Sick { get; set; }
            public int Emergency_MAX { get; set; }
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
            public int? TeamleaderId { get; set; }
            public string HrCode { get; set; } = "";
            public string Email { get; set; } = "";
            public string Phone { get; set; } = "";
            public string Title { get; set; } = "";
            public AccountTypeEnum AccountType { get; set; } = AccountTypeEnum.Internal;
            public int Permission_MAX { get; set; }
            public int Permission { get; set; }
            public VacationDto Vacation { get; set; } = new VacationDto { };
        }
    }
}

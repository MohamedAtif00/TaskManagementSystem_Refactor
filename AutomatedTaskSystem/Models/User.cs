    using System.ComponentModel.DataAnnotations;
    using AutomatedTaskSystem.Models.Enums;
    using AutomatedTaskSystem.Models.Enums.UserRole;

    namespace AutomatedTaskSystem.Models;

    public class User
    {
        public bool Archived { get; set; } = false;
        public int Id { get; set; }
        [MaxLength(6), MinLength(6)]
        public string Code { get; set; } = "";
        public bool OnBoard { get; set; } = false;
        public string Name { get; set; } = "";
        public AccountTypeEnum AccountType { get; set; } = AccountTypeEnum.Internal;
        public int Annual_leave_MAX { get; set; }
        public int Annual_leave { get; set; } = 0;
        public int Sick_leave { get; set; } = 0;
        public int Emergency_leave_MAX { get; set; }
        public int Emergency_leave { get; set; } = 0;   
        public int Permission_MAX { get; set; }
        public int Permission { get; set; } = 0;
        public int WorkFromHome { get; set; } = 0;
        public int WorkFromHome_MAX { get; set; } = 0;
        public int FromNextBalanceDaysUsed { get; set; } = 0;
        public int OldAnnualBalance { get; set; } = 0;
        public string HR_code { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Title { get; set; } 
        public User? Teamleader { get; set; }
        public int? TeamleaderId { get; set; }
        public List<Project> Projects { get; set; } = new List<Project> { };
        public List<Task> Tasks { get; set; } = new List<Task> { };
        public Team? Team { get; set; } = null;
        public int? TeamId { get; set; }
        public Group? Group { get; set; }

        public int? GroupId { get; set; }
        [Range(0, 3)]
        public UserRoleEnum Role { get; set; } = UserRoleEnum.Member;
        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken> { };
        public List<UserChanges> UserChanges { get; set; } = new();
        public List<UserChanges> ChangedByUser { get; set; } = new();
        public List<LeaveRequest>? vacations { get; set; }
    }

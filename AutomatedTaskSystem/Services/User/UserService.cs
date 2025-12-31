using System.Security.Claims;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Dtos.UserTaskDtos;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.TokenService;
using Microsoft.AspNetCore.Mvc;
using static AutomatedTaskSystem.DTO.Responses;

namespace AutomatedTaskSystem.Services.UserService;

public class UserService : IUserService
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;

    public UserService(DataContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<ActionResult<BaseResponseService>> ArchiveUser(int id)
    {
        var user = await _context.Users.Where(u => u.Id == id && !u.Archived).FirstOrDefaultAsync();

        if (user is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = $"User of id:{id} is not found" }
            );

        user.Archived = true;

        await _context.SaveChangesAsync();

        return new NotFoundObjectResult(
            new BaseResponseService { Error = false, Message = $"User of id:{id} is now deleted" }
        );
    }

    public async Task<ActionResult<ResponseService<Responses.UserAddedDTO>>> CreateUser(
    Requests.UserDTO req
    )
    {
        Group group = null;
	        if (req.Role != UserRoleEnum.Owner)
	        {
	            if (req.GroupId is null)
	            {
	                return new NotFoundObjectResult(
	                    new BaseResponseService
	                    {
	                        Error = true,
	                        Message = "GroupId is required for non-owner roles"
	                    }
	                );
	            }

	            group = await _context.Groups
	                .Where(g => g.Id == req.GroupId.Value && !g.Archived)
	                .FirstOrDefaultAsync();
	            if (group is null)
	                return new NotFoundObjectResult(
	                    new BaseResponseService
	                    {
	                        Error = true,
	                        Message = $"Group of id:{req.GroupId} is not found"
	                    }
	                );
	        }


         if (req.Email != null && await CheckEmailExist(req.Email))
            return new NotFoundObjectResult(
                new BaseResponseService
                {
                    Error = true,
                    Message = $"Group of id:{req.GroupId} is not found"
                }
            );

         var newUser = new Models.User
        {
            Archived = false,
            Code = await GenerateCode(),
            Group = group,
            GroupId =group != null? group.Id:null,
            Name = req.Name,
            OnBoard = false,
            Role = req.Role,
            HR_code = req.HrCode,
            TeamleaderId = req.Role == UserRoleEnum.Member ? req.TeamleaderId : null,
            AccountType = req.AccountType,
            Email = req.Email,
            Annual_leave = req.Vacation.Annual,
            Annual_leave_MAX = req.Vacation.Annual,
            Sick_leave = req.Vacation.Sick,
            Emergency_leave_MAX = req.Vacation.Emergency,
            Title = req.Title,
            Phone = req.Phone,
            Permission = req.Permission,
             Permission_MAX = req.Permission_MAX,   
             WorkFromHome = req.WorkFromHome,
             WorkFromHome_MAX = req.WorkFromHome_MAX
         };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return new ResponseService<Responses.UserAddedDTO>
        {
            Data = new Responses.UserAddedDTO
            {
                Code = newUser.Code,
                User = new Responses.UserDTO
                {
                    Role = newUser.Role,
                    Group = req.Role == UserRoleEnum.Owner?  null:new IDName{ Id = newUser.GroupId??0, Name = newUser.Group.Name },
                    Id = newUser.Id,
                    Name = newUser.Name,
                    Email = newUser.Email,
                    HrCode = newUser.HR_code,
                    AccountType = newUser.AccountType,
                    Title = newUser.Title,
                    Phone = newUser.Phone,
                    OnBoard = newUser.OnBoard,
                    Archived = newUser.Archived,
                    TeamleaderId = newUser.TeamleaderId,
                    Annual_leave = newUser.Annual_leave,
                    Annual_leave_MAX = newUser.Annual_leave_MAX,
                    Permission = newUser.Permission,
                    Permission_MAX = newUser.Permission_MAX,
                    WorkFromHome = newUser.WorkFromHome,
                    WorkFromHome_MAX = newUser.WorkFromHome_MAX,
                    Vacation = new Responses.VacationDto
                    {
                        Annual = newUser.Annual_leave,
                        Sick = newUser.Sick_leave,
                        Emergency = newUser.Emergency_leave,
                        Annual_MAX = newUser.Annual_leave_MAX,
                        Emergency_MAX = newUser.Emergency_leave_MAX

                    }
                }
            },
            Error = false,
            Message = $"Create new User: id:{newUser.Id}"
        };
    }

    public async Task<ActionResult<ResponseService<Responses.UserDTO>>> EditUser(
     int id,
     Requests.UserDTO req
    )
    {
        // Get the current user's ID from the token
        var currentUserId = _tokenService.GetUserIdFromToken();
        var authUser = await _context.Users
            .Where(u => u.Id == int.Parse(currentUserId.Data) && !u.Archived)
            .FirstOrDefaultAsync();

        var user = await _context.Users
            .Where(u => u.Id == id && !u.Archived)
            .Include(u => u.Group)
            .Include(u => u.Tasks)
            .FirstOrDefaultAsync();

        if (user is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = $"User of id:{id} is not found" }
            );

	        Group group = null;
	        if (user.Role != UserRoleEnum.Owner)
	        {
	            if (req.GroupId is null)
	            {
	                return new NotFoundObjectResult(
	                    new BaseResponseService
	                    {
	                        Error = true,
	                        Message = "GroupId is required for non-owner roles"
	                    }
	                );
	            }

	            group = await _context.Groups
	                .Where(g => g.Id == req.GroupId.Value && !g.Archived)
	                .FirstOrDefaultAsync();
	            if (group is null)
	                return new NotFoundObjectResult(
	                    new BaseResponseService
	                    {
	                        Error = true,
	                        Message = $"Group of id:{req.GroupId} is not found"
	                    }
	                );
	        }

        // Create a list to track changes
        var changes = new List<string>();

	        if (user.Role != UserRoleEnum.Owner && group != null)
	        {
	            if (user.GroupId != req.GroupId)
	            {
	                changes.Add($"Group changed from '{user.Group?.Name}' (ID:{user.GroupId}) to '{group.Name}' (ID:{req.GroupId})");
	                user.Group = group;
	                user.GroupId = group.Id;
	            }
	        }
        
        if (user.Archived != req.Archived)
        {
            changes.Add($"Archive changed from {user.Archived} to {req.Archived}");
            user.Archived = req.Archived;
        }

        // Check and record each change
        if (user.Name != req.Name)
        {
            changes.Add($"Name changed from '{user.Name}' to '{req.Name}'");
            user.Name = req.Name;
        }


        if (user.Role != req.Role)
        {
            changes.Add($"Role changed from '{user.Role}' to '{req.Role}'");
            user.Role = req.Role;
        }

        if (user.AccountType != req.AccountType)
        {
            changes.Add($"AccountType changed from '{user.AccountType}' to '{req.AccountType}'");
            user.AccountType = req.AccountType;
        }

        if (user.TeamleaderId != req.TeamleaderId)
        {
            changes.Add($"TeamLeaderId changed from '{user.TeamleaderId}' to '{req.TeamleaderId}'");
            user.TeamleaderId = req.TeamleaderId;
        }

        if (user.Annual_leave != req.Vacation.Annual)
        {
            changes.Add($"Annual leave changed from {user.Annual_leave} to {req.Vacation.Annual}");
            user.Annual_leave = req.Vacation.Annual;
        }

        if (user.Sick_leave != req.Vacation.Sick)
        {
            changes.Add($"Sick leave changed from {user.Sick_leave} to {req.Vacation.Sick}");
            user.Sick_leave = req.Vacation.Sick;
        }

        if (user.Emergency_leave != req.Vacation.Emergency)
        {
            changes.Add($"Emergency leave changed from {user.Emergency_leave} to {req.Vacation.Emergency}");
            user.Emergency_leave = req.Vacation.Emergency;
        }

        if (user.Annual_leave_MAX != req.Vacation.Annual_MAX)
        {
            changes.Add($"Annual leave MAX changed from {user.Annual_leave_MAX} to {req.Vacation.Annual_MAX}");
            user.Annual_leave_MAX = req.Vacation.Annual_MAX;
        }

        if (user.Emergency_leave_MAX != req.Vacation.Emergency_MAX)
        {
            changes.Add($"Emergency leave MAX changed from {user.Emergency_leave_MAX} to {req.Vacation.Emergency_MAX}");
            user.Emergency_leave_MAX = req.Vacation.Emergency_MAX;
        }

        if (user.Permission_MAX != req.Permission_MAX)
        {
            changes.Add($"Permission_MAX changed from {user.Permission_MAX} to {req.Permission_MAX}");
            user.Permission_MAX = req.Permission_MAX;
        }

        if (user.Permission != req.Permission)
        {
            changes.Add($"Permission changed from {user.Permission} to {req.Permission}");
            user.Permission = req.Permission;
        }

        if (user.WorkFromHome != req.WorkFromHome)
        {
            changes.Add($"Work From Home changed from {user.WorkFromHome} to {req.WorkFromHome}");
            user.WorkFromHome = req.WorkFromHome;
        }

        if (user.WorkFromHome_MAX != req.WorkFromHome_MAX)
        {
            changes.Add($"WorkFromHome_MAX changed from {user.WorkFromHome_MAX} to {req.WorkFromHome_MAX}");
            user.WorkFromHome_MAX = req.WorkFromHome_MAX;
        }

        if (user.Email != req.Email)
        {
            changes.Add($"Email changed from '{user.Email}' to '{req.Email}'");
            user.Email = req.Email;
        }

        if (user.Phone != req.Phone)
        {
            changes.Add($"Phone changed from '{user.Phone}' to '{req.Phone}'");
            user.Phone = req.Phone;
        }

        if (user.Title != req.Title)
        {
            changes.Add($"Title changed from '{user.Title}' to '{req.Title}'");
            user.Title = req.Title;
        }

        if (user.HR_code != req.HrCode)
        {
            changes.Add($"HR code changed from '{user.HR_code}' to '{req.HrCode}'");
            user.HR_code = req.HrCode;
        }
        

        // Check if there are any changes to record
        if (changes.Any())
        {
            // Record the changes in the UserChanges table
            var userChange = new UserChanges
            {
                UserId = user.Id,
                ChangedByUserId = int.Parse(currentUserId.Data),
                ChangedByUserName = authUser?.Name ?? "System",
                Action = "Updated",
                Changes = string.Join("; ", changes),
                ChangedAt = DateTime.UtcNow
            };

            _context.UserChanges.Add(userChange);
        }

        await _context.SaveChangesAsync();

        var message = changes.Any()
            ? $"User of id:{user.Id} updated. Changes: {string.Join("; ", changes)}"
            : $"User of id:{user.Id} - no changes detected";

        return new OkObjectResult(new ResponseService<Responses.UserDTO>
        {
            Data = new Responses.UserDTO
            {
                Group = user.Group is not null
                    ? new IDName { Id = user.GroupId ?? 0, Name = user.Group.Name }
                    : null,
                Role = user.Role,
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Title = user.Title,
                HrCode = user.HR_code,
                AccountType = user.AccountType,
                TeamleaderId = user.TeamleaderId,
                Annual_leave = user.Annual_leave,
                Annual_leave_MAX = user.Annual_leave_MAX,
                Permission = user.Permission,
                Permission_MAX = user.Permission_MAX,
                WorkFromHome = user.WorkFromHome,
                WorkFromHome_MAX = user.WorkFromHome_MAX,
                Vacation = new Responses.VacationDto
                {
                    Annual = user.Annual_leave,
                    Sick = user.Sick_leave,
                    Emergency = user.Emergency_leave,
                    Annual_MAX = user.Annual_leave_MAX,
                    Emergency_MAX = user.Emergency_leave_MAX
                }
            },
            Error = false,
            Message = message
        });
    }

    public async Task<ActionResult<ResponseService<List<GetUserChangesDto>>>> GetUserChanges(int userId) { 
        return new ResponseService<List<GetUserChangesDto>>
        {
            Data = await _context.UserChanges
                .Where(uc => uc.UserId == userId)
                .Select(uc => new GetUserChangesDto
                {
                    Id = uc.Id,
                    UserId = uc.UserId,
                    ChangedByUserId = uc.ChangedByUserId,
                    ChangedByUserName = uc.ChangedByUserName,
                    Action = uc.Action,
                    Changes = uc.Changes,
                    ChangedAt = uc.ChangedAt
                })
                .ToListAsync(),
            Error = false,
            Message = $"User changes of id:{userId}"
        };
    }

    public async Task<ActionResult<ResponseService<Responses.UserDTO>>> GetUserById(int Id)
    {
        var user = _context.Users
            .Include(u => u.Group)
            .FirstOrDefault(u => u.Id == Id && !u.Archived);

        if (user is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = $"User of id:{Id} is not found" }
            );

        return new ResponseService<Responses.UserDTO>
        {
            Data = new Responses.UserDTO
            {
                Group = user.Group is not null
                    ? new IDName { Id = user.GroupId ?? 0, Name = user.Group.Name }
                    : null,

                GroupId = user.GroupId,
                Role = user.Role,
                Id = user.Id,
                Name = user.Name,
                Code = user.Code,
                HrCode = user.HR_code,
                AccountType = user.AccountType,
                Email = user.Email,
                Phone = user.Phone,
                Title = user.Title,
                IsAchived = user.Archived,
                Permission = user.Permission,
                Permission_MAX = user.Permission_MAX,
                WorkFromHome = user.WorkFromHome,
                WorkFromHome_MAX = user.WorkFromHome_MAX,
                Vacation = new Responses.VacationDto
                {
                    Annual = user.Annual_leave,
                    Sick = user.Sick_leave,
                    Emergency = user.Emergency_leave,
                    Annual_MAX = user.Annual_leave_MAX,
                    Emergency_MAX = user.Emergency_leave_MAX
                    
                },
                TeamleaderId = user.TeamleaderId
            },
            Error = false,
            Message = $"User of id:{user.Id}"
        };
    }

    public async Task<ActionResult<ResponseService<List<Responses.UserDTO>>>> GetUsers()
    {
        var users = await _context.Users
            .Where(u => !u.Archived)
            .Include(u => u.Group)
            .Include(u => u.Teamleader)
            .ToListAsync();

        return new ResponseService<List<Responses.UserDTO>>
        {
            Data = users
                .Select(u => new Responses.UserDTO
                {
                    Id = u.Id,
                    Archived = u.Archived,
                    Code = u.Code,
                    OnBoard = u.OnBoard,
                    Name = u.Name,
                    Group = u.Group != null ? new IDName
                    {
                        Name = u.Group.Name ?? "", // If Group.Name is null, use an empty string
                        Id = u.Group.Id
                    } : null,
                    AccountType = u.AccountType,
                    Annual_leave_MAX = u.Annual_leave_MAX,
                    Annual_leave = u.Annual_leave,
                    Sick_leave = u.Sick_leave,
                    Emergency_leave_MAX = u.Emergency_leave_MAX,
                    Emergency_leave = u.Emergency_leave,
                    Permission_MAX = u.Permission_MAX,
                    Permission = u.Permission,
                    HrCode = u.HR_code,
                    Email = u.Email,
                    
                    TeamleaderId = u.TeamleaderId,
                    Teamleader = u.Teamleader == null
                        ? null
                        : new Responses.UserDTO
                        {
                            Id = u.Teamleader.Id,
                            Name = u.Teamleader.Name
                        },
                    GroupId = u.GroupId,
                    Role = u.Role,
                    Vacation = new Responses.VacationDto
                    {
                        Annual = u.Annual_leave,
                        Sick = u.Sick_leave,
                        Emergency = u.Emergency_leave
                    }
                })
                .ToList(),
            Error = false,
            Message = "List of all users"
        };
    }


    private async Task<string> GenerateCode()
    {
        var code = "";
        Random random = new Random();
        var chars = new List<char>
        {
            'A',
            'B',
            'C',
            'D',
            'E',
            'F',
            'G',
            'H',
            'I',
            'J',
            'K',
            'L',
            'M',
            'N',
            'O',
            'P',
            'Q',
            'R',
            'S',
            'T',
            'U',
            'V',
            'W',
            'X',
            'Y',
            'Z',
            '0',
            '1',
            '2',
            '3',
            '4',
            '5',
            '6',
            '7',
            '8',
            '9'
        };

        for (int i = 0; i < 6; i++)
        {
            var index = random.Next(0, chars.Count);
            code += chars[index];
            chars.RemoveAt(index);
        }
        var alreadyExists = await _context.Users.AnyAsync(u => u.Code == code);
        if (alreadyExists)
            return await GenerateCode();

        return code;
    }

    private async Task<bool> CheckEmailExist(string email) => await _context.Users.AnyAsync(x => x.Email == email);



}

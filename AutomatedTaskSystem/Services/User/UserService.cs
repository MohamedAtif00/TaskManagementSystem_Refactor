using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;
using static AutomatedTaskSystem.DTO.Responses;

namespace AutomatedTaskSystem.Services.UserService;

public class UserService : IUserService
{
    private readonly DataContext _context;

    public UserService(DataContext context)
    {
        _context = context;
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
        var group = await _context.Groups
            .Where(g => g.Id == req.GroupId && !g.Archived)
            .FirstOrDefaultAsync();
        if (group is null)
            return new NotFoundObjectResult(
                new BaseResponseService
                {
                    Error = true,
                    Message = $"Group of id:{req.GroupId} is not found"
                }
            );


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
            GroupId = group.Id,
            Name = req.Name,
            OnBoard = false,
            Role = req.Role,
            HR_code = req.HrCode,
            TeamleaderId = req.Role == UserRoleEnum.Member ? req.Teamleader : null,
            AccountType = req.AccountType,
            Email = req.Email,
            Annual_leave_MAX = req.Vacation.Annual,
            Sick_leave = req.Vacation.Sick,
            Emergency_leave_MAX = req.Vacation.Emergency
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return new ResponseService<Responses.UserAddedDTO>
        {
            Data = new Responses.UserAddedDTO
            {
                Code = newUser.Code,
                User =
                {
                    Role = newUser.Role,
                    Group = { Id = newUser.GroupId, Name = newUser.Group.Name },
                    Id = newUser.Id,
                    Name = newUser.Name,

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
        var user = await _context.Users
            .Where(u => u.Id == id && !u.Archived)
            .Include(u => u.Group)
            .Include(u => u.Tasks)
            .FirstOrDefaultAsync();

        if (user is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = $"User of id:{id} is not found" }
            );

        var group = await _context.Groups
            .Where(g => g.Id == req.GroupId && !g.Archived)
            .FirstOrDefaultAsync();
        if (group is null)
            return new NotFoundObjectResult(
                new BaseResponseService
                {
                    Error = true,
                    Message = $"Group of id:{req.GroupId} is not found"
                }
            );

        user.Name = req.Name;
        user.Group = group;
        user.GroupId = group.Id;
        user.Role = req.Role;
        user.AccountType = req.AccountType;
        user.Annual_leave = req.Vacation.Annual;
        user.Sick_leave = req.Vacation.Sick;
        user.Emergency_leave = req.Vacation.Emergency;
        user.Code = await GenerateCode();


        await _context.SaveChangesAsync();

        return new ResponseService<Responses.UserDTO>
        {
            Data = new Responses.UserDTO
            {
                Group = { Id = user.GroupId, Name = user.Group.Name },
                Role = user.Role,
                Id = user.Id,
                Name = user.Name
            },
            Error = false,
            Message = $"User of id:{user.Id}"
        };
    }

    public async Task<ActionResult<ResponseService<Responses.UserDTO>>> GetUserById(int Id)
    {
        var user = await _context.Users
            .Where(u => u.Id == Id && !u.Archived)
            .Include(u => u.Group)
            .FirstOrDefaultAsync();

        if (user is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = $"User of id:{Id} is not found" }
            );

        return new ResponseService<Responses.UserDTO>
        {
            Data = new Responses.UserDTO
            {
                Group = { Id = user.GroupId, Name = user.Group.Name },
                Role = user.Role,
                Id = user.Id,
                Name = user.Name,
                Code = user.Code,
                HrCode = user.HR_code,
                AccountType = user.AccountType,
                Email = user.Email,
                Vacation = new Responses.VacationDto
                {
                    Annual = user.Annual_leave,
                    Sick = user.Sick_leave,
                    Emergency = user.Emergency_leave
                }
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

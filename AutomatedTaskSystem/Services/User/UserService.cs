using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

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
        string Name,
        int GroupId,
        int RoleId
    )
    {
        var group = await _context.Groups
            .Where(g => g.Id == GroupId && !g.Archived)
            .FirstOrDefaultAsync();
        if (group is null)
            return new NotFoundObjectResult(
                new BaseResponseService
                {
                    Error = true,
                    Message = $"Group of id:{GroupId} is not found"
                }
            );

        var role = await _context.Roles.Where(r => r.Id == RoleId).FirstOrDefaultAsync();
        if (role is null)
            return new NotFoundObjectResult(
                new BaseResponseService
                {
                    Error = true,
                    Message = $"Role of id:{RoleId} is not found"
                }
            );

        var newUser = new User
        {
            Archived = false,
            Code = await GenerateCode(),
            Group = group,
            GroupId = group.Id,
            Name = Name,
            OnBoard = false,
            Role = role,
            RoleId = role.Id
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
                    Role = { Id = newUser.RoleId, Name = newUser.Role.Name },
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
        string Name,
        int GroupId,
        int RoleId
    )
    {
        var user = await _context.Users
            .Where(u => u.Id == id && !u.Archived)
            .Include(u => u.Group)
            .Include(u => u.Tasks)
            .Include(u => u.Role)
            .FirstOrDefaultAsync();

        if (user is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = $"User of id:{id} is not found" }
            );

        var group = await _context.Groups
            .Where(g => g.Id == GroupId && !g.Archived)
            .FirstOrDefaultAsync();
        if (group is null)
            return new NotFoundObjectResult(
                new BaseResponseService
                {
                    Error = true,
                    Message = $"Group of id:{GroupId} is not found"
                }
            );

        var role = await _context.Roles.Where(r => r.Id == RoleId).FirstOrDefaultAsync();
        if (role is null)
            return new NotFoundObjectResult(
                new BaseResponseService
                {
                    Error = true,
                    Message = $"Role of id:{RoleId} is not found"
                }
            );

        user.Name = Name;
        user.Group = group;
        user.GroupId = group.Id;
        user.Role = role;
        user.RoleId = role.Id;

        await _context.SaveChangesAsync();

        return new ResponseService<Responses.UserDTO>
        {
            Data = new Responses.UserDTO
            {
                Group = { Id = user.GroupId, Name = user.Group.Name },
                Role = { Id = user.RoleId, Name = user.Role.Name },
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
            .Include(u => u.Role)
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
                Role = { Id = user.RoleId, Name = user.Role.Name },
                Id = user.Id,
                Name = user.Name
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
            .Include(u => u.Role)
            .ToListAsync();

        return new ResponseService<List<Responses.UserDTO>>
        {
            Data = users
                .Select(
                    u =>
                        new Responses.UserDTO
                        {
                            Group = { Id = u.GroupId, Name = u.Group.Name },
                            Role = { Id = u.RoleId, Name = u.Role.Name },
                            Id = u.Id,
                            Name = u.Name
                        }
                )
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
}

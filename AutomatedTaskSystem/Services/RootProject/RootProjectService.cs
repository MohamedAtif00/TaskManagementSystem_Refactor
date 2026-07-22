using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AutomatedTaskSystem.Services.RootProjectService;

public interface IFolderService
{
    Task<ResponseService<List<FolderDto>>> GetRootsAsync();
    Task<ResponseService<FolderDto>> GetAsync(int folderId);
    Task<ResponseService<FolderDto>> CreateAsync(string name, int? parentFolderId, string? description, List<string>? levelNames);
    Task<ResponseService<FolderDto>> UpdateAsync(int folderId, string name, string? description, List<string>? levelNames);
    Task<ResponseService<List<FolderDto>>> GetChildrenAsync(int folderId);
    Task<ResponseService<List<FolderDto>>> GetSubjectsFoldersAsync();
}

public class FolderService(DataContext context) : IFolderService
{
    public async Task<ResponseService<List<FolderDto>>> GetRootsAsync()
    {
        var roots = await context.Folders
            .AsNoTracking()
            .Include(f => f.Project)
            .Where(f => f.ParentFolderId == null)
            .OrderBy(f => f.Name)
            .Select(f => ToFolderDto(f, 0, f.Name))
            .ToListAsync();

        return new ResponseService<List<FolderDto>> { Data = roots, Error = false, Message = "Root folders" };
    }

    public async Task<ResponseService<FolderDto>> GetAsync(int folderId)
    {
        var folder = await context.Folders
            .AsNoTracking()
            .Include(f => f.Project)
            .FirstOrDefaultAsync(f => f.Id == folderId);
        if (folder is null)
            return new ResponseService<FolderDto> { Error = true, Message = "Folder not found" };

        var level = await ComputeLevelAsync(folder.Id);
        var path = await ComputePathAsync(folder.Id);
        return new ResponseService<FolderDto>
        {
            Data = ToFolderDto(folder, level, path),
            Error = false,
            Message = "Folder",
        };
    }

    public async Task<ResponseService<FolderDto>> CreateAsync(string name, int? parentFolderId, string? description, List<string>? levelNames)
    {
        int projectId;
        Models.Project? projectForRoot = null;
        if (parentFolderId.HasValue)
        {
            var parent = await context.Folders.AsNoTracking().FirstOrDefaultAsync(f => f.Id == parentFolderId.Value);
            if (parent is null)
                return new ResponseService<FolderDto> { Error = true, Message = "Parent folder not found" };
            projectId = parent.ProjectId;
        }
        else
        {
            var project = new Models.Project
            {
                Name = name,
                Description = description?.Trim(),
                LevelNamesJson = SerializeLevelNames(levelNames) ?? CurriculumHierarchy.DefaultLevelNamesJson,
            };
            context.Projects.Add(project);
            await context.SaveChangesAsync();
            projectId = project.Id;
            projectForRoot = project;
        }

        var entity = new Folder { Name = name, ParentFolderId = parentFolderId, ProjectId = projectId };
        if (projectForRoot is not null)
            entity.Project = projectForRoot;
        context.Folders.Add(entity);
        await context.SaveChangesAsync();

        var level = await ComputeLevelAsync(entity.Id);
        var path = await ComputePathAsync(entity.Id);
        return new ResponseService<FolderDto>
        {
            Data = ToFolderDto(entity, level, path),
            Error = false,
            Message = "Folder created",
        };
    }

    public async Task<ResponseService<FolderDto>> UpdateAsync(int folderId, string name, string? description, List<string>? levelNames)
    {
        var folder = await context.Folders.FirstOrDefaultAsync(f => f.Id == folderId);
        if (folder is null)
            return new ResponseService<FolderDto> { Error = true, Message = "Folder not found" };

        folder.Name = name;
        if (folder.ParentFolderId is null)
        {
            var project = await context.Projects.FirstOrDefaultAsync(p => p.Id == folder.ProjectId);
            if (project is not null)
            {
                project.Name = name;
                if (description is not null)
                    project.Description = description.Trim();
                if (levelNames is not null)
                    project.LevelNamesJson = SerializeLevelNames(levelNames);
            }
        }
        await context.SaveChangesAsync();
        var level = await ComputeLevelAsync(folder.Id);
        var path = await ComputePathAsync(folder.Id);

        return new ResponseService<FolderDto>
        {
            Data = ToFolderDto(folder, level, path),
            Error = false,
            Message = "Folder updated",
        };
    }

    public async Task<ResponseService<List<FolderDto>>> GetChildrenAsync(int folderId)
    {
        var exists = await context.Folders.AnyAsync(f => f.Id == folderId);
        if (!exists)
            return new ResponseService<List<FolderDto>> { Error = true, Message = "Folder not found" };

        var parentLevel = await ComputeLevelAsync(folderId);
        var parentPath = await ComputePathAsync(folderId);
        var children = await context.Folders
            .AsNoTracking()
            .Where(f => f.ParentFolderId == folderId)
            .OrderBy(f => f.Name)
            .ToListAsync();

        return new ResponseService<List<FolderDto>>
        {
            Data = children.Select(c => ToFolderDto(c, parentLevel + 1, $"{parentPath}/{c.Name}")).ToList(),
            Error = false,
            Message = "Children",
        };
    }

    public async Task<ResponseService<List<FolderDto>>> GetSubjectsFoldersAsync()
    {
        var folderIds = await context.Subjects
            .Where(s => !s.Archived)
            .Select(s => s.FolderId)
            .Distinct()
            .ToListAsync();
        var folders = await context.Folders.Where(f => folderIds.Contains(f.Id)).OrderBy(f => f.Name).ToListAsync();
        var result = new List<FolderDto>(folders.Count);
        foreach (var folder in folders)
        {
            result.Add(
                ToFolderDto(folder, await ComputeLevelAsync(folder.Id), await ComputePathAsync(folder.Id))
            );
        }

        return new ResponseService<List<FolderDto>>
        {
            Data = result,
            Error = false,
            Message = "Folders with subjects",
        };
    }

    private static FolderDto ToFolderDto(Folder folder, int level, string path) =>
        new()
        {
            Id = folder.Id,
            Name = folder.Name,
            ProjectId = folder.ProjectId,
            ParentFolderId = folder.ParentFolderId,
            Level = level,
            Path = path,
            LevelNames = folder.ParentFolderId is null
                ? DeserializeLevelNames(folder.Project?.LevelNamesJson)
                : new List<string>(),
        };

    private static string? SerializeLevelNames(List<string>? levelNames)
    {
        var cleaned = levelNames?
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (cleaned is { Count: > 0 })
            return JsonSerializer.Serialize(cleaned);

        return CurriculumHierarchy.DefaultLevelNamesJson;
    }

    private static List<string> DeserializeLevelNames(string? levelNamesJson)
    {
        if (string.IsNullOrWhiteSpace(levelNamesJson))
            return CurriculumHierarchy.FolderLevelNames.ToList();

        try
        {
            var parsed = JsonSerializer.Deserialize<List<string>>(levelNamesJson);
            return parsed is { Count: > 0 }
                ? parsed
                : CurriculumHierarchy.FolderLevelNames.ToList();
        }
        catch
        {
            return CurriculumHierarchy.FolderLevelNames.ToList();
        }
    }

    private async Task<int> ComputeLevelAsync(int folderId)
    {
        var level = 0;
        var current = await context.Folders.AsNoTracking().FirstOrDefaultAsync(f => f.Id == folderId);
        while (current?.ParentFolderId is int parentId)
        {
            level++;
            current = await context.Folders.AsNoTracking().FirstOrDefaultAsync(f => f.Id == parentId);
        }

        return level;
    }

    private async Task<string> ComputePathAsync(int folderId)
    {
        var parts = new List<string>();
        var current = await context.Folders.AsNoTracking().FirstOrDefaultAsync(f => f.Id == folderId);
        while (current is not null)
        {
            parts.Add(current.Name);
            current = current.ParentFolderId.HasValue
                ? await context.Folders.AsNoTracking().FirstOrDefaultAsync(f => f.Id == current.ParentFolderId.Value)
                : null;
        }

        parts.Reverse();
        return string.Join("/", parts);
    }
}

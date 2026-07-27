using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.UserService;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AutomatedTaskSystem.Seeding
{
    public class DataSeeder
    {
        private readonly IUserService _userService;
        private readonly DataContext _context;

        public const string DefaultProjectName = "Selah Eltelmeez";
        public const string DefaultYearName = "2026/2027";

        public DataSeeder(IUserService userService, DataContext context)
        {
            _userService = userService;
            _context = context;
        }

        public async System.Threading.Tasks.Task Seed()
        {
            var isEmpty = !_context.Users.Any(x => x.Role == Models.Enums.UserRole.UserRoleEnum.Owner);
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
                await _userService.CreateUser(owner);
            }

            await SeedDefaultCurriculumHierarchyAsync();
            await SeedEmptyLOSprintAsync();
        }

        private async System.Threading.Tasks.Task SeedDefaultCurriculumHierarchyAsync()
        {
            var year = await EnsureYearAsync(DefaultYearName);
            var project = await EnsureProjectAsync(DefaultProjectName, year.Id);
            var assigned = await AssignOrphanSubjectsAsync(project.Id);

            if (assigned > 0)
                Console.WriteLine($"[DataSeeder] Assigned {assigned} subject(s) into term/subject-group folders.");
        }

        private async Task<AcademicYear> EnsureYearAsync(string name)
        {
            var year = await _context.AcademicYears.FirstOrDefaultAsync(y => y.Name == name);
            if (year is not null) return year;
            year = new AcademicYear { Name = name };
            _context.AcademicYears.Add(year);
            await _context.SaveChangesAsync();
            return year;
        }

        private async Task<CurriculumProject> EnsureProjectAsync(string name, int yearId)
        {
            var project = await _context.CurriculumProjects.FirstOrDefaultAsync(p => p.YearId == yearId && p.Name == name);
            if (project is not null) return project;
            project = new CurriculumProject { YearId = yearId, Name = name };
            _context.CurriculumProjects.Add(project);
            await _context.SaveChangesAsync();
            return project;
        }

        private async Task<CurriculumTerm> EnsureTermAsync(string name, int projectId)
        {
            var term = await _context.CurriculumTerms.FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Name == name);
            if (term is not null) return term;
            term = new CurriculumTerm { ProjectId = projectId, Name = name };
            _context.CurriculumTerms.Add(term);
            await _context.SaveChangesAsync();
            return term;
        }

        private async Task<SubjectGroup> EnsureSubjectGroupAsync(string name, int termId)
        {
            var group = await _context.SubjectGroups.FirstOrDefaultAsync(g => g.TermId == termId && g.Name == name);
            if (group is not null) return group;
            group = new SubjectGroup { TermId = termId, Name = name };
            _context.SubjectGroups.Add(group);
            await _context.SaveChangesAsync();
            return group;
        }

        private async Task<int> AssignOrphanSubjectsAsync(int projectId)
        {
            var validGroupIds = await _context.SubjectGroups.AsNoTracking().Select(g => g.Id).ToHashSetAsync();
            var subjects = await _context.Subjects
                .Where(s => !s.Archived && !validGroupIds.Contains(s.SubjectGroupId))
                .ToListAsync();

            var changed = 0;
            foreach (var subject in subjects)
            {
                var termNumber = ResolveTermNumber(subject.Name);
                var term = await EnsureTermAsync($"Term {termNumber}", projectId);
                var groupName = ResolveSubjectGroupName(subject.Name);
                var group = await EnsureSubjectGroupAsync(groupName, term.Id);
                if (subject.SubjectGroupId == group.Id) continue;
                subject.SubjectGroupId = group.Id;
                changed++;
            }

            if (changed > 0)
                await _context.SaveChangesAsync();
            return changed;
        }

        private static int ResolveTermNumber(string subjectName)
        {
            var lower = subjectName.ToLowerInvariant();
            var termMatch = Regex.Match(lower, @"(?:^|_)([12])[ae](?:_|$)");
            if (termMatch.Success && int.TryParse(termMatch.Groups[1].Value, out var termFromSuffix))
                return termFromSuffix;
            var literalMatch = Regex.Match(lower, @"term[_\s-]?([12])");
            if (literalMatch.Success && int.TryParse(literalMatch.Groups[1].Value, out var termFromLiteral))
                return termFromLiteral;
            return 1;
        }

        private static string ResolveSubjectGroupName(string subjectName)
        {
            var lower = subjectName.ToLowerInvariant().Trim();
            var prefixMatch = Regex.Match(lower, @"^([^_]+)_");
            if (!prefixMatch.Success) return "Other";
            return prefixMatch.Groups[1].Value switch
            {
                "ara" => "Arabic",
                "eng" => "English",
                "mth" => "Math",
                "sci" => "Science",
                "soc" => "Social Studies",
                "ict" => "ICT",
                "mul" => "Multimedia",
                "rel" => "Religion",
                "tsk" => "Tokkatsu",
                _ => "Other"
            };
        }

        private async System.Threading.Tasks.Task SeedEmptyLOSprintAsync()
        {
            const string demoSprintName = "Demo Sprint - No Learning Objectives";
            var existingSprint = await _context.Sprints.FirstOrDefaultAsync(s => s.Name == demoSprintName);
            if (existingSprint is not null) return;

            var demoSprint = new Sprint
            {
                Name = demoSprintName,
                Description = "Demo sprint for empty state UI testing.",
                StartDate = DateTime.Now.AddDays(-7),
                EndDate = DateTime.Now.AddDays(14),
                IsArchived = false,
                SprintLearningObjectives = new List<SprintLearningObjective>()
            };
            _context.Sprints.Add(demoSprint);
            await _context.SaveChangesAsync();
        }
    }
}

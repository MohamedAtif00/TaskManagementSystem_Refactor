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



        public const string RootFolderName = "Selah Eltelmeez";
        public const string FallbackYearLabel = "2026/2027";



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



            await SeedDefaultFolderHierarchyAsync();



            // Seed demo sprint with no learning objectives for testing empty state UI

            await SeedEmptyLOSprintAsync();

        }



        /// <summary>

        /// Ensures Selah Eltelmeez -> 2026/2027 -> Term -> Subject
        /// and assigns all subjects by parsed subject/term.

        /// </summary>

        private async System.Threading.Tasks.Task SeedDefaultFolderHierarchyAsync()

        {
            var root = await EnsureFolderAsync(RootFolderName, null);
            var year = await EnsureFolderAsync(FallbackYearLabel, root.Id);
            var assigned = await AssignSubjectsByGradeAndTermAsync(year.Id);

            if (assigned > 0)

            {

                Console.WriteLine(

                    $"[DataSeeder] Assigned {assigned} subject(s) into term/subject folders.");

            }

        }

        private async Task<Folder> EnsureFolderAsync(string name, int? parentFolderId)

        {
            var folder = await _context.Folders.FirstOrDefaultAsync(f =>
                f.Name == name && f.ParentFolderId == parentFolderId);
            if (folder is not null)
            {
                if (folder.ProjectId == 0)
                {
                    var fallbackProject = new Project
                    {
                        Name = parentFolderId is null ? name : RootFolderName,
                        Description = "Auto-created during seeding",
                    };
                    _context.Projects.Add(fallbackProject);
                    await _context.SaveChangesAsync();
                    folder.ProjectId = fallbackProject.Id;
                    await _context.SaveChangesAsync();
                }
                return folder;
            }

            int projectId;
            if (parentFolderId is null)
            {
                var project = new Project { Name = name, Description = "Auto-created during seeding" };
                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
                projectId = project.Id;
            }
            else
            {
                var parent = await _context.Folders.AsNoTracking().FirstOrDefaultAsync(f => f.Id == parentFolderId.Value);
                if (parent is null)
                    throw new InvalidOperationException($"Parent folder {parentFolderId.Value} was not found during seeding.");
                projectId = parent.ProjectId;
            }

            folder = new Folder { Name = name, ParentFolderId = parentFolderId, ProjectId = projectId };
            _context.Folders.Add(folder);
            await _context.SaveChangesAsync();
            return folder;
        }

        private async Task<int> AssignSubjectsByGradeAndTermAsync(int yearFolderId)

        {
            var subjects = await _context.Subjects
                .Where(s => !s.Archived)
                .ToListAsync();

            var changed = 0;
            foreach (var subject in subjects)
            {
                var termNumber = ResolveTermNumber(subject.Name);
                var termFolder = await EnsureFolderAsync($"Term {termNumber}", yearFolderId);

                var subjectFolderName = ResolveSubjectFolderName(subject.Name);
                var subjectFolder = await EnsureFolderAsync(subjectFolderName, termFolder.Id);

                if (subject.FolderId == subjectFolder.Id)
                    continue;

                subject.FolderId = subjectFolder.Id;
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

        private static string ResolveSubjectFolderName(string subjectName)
        {
            var lower = subjectName.ToLowerInvariant().Trim();
            var prefixMatch = Regex.Match(lower, @"^([^_]+)_");
            if (!prefixMatch.Success)
                return "Other";

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



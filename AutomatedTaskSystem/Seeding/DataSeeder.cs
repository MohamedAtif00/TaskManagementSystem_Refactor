using AutomatedTaskSystem.Helper;
using AutomatedTaskSystem.Data;

using AutomatedTaskSystem.DTO;

using AutomatedTaskSystem.Models;

using AutomatedTaskSystem.Services.UserService;

using Microsoft.EntityFrameworkCore;



namespace AutomatedTaskSystem.Seeding

{

    public class DataSeeder

    {

        private readonly IUserService _userService;

        private readonly DataContext _context;



        public const string DefaultRootProjectName = "Selah Eltelmeez";

        public const string DefaultYearLabel = "2026/2027";



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



            await SeedDefaultProjectHierarchyAsync();



            // Seed demo sprint with no learning objectives for testing empty state UI

            await SeedEmptyLOSprintAsync();

        }



        /// <summary>

        /// Ensures Selah Eltelmeez → 2026/2027 → Term 1 &amp; Term 2, then assigns every subject

        /// (active, on hold, closed, reopened — not archived) to the term implied by its code suffix

        /// (e.g. <c>ara_1k_1a</c> → Term 1, <c>ara_3r_2e</c> → Term 2).

        /// </summary>

        private async System.Threading.Tasks.Task SeedDefaultProjectHierarchyAsync()

        {

            var root = await EnsureRootProjectAsync(DefaultRootProjectName);

            var year = await EnsureProjectYearAsync(root.Id, DefaultYearLabel);

            var terms = await EnsureDefaultTermsAsync(year);



            var assigned = await ClassifySubjectsIntoTermsAsync(terms);

            if (assigned > 0)

            {

                Console.WriteLine(

                    $"[DataSeeder] Classified {assigned} subject(s) into terms by name suffix (1a/1e → Term 1, 2a/2e → Term 2).");

            }

        }



        private async Task<RootProject> EnsureRootProjectAsync(string projectName)

        {

            var root = await _context.RootProjects.FirstOrDefaultAsync(r => r.Name == projectName);

            if (root is not null)

                return root;



            root = new RootProject { Name = projectName, Description = "" };

            _context.RootProjects.Add(root);

            await _context.SaveChangesAsync();

            return root;

        }



        private async Task<ProjectYear> EnsureProjectYearAsync(int rootProjectId, string yearLabel)

        {

            var year = await _context.ProjectYears.FirstOrDefaultAsync(y =>

                y.RootProjectId == rootProjectId && y.Label == yearLabel);

            if (year is not null)

                return year;



            year = new ProjectYear { RootProjectId = rootProjectId, Label = yearLabel };

            _context.ProjectYears.Add(year);

            await _context.SaveChangesAsync();

            return year;

        }



        private async Task<Dictionary<int, ProjectTerm>> EnsureDefaultTermsAsync(ProjectYear year)

        {

            await NormalizeLegacyTermNamesAsync(year);



            var term1 = await EnsureTermAsync(year, SubjectTermClassifier.TermDisplayName(1), order: 0);

            var term2 = await EnsureTermAsync(year, SubjectTermClassifier.TermDisplayName(2), order: 1);



            return new Dictionary<int, ProjectTerm>

            {

                [1] = term1,

                [2] = term2,

            };

        }



        /// <summary>Renames legacy <c>Term2</c> / <c>Term1</c> rows to <c>Term 2</c> / <c>Term 1</c>.</summary>

        private async System.Threading.Tasks.Task NormalizeLegacyTermNamesAsync(ProjectYear year)

        {

            var legacyNames = new Dictionary<string, string>

            {

                ["Term1"] = "Term 1",

                ["Term2"] = "Term 2",

            };



            var changed = false;

            foreach (var (from, to) in legacyNames)

            {

                var legacy = await _context.ProjectTerms.FirstOrDefaultAsync(t =>

                    t.ProjectYearId == year.Id && t.Name == from);

                if (legacy is null)

                    continue;



                var canonical = await _context.ProjectTerms.FirstOrDefaultAsync(t =>

                    t.ProjectYearId == year.Id && t.Name == to);

                if (canonical is not null && canonical.Id != legacy.Id)

                {

                    var subjectsOnLegacy = await _context.Subjects

                        .Where(s => s.TermId == legacy.Id)

                        .ToListAsync();

                    foreach (var subject in subjectsOnLegacy)

                        subject.TermId = canonical.Id;



                    _context.ProjectTerms.Remove(legacy);

                    changed = true;

                }

                else

                {

                    legacy.Name = to;

                    legacy.Order = to == "Term 1" ? 0 : 1;

                    changed = true;

                }

            }



            if (changed)

                await _context.SaveChangesAsync();

        }



        private async Task<ProjectTerm> EnsureTermAsync(ProjectYear year, string name, int order)

        {

            var term = await _context.ProjectTerms.FirstOrDefaultAsync(t =>

                t.ProjectYearId == year.Id && t.Name == name);

            if (term is not null)

                return term;



            term = new ProjectTerm { ProjectYearId = year.Id, Name = name, Order = order };

            _context.ProjectTerms.Add(term);

            await _context.SaveChangesAsync();

            return term;

        }



        /// <summary>

        /// Assigns all non-archived subjects to Term 1 or Term 2 based on name; includes hold/closed/reopened.

        /// </summary>

        private async Task<int> ClassifySubjectsIntoTermsAsync(IReadOnlyDictionary<int, ProjectTerm> termsByNumber)

        {

            var subjects = await _context.Subjects

                .Where(s => !s.Archived)

                .ToListAsync();



            var changed = 0;

            var unclassified = 0;



            foreach (var subject in subjects)

            {

                var parsed = SubjectTermClassifier.TryParseTermNumberFromSubjectName(subject.Name);

                var termNumber = SubjectTermClassifier.ResolveTermNumberFromSubjectName(subject.Name);

                var targetTerm = termsByNumber[termNumber];



                if (parsed is null)

                    unclassified++;



                if (subject.TermId == targetTerm.Id)

                    continue;



                subject.TermId = targetTerm.Id;

                changed++;

            }



            if (changed > 0)

                await _context.SaveChangesAsync();



            if (unclassified > 0)

            {

                Console.WriteLine(

                    $"[DataSeeder] {unclassified} subject(s) without a term suffix (e.g. *_1a) were assigned to Term 1.");

            }



            return changed;

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



namespace AutomatedTaskSystem.DTO
{
    public static partial class Responses
    {
        public class ProjectDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public IDName Year { get; set; } = new IDName { };
            public bool Term { get; set; }
        }

        public class DetailedProjectDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public List<ProjectUnitDTO> Units { get; set; } = new List<ProjectUnitDTO> { };
        }

        public class ProjectUnitDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public List<ProjectLessonDTO> Lessons { get; set; } = new List<ProjectLessonDTO> { };
        }

        public class ProjectLessonDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public List<LearningObjectiveDTO> LearningObjectives { get; set; } =
                new List<LearningObjectiveDTO> { };
        }

        public class LearningObjectiveDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Tag { get; set; } = "";
            public string Environment { get; set; } = "";
            public string Template { get; set; } = "";
            public IDName Schema { get; set; } = new IDName { };
        }

        public class User
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Group { get; set; } = "";
        }

        public class LOUsersDTO
        {
            public List<User> Assigned { get; set; } = new List<User> { };
            public List<User> Unassigned { get; set; } = new List<User> { };
        }

        public class CommentDTO
        {
            public int Id { get; set; }
            public string Content { get; set; } = "";
            public DateTime Timestamp { get; set; } = DateTime.Now;
            public IDName User { get; set; } = new IDName { };
        }
    }

    public static partial class Requests
    {
        public class LearningObjectiveDTO
        {
            public string Name { get; set; } = "";
            public string Tag { get; set; } = "";
            public string Template { get; set; } = "";
            public string Environment { get; set; } = "";
            public int SchemaId { get; set; }
        }

        public class NameDTO
        {
            public string Name { get; set; } = "";
        }

        public class ProjectDTO
        {
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public int YearId { get; set; }
            public bool Term { get; set; }
        }

        public class LOAssignDTO
        {
            public List<int> UserIds { get; set; } = new List<int> { };
        }

        public class CommentDTO
        {
            public string Comment { get; set; } = "";
        }
    }
}

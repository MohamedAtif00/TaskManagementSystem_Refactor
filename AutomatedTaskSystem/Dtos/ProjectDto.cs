using AutomatedTaskSystem.Models.Enums.ProjectStatus;
using AutomatedTaskSystem.Models.Enums.TaskStatus;

namespace AutomatedTaskSystem.DTO
{
    public static partial class Responses
    {
        public class SubjectDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public int FolderId { get; set; }
            public string FolderPath { get; set; } = "";
            public List<string> LevelNames { get; set; } = new();
            public ProjectStatusEnum Status { get; set; } = ProjectStatusEnum.Active;
            public int? Count { get; set; }
            public int? ProgressPercent { get; set; }
        }

        public class DetailedProjectDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public List<ProjectUnitDTO> Units { get; set; } = new List<ProjectUnitDTO> { };
            public ProjectStatusEnum Status { get; set; } = ProjectStatusEnum.Active;
            public int FolderId { get; set; }
            public int YearId { get; set; }
            public string FolderPath { get; set; } = "";
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

        public class SubjectCopyLineageNodeStatusDTO
        {
            public int NodeId { get; set; }
            public string NodeName { get; set; } = "";
            public int Order { get; set; }
            public bool IsComplete { get; set; }
            public string Status { get; set; } = "";
        }

        public class SubjectCopyLineageSchemaNodesDTO
        {
            public int SchemaId { get; set; }
            public string SchemaName { get; set; } = "";
            public List<SubjectCopyLineageNodeStatusDTO> Nodes { get; set; } = new();
        }

        public class SubjectCopyLineageLoTaskDTO
        {
            public int? TaskId { get; set; }
            public string NodeName { get; set; } = "";
            public string StepName { get; set; } = "";
            public int Status { get; set; }
            public bool IsComplete { get; set; }
            public bool HasTask { get; set; }
            public int Order { get; set; }
        }

        public class SubjectCopyLineageLoDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Title { get; set; } = "";
            public string Stage { get; set; } = "";
            public int ProgressPercent { get; set; }
            public List<SubjectCopyLineageLoTaskDTO> Tasks { get; set; } = new();
        }

        public class SubjectCopyLineageNodeDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public ProjectStatusEnum Status { get; set; } = ProjectStatusEnum.Active;
            public int? ProgressPercent { get; set; }
            public List<SubjectCopyLineageLoDTO> LearningObjectives { get; set; } = new();
            public List<SubjectCopyLineageSchemaNodesDTO> SchemaNodes { get; set; } = new();
        }

        public class SubjectCopyLineageChainDTO
        {
            public List<SubjectCopyLineageNodeDTO> Subjects { get; set; } = new();
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

        public class SubjectWriteDTO
        {
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public int FolderId { get; set; }
        }

        public class LOAssignDTO
        {
            public List<int> UserIds { get; set; } = new List<int> { };
        }

        public class CommentDTO
        {
            public string Comment { get; set; } = "";
        }

        public class CopySubjectRequest
        {
            public string Name { get; set; } = "";
            public List<LoSchemaOverrideGroup> SchemaOverrides { get; set; } = new();
        }

        public class LoSchemaOverrideGroup
        {
            public int SchemaId { get; set; }
            public List<int> SourceLearningObjectiveIds { get; set; } = new();
        }
    }
}

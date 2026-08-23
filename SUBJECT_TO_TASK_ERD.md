# Subject → Task Hierarchy ERD

Entity relationship diagram for the curriculum chain from **Subject** down to **Task**, based on the active models in `AutomatedTaskSystem/Models` and `DataContext`.

## Hierarchy chain

```
Project
  └── Folder (self-referencing tree via ParentFolderId)
        └── Subject
              └── Unit
                    └── Lesson
                          └── LearningObjective
                                └── Task
```

All solid parent → child links are **1:N**.

| Level | Entity | Table | Parent FK | Cardinality |
|------:|--------|-------|-----------|-------------|
| 0 | Project | `FolderProjects` | — | 1 Project → N Folders |
| 1 | Folder | `Folders` | `ProjectId`, `ParentFolderId?` | Folder → N children / N Subjects |
| 2 | Subject | `Subjects` | `FolderId` | 1 Subject → N Units |
| 3 | Unit | `Units` | `SubjectId` | 1 Unit → N Lessons |
| 4 | Lesson | `Lessons` | `UnitId` | 1 Lesson → N LearningObjectives |
| 5 | LearningObjective | `LearningObjectives` | `LessonId` | 1 LO → N Tasks |
| 6 | Task | `Tasks` | `LearningObjectiveId` | — |

## ERD (primary hierarchy)

```mermaid
erDiagram
    Project ||--o{ Folder : "ProjectId"
    Folder ||--o{ Folder : "ParentFolderId"
    Folder ||--o{ Subject : "FolderId"
    Subject ||--o{ Unit : "SubjectId"
    Unit ||--o{ Lesson : "UnitId"
    Lesson ||--o{ LearningObjective : "LessonId"
    LearningObjective ||--o{ Task : "LearningObjectiveId"

    Project {
        int Id PK
        string Name
        string Description
        string LevelNamesJson
    }

    Folder {
        int Id PK
        string Name
        int ProjectId FK
        int ParentFolderId FK "nullable"
    }

    Subject {
        int Id PK
        string Name
        string Description
        int FolderId FK
        int Status
        bool Archived
    }

    Unit {
        int Id PK
        string Name
        int SubjectId FK
        bool Archived
    }

    Lesson {
        int Id PK
        string Name
        int UnitId FK
        bool Archived
    }

    LearningObjective {
        int Id PK
        string Name
        int LessonId FK
        int SchemaId FK
        string Tag
        string Environment
        string Template
        bool Archived
    }

    Task {
        int Id PK
        string Name
        int LearningObjectiveId FK
        int StepId FK "nullable"
        int GroupId FK
        int UserId FK "nullable"
        int Status
        int Priority
        int Duration
        bool Archived
    }
```

## Related entities (not part of the subject tree)

These attach to the hierarchy but are not ancestors of Subject:

```mermaid
erDiagram
    Schema ||--o{ LearningObjective : "SchemaId"
    Schema ||--o{ Node : "SchemaId"
    Node ||--o{ Step : "NodeId"
    Step ||--o{ Task : "StepId optional"
    TaskBank ||--o{ Step : "TaskBankId"
    Group ||--o{ Task : "GroupId"
    Group ||--o{ TaskBank : "GroupId"
    Sprint ||--o{ SprintLearningObjective : "SprintId"
    LearningObjective ||--o{ SprintLearningObjective : "LearningObjectiveId"

    Schema {
        int Id PK
        string Name
        bool Archived
    }

    Node {
        int Id PK
        string Name
        int SchemaId FK
        int Order
    }

    Step {
        int Id PK
        int NodeId FK
        int TaskBankId FK
        int Order
        int Duration
    }

    Sprint {
        int Id PK
        string Name
    }

    SprintLearningObjective {
        int Id PK
        int SprintId FK
        int LearningObjectiveId FK
    }
```

| Relationship | Type | Notes |
|--------------|------|-------|
| LearningObjective → Schema | N:1 | Workflow template applied to the LO |
| Schema → Node → Step | 1:N | Process definition |
| Task → Step | N:1 optional | Task instance of a workflow step |
| Sprint ↔ LearningObjective | N:M | Via `SprintLearningObjective` |
| Task → Group | N:1 | Work group assigned to the task |

## Model sources

| Entity | File |
|--------|------|
| Project | [`AutomatedTaskSystem/Models/Project.cs`](AutomatedTaskSystem/Models/Project.cs) |
| Folder | [`AutomatedTaskSystem/Models/Folder.cs`](AutomatedTaskSystem/Models/Folder.cs) |
| Subject | [`AutomatedTaskSystem/Models/Subject.cs`](AutomatedTaskSystem/Models/Subject.cs) |
| Unit | [`AutomatedTaskSystem/Models/Unit.cs`](AutomatedTaskSystem/Models/Unit.cs) |
| Lesson | [`AutomatedTaskSystem/Models/Lesson.cs`](AutomatedTaskSystem/Models/Lesson.cs) |
| LearningObjective | [`AutomatedTaskSystem/Models/LearningObjective.cs`](AutomatedTaskSystem/Models/LearningObjective.cs) |
| Task | [`AutomatedTaskSystem/Models/Task.cs`](AutomatedTaskSystem/Models/Task.cs) |

DbContext: [`AutomatedTaskSystem/Data/DataContext.cs`](AutomatedTaskSystem/Data/DataContext.cs)

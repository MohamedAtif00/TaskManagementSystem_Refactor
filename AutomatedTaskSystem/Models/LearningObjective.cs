namespace AutomatedTaskSystem.Models;

public class LearningObjective
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public Lesson Lesson { get; set; } = new Lesson { };
    public int LessonId { get; set; }
    public string Tag { get; set; } = "";
    public string Environment { get; set; } = "";
    public string Template { get; set; } = "";
    public Schema Schema { get; set; } = new Schema { };
    public int SchemaId { get; set; }
    public List<Models.Task> Tasks { get; set; } = new List<Task> { };
    public List<Comment> Comments { get; set; } = new List<Comment> { };
    public DateTime? StartedAt { get; set; } = null;
    public DateTime? DoneAt { get; set; } = null;
    public DateTime CreateAt { get; set; } = DateTime.Now;
    public bool Archived { get; set; } = false;
}

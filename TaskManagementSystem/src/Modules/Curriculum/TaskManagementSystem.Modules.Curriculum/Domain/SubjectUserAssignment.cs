namespace TaskManagementSystem.Modules.Curriculum.Domain;

public sealed class SubjectUserAssignment
{
    private SubjectUserAssignment()
    {
    }

    public int SubjectsId { get; internal set; }
    public int UsersId { get; internal set; }

    internal static SubjectUserAssignment Create(int subjectsId, int usersId) =>
        new() { SubjectsId = subjectsId, UsersId = usersId };
}

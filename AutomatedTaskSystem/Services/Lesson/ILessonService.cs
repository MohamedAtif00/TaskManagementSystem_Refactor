using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;

namespace AutomatedTaskSystem.Services.Lesson
{
    public interface ILessonService
    {
        Task<LearningObjective> CreateLO(int lessonId, Requests.LearningObjectiveDTO req);
    }
}
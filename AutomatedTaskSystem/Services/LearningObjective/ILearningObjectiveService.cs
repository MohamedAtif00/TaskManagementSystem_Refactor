using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;

namespace AutomatedTaskSystem.Services.LearningObjectiveService;

public interface ILearningObjectiveService
{
    Task<ResponseService<List<LearningObjective>>> GetLearningObjectivesBySubjectId(int subjectId);
}

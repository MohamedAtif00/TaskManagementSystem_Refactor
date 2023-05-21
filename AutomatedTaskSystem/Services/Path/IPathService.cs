using AutomatedTaskSystem.Models;

namespace AutomatedTaskSystem.Services.PathService;

public interface IPathService
{
	Task<List<Models.Path>> GeneratePath(int schemaId, LearningObjective learningObjective);
	Task<List<Models.Path>> GeneratePathFromStartPoint(Step StartPoint, LearningObjective learningObjective);
	Task<List<Models.Path>> UpdatePathTask(Models.Task task, Step step);
	Task<List<Models.Path>> GetPathByTask(Models.Task task);
	Task<List<Models.Path>> GetPathByLearningObjective(LearningObjective learningObjective);
	Task<List<Models.Path>> GetStepPreviousPath(Step step);
	Task<bool> DeleteLearningObjectivePath(int learningObjectiveId);
}

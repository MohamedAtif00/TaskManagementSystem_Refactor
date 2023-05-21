using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.TaskService;

namespace AutomatedTaskSystem.Services.PathService;

public class PathService : IPathService
{
	private readonly DataContext _context;

	public PathService(DataContext context, ITaskService taskService)
	{
		_context = context;
	}

	public async Task<List<Models.Path>> GeneratePath(
		int schemaId,
		LearningObjective learningObjective
	)
	{
		var res = new List<Models.Path> { };

		var paths = await _context.Paths
			.Where(p => p.LearningObjectiveId == learningObjective.Id)
			.Include(p => p.LearningObjective)
			.Include(p => p.Task)
			.Include(p => p.Step)
			.Include(p => p.NextStep)
			.ToListAsync();

		var schema = await _context.Schemas
			.Where(s => s.Id == schemaId)
			.Include(s => s.Nodes)
			.ThenInclude(n => n.Steps)
			.ThenInclude(s => s.TaskBank)
			.ThenInclude(tb => tb.Group)
			.Include(s => s.Nodes)
			.ThenInclude(n => n.Previous)
			.FirstOrDefaultAsync();

		if (schema is not null)
		{
			foreach (var node in schema.Nodes)
				foreach (var step in node.Steps)
				{
					var nextStep = getStepByOrder(node, step.Order + 1);

					if (nextStep is not null)
					{
						var path = new Models.Path
						{
							Step = step,
							StepId = step.Id,
							NextStep = nextStep,
							NextStepId = nextStep.Id,
							LearningObjective = learningObjective,
							LearningObjectiveId = learningObjective.Id,
						};

						res.Add(path);
						_context.Paths.Add(path);
					}
					else
					{
						foreach (var item in nextNodesInSchema(node, schema))
						{
							var nextStepInItem = firstStepInNode(item);

							if (nextStepInItem is not null)
							{
								var path = new Models.Path
								{
									Step = step,
									StepId = step.Id,
									NextStep = nextStepInItem,
									NextStepId = nextStepInItem.Id,
									LearningObjective = learningObjective,
									LearningObjectiveId = learningObjective.Id,
								};

								res.Add(path);
								_context.Paths.Add(path);
							}
						}
					}
				}
		}

		await _context.SaveChangesAsync();

		return res;
	}

	public async Task<List<Models.Path>> GeneratePathFromStartPoint(
		Step StartPoint,
		LearningObjective learningObjective
	)
	{
		var node = await _context.Nodes.Where(n => n.Id == StartPoint.NodeId).FirstOrDefaultAsync();

		if (node is null)
			throw new NotImplementedException("Tarsh");

		var schema = await _context.Schemas
			.Where(s => s.Id == node.SchemaId)
			.Include(s => s.Nodes)
			.ThenInclude(n => n.Steps)
			.ThenInclude(s => s.TaskBank)
			.ThenInclude(tb => tb.Group)
			.Include(s => s.Nodes)
			.ThenInclude(n => n.Previous)
			.FirstOrDefaultAsync();

		var res = new List<Models.Path> { };

		if (schema is not null)
		{
			var nextNodes = nextNodesAfterCurrentNode(node, schema);
			nextNodes.Add(node);
			foreach (var nextNode in nextNodes)
				foreach (var step in nextNode.Steps)
				{
					if (
						nextNode.Id != node.Id
						|| (nextNode.Id == node.Id && step.Order >= StartPoint.Order)
					)
					{
						var nextStep = getStepByOrder(nextNode, step.Order + 1);

						if (nextStep is not null)
						{
							if (
								!await checkIfNewPathExists(
									stepId: step.Id,
									nextStepId: nextStep.Id,
									loId: learningObjective.Id,
									additionalPath: res
								)
							)
							{
								var path = new Models.Path
								{
									Step = step,
									StepId = step.Id,
									NextStep = nextStep,
									NextStepId = nextStep.Id,
									LearningObjective = learningObjective,
									LearningObjectiveId = learningObjective.Id,
								};

								res.Add(path);
								_context.Paths.Add(path);
							}
						}
						else
						{
							foreach (var item in nextNodesInSchema(nextNode, schema))
							{
								var nextStepInItem = firstStepInNode(item);

								if (
									nextStepInItem is not null
									&& !await checkIfNewPathExists(
										stepId: step.Id,
										nextStepId: nextStepInItem.Id,
										loId: learningObjective.Id,
										additionalPath: res
									)
								)
								{
									var path = new Models.Path
									{
										Step = step,
										StepId = step.Id,
										NextStep = nextStepInItem,
										NextStepId = nextStepInItem.Id,
										LearningObjective = learningObjective,
										LearningObjectiveId = learningObjective.Id,
									};

									res.Add(path);
									_context.Paths.Add(path);
								}
							}
						}
					}
				}
		}

		await _context.SaveChangesAsync();

		return res;
	}

	public async Task<List<Models.Path>> UpdatePathTask(Models.Task task, Step step)
	{
		var paths = await _context.Paths
			.Where(
				p =>
					p.TaskId == null
					&& p.LearningObjectiveId == task.LearningObjectiveId
					&& p.StepId == step.Id
			)
			.ToListAsync();

		foreach (var path in paths)
			path.Task = task;

		await _context.SaveChangesAsync();

		return paths;
	}

	public async Task<List<Models.Path>> GetPathByTask(Models.Task task)
	{
		return await _context.Paths
			.Include(p => p.Step)
			.Include(p => p.NextStep)
			.ThenInclude(ns => ns.Node)
			.Include(p => p.NextStep)
			.ThenInclude(ns => ns.TaskBank)
			.ThenInclude(tb => tb.Group)
			.Where(p => p.TaskId == task.Id)
			.ToListAsync();
	}

	public async Task<List<Models.Path>> GetStepPreviousPath(Step step)
	{
		return await _context.Paths
			.Include(p => p.NextStep)
			.Include(p => p.Step)
			.Include(p => p.Task)
			.Where(p => p.NextStepId == step.Id)
			.ToListAsync();
	}

	public async Task<List<Models.Path>> GetPathByLearningObjective(
		LearningObjective learningObjective
	)
	{
		return await _context.Paths
			.Include(p => p.NextStep)
			.Include(p => p.Step)
			.Include(p => p.Task)
			.Where(p => p.LearningObjectiveId == learningObjective.Id)
			.ToListAsync();
	}

	public async Task<bool> DeleteLearningObjectivePath(int learningObjectiveId)
	{
		var paths = await _context.Paths
			.Where(p => p.LearningObjectiveId == learningObjectiveId)
			.ToListAsync();

		foreach (var item in paths)
			_context.Paths.Remove(item);

		await _context.SaveChangesAsync();

		return true;
	}

	private async Task<bool> checkIfNewPathExists(
		int stepId,
		int nextStepId,
		int loId,
		List<Models.Path> additionalPath
	)
	{
		var paths = await _context.Paths
			.Where(p => p.LearningObjectiveId == loId)
			.Include(p => p.LearningObjective)
			.Include(p => p.Task)
			.Include(p => p.Step)
			.Include(p => p.NextStep)
			.ToListAsync();

		return paths.Any(
				p =>
					p.Task == null
					&& p.StepId == stepId
					&& p.NextStepId == nextStepId
					&& p.LearningObjectiveId == loId
			)
			|| additionalPath.Any(
				p =>
					p.Task == null
					&& p.StepId == stepId
					&& p.NextStepId == nextStepId
					&& p.LearningObjectiveId == loId
			);
	}

	private List<Node> nextNodesAfterCurrentNode(Node node, Schema schema)
	{
		var res = new List<Node> { };

		var nexts = nextNodesInSchema(node, schema);

		foreach (var item in nexts)
		{
			res.Add(item);

			var afterNext = nextNodesAfterCurrentNode(item, schema);
			foreach (var _ in afterNext)
				res.Add(_);
		}

		return res;
	}

	private List<Node> nextNodesInSchema(Node currentNode, Schema schema) =>
		schema.Nodes.Where(n => n.Previous.Any(n => n.Id == currentNode.Id)).ToList();

	private List<Node> firstNodesInSchema(Schema schema)
		=> schema.Nodes.Where(n => n.isStart).ToList();

	private Step? getStepByOrder(Node node, int Order)
		=> node.Steps.Where(s => s.Order == Order).FirstOrDefault();

	private Step? firstStepInNode(Node node)
		=> node.Steps.Where(s => s.Order == 1).FirstOrDefault();
}

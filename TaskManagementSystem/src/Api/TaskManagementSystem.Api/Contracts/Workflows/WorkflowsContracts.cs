using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Api.Contracts.Workflows;

public sealed class CreateSchemaRequest
{
    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public int? TypeId { get; init; }
}

public sealed class UpdateSchemaRequest
{
    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public int? TypeId { get; init; }
}

public sealed class SchemaTypeResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;
}

public sealed class SchemaListItemResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public int? TypeId { get; init; }
}

public sealed class SchemaDetailResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public int? TypeId { get; init; }
}

public sealed class CreateTaskBankItemRequest
{
    public string Name { get; init; } = string.Empty;

    public int Duration { get; init; }

    public TaskBankType Type { get; init; }

    public bool TeamLeaderOnly { get; init; }

    public int TeamId { get; init; }
}

public sealed class UpdateTaskBankItemRequest
{
    public string Name { get; init; } = string.Empty;

    public int Duration { get; init; }

    public TaskBankType Type { get; init; }

    public bool TeamLeaderOnly { get; init; }

    public int TeamId { get; init; }
}

public sealed class TaskBankListItemResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int Duration { get; init; }

    public TaskBankType Type { get; init; }

    public bool TeamLeaderOnly { get; init; }

    public int TeamId { get; init; }
}

public sealed class CreateNodeRequest
{
    public string Name { get; init; } = string.Empty;

    public bool IsStart { get; init; }

    public bool IsEnd { get; init; }
}

public sealed class UpdateNodeRequest
{
    public string Name { get; init; } = string.Empty;

    public bool IsStart { get; init; }

    public bool IsEnd { get; init; }
}

public sealed class NodeListItemResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int Order { get; init; }

    public bool IsStart { get; init; }

    public bool IsEnd { get; init; }

    public int SchemaId { get; init; }
}

public sealed class CreateStepRequest
{
    public int TaskBankId { get; init; }

    public int Duration { get; init; }

    public int Priority { get; init; }
}

public sealed class UpdateStepRequest
{
    public int TaskBankId { get; init; }

    public int Duration { get; init; }

    public int Priority { get; init; }
}

public sealed class StepListItemResponse
{
    public int Id { get; init; }

    public int Order { get; init; }

    public int Duration { get; init; }

    public int Priority { get; init; }

    public int NodeId { get; init; }

    public int TaskBankId { get; init; }
}

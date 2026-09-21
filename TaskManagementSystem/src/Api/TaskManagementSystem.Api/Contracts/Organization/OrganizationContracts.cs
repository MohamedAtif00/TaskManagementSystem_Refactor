namespace TaskManagementSystem.Api.Contracts.Organization;

public sealed class CreateTeamRequest
{
    public string Name { get; init; } = string.Empty;

    public int? TeamleaderId { get; init; }
}

public sealed class UpdateTeamRequest
{
    public string Name { get; init; } = string.Empty;

    public int? TeamleaderId { get; init; }
}

public sealed class TeamListItemResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int Members { get; init; }

    public int? TeamleaderId { get; init; }

    public string? TeamleaderName { get; init; }
}

public sealed class TeamMemberResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;
}

public sealed class TeamDetailResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<TeamMemberResponse> Members { get; init; } = [];

    public int? TeamleaderId { get; init; }

    public string? TeamleaderName { get; init; }
}

public sealed class CreateSectionRequest
{
    public string Name { get; init; } = string.Empty;

    public int HeadId { get; init; }

    public IReadOnlyList<int> TeamIds { get; init; } = [];
}

public sealed class UpdateSectionRequest
{
    public string Name { get; init; } = string.Empty;

    public int HeadId { get; init; }

    public IReadOnlyList<int> TeamIds { get; init; } = [];
}

public sealed class SectionListItemResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;
}

public sealed class IdNameResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;
}

public sealed class SectionTeamResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;
}

public sealed class SectionDetailResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public IdNameResponse Head { get; init; } = new();

    public IReadOnlyList<SectionTeamResponse> Teams { get; init; } = [];
}

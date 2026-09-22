namespace TaskManagementSystem.Modules.HR.Application;

internal static class HrOpinionAuthorization
{
    public static bool CanGiveOpinion(
        int actorUserId,
        string actorRole,
        int requestUserId,
        int? teamleaderId,
        int? sectionheadId) =>
        actorRole switch
        {
            "Owner" => true,
            "TeamLeader" => teamleaderId == actorUserId,
            "SectionHead" => sectionheadId == actorUserId,
            "ProjectManger" => requestUserId != actorUserId,
            _ => false
        };
}

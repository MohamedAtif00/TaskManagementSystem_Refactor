import { ISummary } from "../../pages/summaries/[projectId]";
import authService from "../Auth";
import { BasicInfo, url } from "./";
import REPORTS from "./Reports";
import { IDName } from "./workFromHome";

/** Single segment for `/subjects/{id}/analytics/...` (avoids `1,2` when query is string[]). */
const projectAnalyticsPathId = (projectId: string | string[] | number | undefined) => {
    if (projectId === undefined || projectId === null) return "";
    if (typeof projectId === "number") return String(projectId);
    if (Array.isArray(projectId)) return projectId[0] != null ? String(projectId[0]) : "";
    return String(projectId);
};

const PROJECTS = {
    REPORTS,
    SUMMARY: async (projectId: string | string[]) => {
        try {
            const res = await fetch(`${url}/subjects/${projectId}/summary`);
            const data: ISummary = await res.json();
            return data;
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    USERS_UNASSIGNED: async (projectId: string | string[]) => {
        try {
            const res = await fetch(
                `${url}/subjects/${projectId}/users/unassigned`
            );
            const data: {
                data: IUser[];
                error: boolean;
                message: string;
            } = await res.json();
            return data;
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    USERS_ASSIGNED: async (projectId: string | string[]) => {
        try {
            const res = await fetch(
                `${url}/subjects/${projectId}/users/assigned`
            );
            const data: {
                data: IUser[];
                error: boolean;
                message: string;
            } = await res.json();
            return data;
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    UNASSIGN: async ({
        projectId,
        userIds,
    }: {
        projectId: string | string[];
        userIds: number[];
    }) => {
        try {
            const res = await fetch(`${url}/subjects/${projectId}/unassign`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ userIds }),
            });
            const data: IProject = await res.json();
            return data;
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    ASSIGN: async ({
        projectId,
        userIds,
        groupIds,
    }: {
        projectId: string | string[];
        userIds: number[] | undefined;
        groupIds: number[] | undefined;
    }) => {
        try {
            const res = await fetch(`${url}/subjects/${projectId}/assign`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ userIds }),
            });
            const data: IProject = await res.json();
            return data;
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    EDIT: async ({
        id,
        name,
        description,
        folderId,
    }: {
        id: string | string[] | number;
        name: string;
        description: string;
        folderId: number;
    }) => {
        try {
            const res = await fetch(`${url}/subjects/${id}`, {
                method: "PATCH",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ name, description, folderId }),
            });
            const data: {
                data: IProject;
                error: boolean;
                message: string;
            } = await res.json();
            return data;
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    UPDATE_STATUS: async ({
        id,
        status,
    }: {
        id: string | string[] | number;
        status: 0 | 1 | 2;
    }) => {
        try {
            const res = await fetch(`${url}/subjects/${id}/status`, {
                method: "PATCH",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ status }),
            });
            const data: {
                data: IProject;
                error: boolean;
                message: string;
            } = await res.json();
            return data;
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    CREATE: async ({
        name,
        description,
        folderId,
    }: {
        name: string;
        description: string;
        folderId: number;
    }) => {
        try {
            const res = await fetch(`${url}/subjects`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ name, description, folderId }),
            });
            const data: {
                data: IProject;
                error: boolean;
                message: string;
            } = await res.json();
            return data;
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    DELETE: async (id: string | string[] | number) => {
        try {
            const res = await fetch(`${url}/subjects/${id}`, {
                method: "DELETE",
            });
            const data: {
                error: boolean;
                message: string;
            } = await res.json();
            return data;
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    GET_ALL_LOS: async (projectId: number) => {

        const res = await fetch(`${url}/subjects/${projectId}/los`);
        const data: {
            error: boolean;
            message: string;
            data: IDName[];
        } = await res.json();
        return data;

    },
    GET_ALL: async () => {
      
        const res = await fetch(`${url}/subjects`);
        const data: {
            data: IProject[];
            error: boolean;
            message: string;
        } = await res.json();
        return data;

    },
    GET_BY_TERM: async (
        folderId: number,
        options?: { includeInactive?: boolean }
    ) => {
        const query = options?.includeInactive ? "?includeInactive=true" : "";
        const res = await fetch(`${url}/subjects/by-folder/${folderId}${query}`, {
            headers: { ...authService.authHeader() },
        });
        const data: {
            data: IProject[];
            error: boolean;
            message: string;
        } = await res.json();
        return data;
    },
    GET_BY_FOLDER: async (
        folderId: number,
        options?: { includeInactive?: boolean }
    ) => PROJECTS.GET_BY_TERM(folderId, options),
    GET_ALL_FOR_SPRINT:async ()=>{

        const res = await fetch(`${url}/subjects/GetAllForSprint`);
        const data: {
            data: IProject[];
            error: boolean;
            message: string;
        } = await res.json();
        return data;
    },
    GET_ANALYTICS_OVERVIEW: async (projectId: number | string | string[], timePeriod?: number) => {
        try {
            const pid = projectAnalyticsPathId(projectId);
            if (!pid) return false;
            const authHeader = authService.authHeader();
            const queryParams = timePeriod ? `?timePeriod=${timePeriod}` : "";
            const res = await fetch(`${url}/subjects/${pid}/analytics/overview${queryParams}`, {
                headers: { ...authHeader },
            });
            const raw = await res.json();
            const error = raw?.error ?? raw?.Error;
            const outer = raw?.data ?? raw?.Data;
            if (!res.ok || error || !outer) {
                return {
                    error: true,
                    message: raw?.message ?? raw?.Message ?? "Failed to fetch project analytics",
                    data: undefined,
                };
            }
            const pp = outer.projectProgress ?? outer.ProjectProgress ?? {};
            const tas = outer.tasksSummary ?? outer.TasksSummary ?? {};
            const las = outer.learningActivitiesSummary ?? outer.LearningActivitiesSummary ?? {};
            const loSum = las.loSummary ?? las.LoSummary ?? {};
            const tagsRaw = las.tags ?? las.Tags ?? [];
            const tags = (Array.isArray(tagsRaw) ? tagsRaw : []).map((t: any) => ({
                groupId: t.groupId ?? t.GroupId,
                label: t.label ?? t.Label ?? "",
                value: t.value ?? t.Value ?? 0,
                color: t.color ?? t.Color ?? "#6b7280",
                isFilled: t.isFilled ?? t.IsFilled ?? false,
            }));
            return {
                error: false,
                message: raw?.message ?? raw?.Message ?? "",
                data: {
                    numberOfUnits: outer.numberOfUnits ?? outer.NumberOfUnits ?? 0,
                    numberOfLessons: outer.numberOfLessons ?? outer.NumberOfLessons ?? 0,
                    numberOfLearningObjectives:
                        outer.numberOfLearningObjectives ?? outer.NumberOfLearningObjectives ?? 0,
                    projectProgress: {
                        progressPercent: pp.progressPercent ?? pp.ProgressPercent ?? 0,
                        totalExpectedTasks: pp.totalExpectedTasks ?? pp.TotalExpectedTasks ?? 0,
                        completedTasks: pp.completedTasks ?? pp.CompletedTasks ?? 0,
                        activeTasks: pp.activeTasks ?? pp.ActiveTasks ?? 0,
                    },
                    learningActivitiesSummary: {
                        loSummary: {
                            completed: loSum.completed ?? loSum.Completed ?? 0,
                            inProcess: loSum.inProcess ?? loSum.InProcess ?? 0,
                            notStarted: loSum.notStarted ?? loSum.NotStarted ?? 0,
                            total: loSum.total ?? loSum.Total ?? 0,
                        },
                        tags,
                    },
                    tasksSummary: {
                        active: tas.active ?? tas.Active ?? 0,
                        completed: tas.completed ?? tas.Completed ?? 0,
                        rollback: tas.rollback ?? tas.Rollback ?? 0,
                        flagged: tas.flagged ?? tas.Flagged ?? 0,
                        notStarted: tas.notStarted ?? tas.NotStarted ?? 0,
                        total: tas.total ?? tas.Total ?? 0,
                    },
                },
            };
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    GET_PROJECT_LO_PROGRESS: async (
        projectId: string | string[],
        timePeriod?: number,
        group?: number,
        inProgressOnly?: boolean
    ) => {
        try {
            const pid = projectAnalyticsPathId(projectId);
            if (!pid) return false;
            const authHeader = authService.authHeader();
            const params = new URLSearchParams();
            if (timePeriod) params.set("timePeriod", String(timePeriod));
            if (group !== undefined && group !== null) params.set("group", String(group));
            if (inProgressOnly) params.set("inProgressOnly", "true");
            const qs = params.toString() ? `?${params.toString()}` : "";
            const res = await fetch(
                `${url}/subjects/${pid}/analytics/learning-objectives-progress${qs}`,
                { headers: { ...authHeader } }
            );
            const raw = await res.json();
            const error = raw?.error ?? raw?.Error;
            const outer = raw?.data ?? raw?.Data;
            const listRaw = outer?.data ?? outer?.Data ?? [];
            const list = Array.isArray(listRaw)
                ? listRaw.map((item: any) => ({
                      id: item.id ?? item.Id,
                      name: item.name ?? item.Name ?? "",
                      value: item.value ?? item.Value ?? 0,
                      status: item.status ?? item.Status ?? "Delayed",
                  }))
                : [];
            if (!res.ok || error) {
                return {
                    error: true,
                    message: raw?.message ?? raw?.Message ?? "Failed to fetch LO progress",
                    data: undefined,
                };
            }
            return {
                error: false,
                message: raw?.message ?? raw?.Message ?? "",
                data: { data: list },
            };
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    GET_PROJECT_IN_PROGRESS_TASKS: async (
        projectId: string | string[],
        timePeriod?: number,
        group?: number
    ) => {
        try {
            const pid = projectAnalyticsPathId(projectId);
            if (!pid) return false;
            const authHeader = authService.authHeader();
            const params = new URLSearchParams();
            if (timePeriod) params.set("timePeriod", String(timePeriod));
            if (group !== undefined && group !== null) params.set("group", String(group));
            const qs = params.toString() ? `?${params.toString()}` : "";
            const res = await fetch(
                `${url}/subjects/${pid}/analytics/in-progress-tasks${qs}`,
                { headers: { ...authHeader } }
            );
            const raw = await res.json();
            const error = raw?.error ?? raw?.Error;
            const outer = raw?.data ?? raw?.Data;
            const listRaw = outer?.data ?? outer?.Data ?? [];
            const list = Array.isArray(listRaw)
                ? listRaw.map((item: any) => ({
                      id: item.id ?? item.Id,
                      name: item.name ?? item.Name ?? "",
                      status: item.status ?? item.Status ?? 0,
                      statusName: item.statusName ?? item.StatusName ?? "",
                      learningObjectiveId:
                          item.learningObjectiveId ?? item.LearningObjectiveId,
                      learningObjectiveName:
                          item.learningObjectiveName ?? item.LearningObjectiveName ?? "",
                      groupId: item.groupId ?? item.GroupId,
                      groupName: item.groupName ?? item.GroupName ?? "",
                      groupColor: item.groupColor ?? item.GroupColor ?? "#6b7280",
                      assigneeName: item.assigneeName ?? item.AssigneeName ?? "",
                  }))
                : [];
            if (!res.ok || error) {
                return {
                    error: true,
                    message: raw?.message ?? raw?.Message ?? "Failed to fetch in-progress tasks",
                    data: undefined,
                };
            }
            return {
                error: false,
                message: raw?.message ?? raw?.Message ?? "",
                data: { data: list },
            };
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    GET_PROJECT_LO_TABLE: async (projectId: string | string[]) => {
        try {
            const pid = projectAnalyticsPathId(projectId);
            if (!pid) return false;
            const authHeader = authService.authHeader();
            const res = await fetch(`${url}/subjects/${pid}/analytics/learning-objectives-table`, {
                headers: { ...authHeader },
            });
            const raw = await res.json();
            const error = raw?.error ?? raw?.Error;
            const outer = raw?.data ?? raw?.Data;
            const rowsRaw = outer?.data ?? outer?.Data ?? [];
            const projectName = outer?.projectName ?? outer?.ProjectName ?? "";
            if (!res.ok || error) {
                return {
                    error: true,
                    message: raw?.message ?? raw?.Message ?? "Failed to fetch learning objectives table",
                    data: undefined,
                };
            }
            const rows = Array.isArray(rowsRaw)
                ? rowsRaw.map((r: any) => ({
                      id: r.id ?? r.Id,
                      name: r.name ?? r.Name,
                      subject: r.subject ?? r.Subject,
                      startDate: r.startDate ?? r.StartDate,
                      endDate:r.endDate ?? r.EndaDate,
                      activeTasks: r.activeTasks ?? r.ActiveTasks,
                      currentPhases: (r.currentPhases ?? r.CurrentPhases ?? []).map((p: any) => ({
                          groupName: p.groupName ?? p.GroupName,
                          colorCode: p.colorCode ?? p.ColorCode,
                      })),
                      status: r.status ?? r.Status,
                      progress: r.progress ?? r.Progress,
                  }))
                : [];
            return {
                error: false,
                message: raw?.message ?? raw?.Message ?? "",
                data: { projectName, data: rows },
            };
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    GET_ONE: async (id: string | string[]) => {
        try {
            const res = await fetch(`${url}/subjects/${id}`, {
                headers: { ...authService.authHeader() },
            });
            if (res.status >= 400) return false;
            const data: {
                data: IProject;
                error: boolean;
                message: string;
            } = await res.json();
            return data;
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    GET_ONE_DETAILED: async (id: string | string[]) => {
        try {
            const res = await fetch(`${url}/subjects/${id}/details`, {
                headers: { ...authService.authHeader() },
            });
            if (res.status >= 400) return false;
            const data: {
                data: ProjectDetails;
                error: boolean;
                message: string;
            } = await res.json();
            return data;
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    UNITS: {
        ADD: async ({
            name,
            projectId,
        }: {
            name: string;
            projectId: number;
        }) => {
            try {
                const res = await fetch(`${url}/subjects/${projectId}/units`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({ name }),
                });
                const data: {
                    data: Unit;
                    error: boolean;
                    message: string;
                } = await res.json();
                return data;
            } catch (error) {
                console.error(error);
                return false;
            }
        },
        EDIT: async ({ name, id }: { name: string; id: number }) => {
            try {
                const res = await fetch(`${url}/units/${id}`, {
                    method: "PATCH",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({ name }),
                });
                const data: {
                    data: BasicInfo;
                    error: boolean;
                    message: string;
                } = await res.json();
                return data;
            } catch (error) {
                console.error(error);
                return false;
            }
        },
        REMOVE: async (id: number) => {
            try {
                const res = await fetch(`${url}/units/${id}`, {
                    method: "DELETE",
                    headers: {
                        "Content-Type": "application/json",
                    },
                });
                const data = await res.json();
                return data;
            } catch (error) {
                console.error(error);
                return false;
            }
        },
        LESSONS: {
            ADD: async ({ unitId, name }: { unitId: number; name: string }) => {
                try {
                    const res = await fetch(`${url}/units/${unitId}/lessons`, {
                        method: "POST",
                        headers: {
                            "Content-Type": "application/json",
                        },
                        body: JSON.stringify({ name }),
                    });
                    const data: Lesson = await res.json();
                    return data;
                } catch (error) {
                    console.error(error);
                    return false;
                }
            },
            EDIT: async ({ id, name }: { id: number; name: string }) => {
                try {
                    const res = await fetch(`${url}/lessons/${id}`, {
                        method: "PATCH",
                        headers: {
                            "Content-Type": "application/json",
                        },
                        body: JSON.stringify({ name }),
                    });
                    const data: ResponseService<BasicInfo> = await res.json();
                    return data;
                } catch (error) {
                    console.error(error);
                    return false;
                }
            },
            REMOVE: async (id: number) => {
                try {
                    const res = await fetch(`${url}/lessons/${id}`, {
                        method: "DELETE",
                        headers: {
                            "Content-Type": "application/json",
                        },
                    });
                    const data = await res.json();
                    return data;
                } catch (error) {
                    console.error(error);
                    return false;
                }
            },
            LEARNING_OBJECTIVES: {
                CREATE: async ({
                    lessonId,
                    name,
                    schemaId,
                    environment,
                    tag,
                    template,
                }: {
                    lessonId: number;
                    name: string;
                    schemaId: number;
                    tag: string;
                    template: string;
                    environment: string;
                }) => {
                    try {
                        const res = await fetch(
                            `${url}/lessons/${lessonId}/learning-objective`,
                            {
                                method: "POST",
                                headers: {
                                    "Content-Type": "application/json",
                                },
                                body: JSON.stringify({
                                    name,
                                    schemaId,
                                    environment,
                                    tag,
                                    template,
                                }),
                            }
                        );
                        const data: LearningObjective = await res.json();
                        return data;
                    } catch (error) {
                        console.error(error);
                        return false;
                    }
                },
                EDIT: async (
                    id: number,
                    edit: {
                        name: string;
                        tag: string;
                        environment: string;
                        nods?:number[];
                        template: string;
                        schemaId: number;
                        steps: number[];
                    }
                ) => {
                    try {
                        const authHeader = authService.authHeader();
                        const res = await fetch(
                            `${url}/learning-objectives/${id}`,
                            {
                                method: "PATCH",
                                headers: {
                                    ...authHeader,
                                    "Content-Type": "application/json",
                                },
                                body: JSON.stringify(edit),
                            }
                        );
                        const data: LearningObjective = await res.json();
                        return data;
                    } catch (error) {
                        console.error(error);
                        return false;
                    }
                },
                REMOVE: async (id: number) => {
                    try {
                        const res = await fetch(
                            `${url}/learning-objectives/${id}`,
                            {
                                method: "DELETE",
                            }
                        );
                        const data = await res.json();
                        return data;
                    } catch (error) {
                        console.error(error);
                        return false;
                    }
                },
                ASSIGN: async (id: string | string[], userIds: number[]) => {
                    try {
                        const res = await fetch(
                            `${url}/learning-objectives/${id}/assign`,
                            {
                                method: "POST",
                                headers: {
                                    "Content-Type": "application/json",
                                },
                                body: JSON.stringify({ userIds }),
                            }
                        );
                        const data: LearningObjective = await res.json();
                        return data;
                    } catch (error) {
                        console.error(error);
                        return false;
                    }
                },
                UNASSIGN: async (id: string | string[], userIds: number[]) => {
                    try {
                        const res = await fetch(
                            `${url}/learning-objectives/${id}/unassign`,
                            {
                                method: "POST",
                                headers: {
                                    "Content-Type": "application/json",
                                },
                                body: JSON.stringify({ userIds }),
                            }
                        );
                        const data: LearningObjective = await res.json();
                        return data;
                    } catch (error) {
                        console.error(error);
                        return false;
                    }
                },
            },
        },
    },
    ROOT: {
        LIST: async () => {
            const res = await fetch(`${url}/curriculum/years`);
            return res.json();
        },
        WITH_SUBJECTS: async () => {
            const res = await fetch(`${url}/curriculum/subject-groups/with-subjects`);
            return res.json();
        },
        GET: async (yearId: number) => {
            const res = await fetch(`${url}/curriculum/years/${yearId}`);
            return res.json();
        },
        TREE: async (yearId: number) => {
            const res = await fetch(`${url}/curriculum/years/${yearId}/tree`);
            return res.json();
        },
        CREATE: async (name: string, description?: string) => {
            const res = await fetch(`${url}/curriculum/years`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ name, description: description ?? "" }),
            });
            return res.json();
        },
        UPDATE: async (yearId: number, name: string, description?: string) => {
            const res = await fetch(`${url}/curriculum/years/${yearId}`, {
                method: "PATCH",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ name, description: description ?? "" }),
            });
            return res.json();
        },
        YEARS: async (yearId: number) => {
            const res = await fetch(`${url}/curriculum/years/${yearId}/projects`);
            return res.json();
        },
        GET_YEAR: async (projectId: number) => {
            const res = await fetch(`${url}/curriculum/projects/${projectId}`);
            return res.json();
        },
        CREATE_YEAR: async (yearId: number, label: string) => {
            const res = await fetch(`${url}/curriculum/years/${yearId}/projects`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ name: label }),
            });
            return res.json();
        },
        UPDATE_YEAR: async (projectId: number, label: string) => {
            const res = await fetch(`${url}/curriculum/projects/${projectId}`, {
                method: "PATCH",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ name: label }),
            });
            return res.json();
        },
        TERMS: async (parentId: number, parentType: "year" | "project" | "term" = "year") => {
            const path =
                parentType === "year"
                    ? `${url}/curriculum/years/${parentId}/projects`
                    : parentType === "project"
                      ? `${url}/curriculum/projects/${parentId}/terms`
                      : `${url}/curriculum/terms/${parentId}/subject-groups`;
            const res = await fetch(path);
            return res.json();
        },
        GET_TERM: async (nodeId: number, nodeType: "project" | "term" | "subjectGroup" = "term") => {
            const path =
                nodeType === "project"
                    ? `${url}/curriculum/projects/${nodeId}`
                    : nodeType === "term"
                      ? `${url}/curriculum/terms/${nodeId}`
                      : `${url}/curriculum/subject-groups/${nodeId}`;
            const res = await fetch(path);
            return res.json();
        },
        CREATE_TERM: async (
            parentId: number,
            body: {
                name: string;
                parentType?: "year" | "project" | "term";
                order?: number;
                startDate?: string | null;
                endDate?: string | null;
            }
        ) => {
            const parentType = body.parentType ?? "project";
            const path =
                parentType === "year"
                    ? `${url}/curriculum/years/${parentId}/projects`
                    : parentType === "project"
                      ? `${url}/curriculum/projects/${parentId}/terms`
                      : `${url}/curriculum/terms/${parentId}/subject-groups`;
            const res = await fetch(path, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    name: body.name,
                    startDate: body.startDate,
                    endDate: body.endDate,
                }),
            });
            return res.json();
        },
        UPDATE_TERM: async (
            nodeId: number,
            body: {
                name: string;
                nodeType?: "project" | "term" | "subjectGroup";
                order?: number;
                startDate?: string | null;
                endDate?: string | null;
            }
        ) => {
            const nodeType = body.nodeType ?? "term";
            const path =
                nodeType === "project"
                    ? `${url}/curriculum/projects/${nodeId}`
                    : nodeType === "term"
                      ? `${url}/curriculum/terms/${nodeId}`
                      : `${url}/curriculum/subject-groups/${nodeId}`;
            const res = await fetch(path, {
                method: "PATCH",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    name: body.name,
                    startDate: body.startDate,
                    endDate: body.endDate,
                }),
            });
            return res.json();
        },
        DELETE_TERM: async (
            nodeId: number,
            body: { nodeType?: "year" | "project" | "term" | "subjectGroup" } = {}
        ) => {
            const nodeType = body.nodeType ?? "term";
            const path =
                nodeType === "year"
                    ? `${url}/curriculum/years/${nodeId}`
                    : nodeType === "project"
                      ? `${url}/curriculum/projects/${nodeId}`
                      : nodeType === "term"
                        ? `${url}/curriculum/terms/${nodeId}`
                        : `${url}/curriculum/subject-groups/${nodeId}`;
            const res = await fetch(path, { method: "DELETE" });
            return res.json();
        },
        ARCHIVED_LIST: async () => {
            const res = await fetch(`${url}/curriculum/archived`);
            return res.json();
        },
        RESTORE: async (
            nodeType: "year" | "project" | "term" | "subjectGroup",
            id: number,
            subjectIds?: number[]
        ) => {
            const path =
                nodeType === "year"
                    ? `${url}/curriculum/years/${id}/restore`
                    : nodeType === "project"
                      ? `${url}/curriculum/projects/${id}/restore`
                      : nodeType === "term"
                        ? `${url}/curriculum/terms/${id}/restore`
                        : `${url}/curriculum/subject-groups/${id}/restore`;
            const res = await fetch(path, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ subjectIds: subjectIds ?? [] }),
            });
            return res.json();
        },
    },
    YEARS: {
        GET_ALL: async () => {
            try {
                const res = await fetch(`${url}/subjects/years`);
                const data: {
                    data: { id: number; name: string }[];
                    error: boolean;
                    message: string;
                } = await res.json();
                return data;
            } catch (error) {
                console.error(error);
                return false;
            }
        },
    },
};

export default PROJECTS;

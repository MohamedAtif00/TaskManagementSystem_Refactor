import { ISummary } from "../../pages/summaries/[projectId]";
import authService from "../Auth";
import { BasicInfo, url } from "./";
import REPORTS from "./Reports";
import { IDName } from "./workFromHome";

const PROJECTS = {
    REPORTS,
    SUMMARY: async (projectId: string | string[]) => {
        try {
            const res = await fetch(`${url}/projects/${projectId}/summary`);
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
                `${url}/projects/${projectId}/users/unassigned`
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
                `${url}/projects/${projectId}/users/assigned`
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
            const res = await fetch(`${url}/projects/${projectId}/unassign`, {
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
    }: {
        projectId: string | string[];
        userIds: number[];
    }) => {
        try {
            const res = await fetch(`${url}/projects/${projectId}/assign`, {
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
        term,
        year,
    }: {
        id: string | string[] | number;
        name: string;
        description: string;
        term: boolean;
        year: number;
    }) => {
        try {
            const res = await fetch(`${url}/projects/${id}`, {
                method: "PATCH",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ name, description, term, yearId: year }),
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
            const res = await fetch(`${url}/projects/${id}/status`, {
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
        term,
        year,
    }: {
        name: string;
        description: string;
        term: boolean;
        year: number;
    }) => {
        try {
            const res = await fetch(`${url}/projects`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ name, description, term, yearId: year }),
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
            const res = await fetch(`${url}/projects/${id}`, {
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

        const res = await fetch(`${url}/projects/${projectId}/los`);
        const data: {
            error: boolean;
            message: string;
            data: IDName[];
        } = await res.json();
        return data;

    },
    GET_ALL: async () => {
      
        const res = await fetch(`${url}/projects`);
        const data: {
            data: IProject[];
            error: boolean;
            message: string;
        } = await res.json();
        return data;

    },GET_ALL_FOR_SPRINT:async ()=>{

        const res = await fetch(`${url}/projects/GetAllForSprint`);
        const data: {
            data: IProject[];
            error: boolean;
            message: string;
        } = await res.json();
        return data;
    }    ,
    GET_ONE: async (id: string | string[]) => {
        try {
            const res = await fetch(`${url}/projects/${id}`);
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
            const res = await fetch(`${url}/projects/${id}/details`);
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
                const res = await fetch(`${url}/projects/${projectId}/units`, {
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
    YEARS: {
        GET_ALL: async () => {
            try {
                const res = await fetch(`${url}/projects/years`);
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

import { ITaskBank } from "../../pages/schemas/task-bank";
import { url, CommonResponse, BasicInfo } from "./";

type Node = {
    schemaId: number;
    name: string;
    isStart: boolean;
    previous: number[];
    requires: number[];
};

export interface UnarchivableSchemaResponse {
    name: string;
    id: number;
    units: {
        name: string;
        id: number;
        lessons: {
            name: string;
            id: number;
            learningObjectives: {
                name: string;
                id: number;
            }[];
        }[];
    }[];
}

const SCHEMAS = {
    TASK_BANK: {
        DELETE: async (id: number) => {
            try {
                const res = await fetch(`${url}/schemas/task-bank/${id}`, {
                    method: "DELETE",
                });
                const data: CommonResponse = await res.json();
                return data;
            } catch (error) {
                console.error(error);
                return false;
            }
        },
        EDIT: async (
            id: string | string[],
            request: {
                name: string;
                tl: boolean;
                type: number;
                group: number;
                duration: number;
            }
        ) => {
            try {
                const res = await fetch(`${url}/schemas/task-bank/${id}`, {
                    method: "PATCH",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify(request),
                });
                const data: ITaskBank = await res.json();
                return data;
            } catch (error) {
                console.error(error);
                return false;
            }
        },
        ADD: async (request: {
            name: string;
            tl: boolean;
            type: number;
            group: number;
            duration: number;
        }) => {
            try {
                const res = await fetch(`${url}/schemas/task-bank`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify(request),
                });
                const data: ITaskBank = await res.json();
                return data;
            } catch (error) {
                console.error(error);
                return false;
            }
        },
        GET_ALL: async () => {
            try {
                const res = await fetch(`${url}/schemas/task-bank`);
                const data: ITaskBank[] = await res.json();
                return data;
            } catch (error) {
                console.error(error);
                return false;
            }
        },
    },
    DUPLICATE: async (id: number) => {
        try {
            const res = await fetch(`${url}/schemas/${id}/duplicate`, {
                method: "POST",
            });
            const data: {
                data: ISchema;
                error: boolean;
                message: string;
            } = await res.json();
            return data;
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    GET_ALL_MINI: async () => {
        try {
            const res = await fetch(`${url}/schemas/mini`);
            const data: BasicInfo[] = await res.json();
            return data;
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    GET_TYPES: async () => {
        try {
            const res = await fetch(`${url}/schemas/types`);
            const data: ResponseService<BasicInfo[]> = await res.json();
            return data;
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    GET_ALL: async () => {
        try {
            const res = await fetch(`${url}/schemas`);
            const data: ISchema[] = await res.json();
            return data;
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    CREATE: async ({
        name,
        description,
        typeId,
    }: {
        name: string;
        description: string;
        typeId?: number;
    }) => {
        try {
            const res = await fetch(`${url}/schemas`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ name, description, typeId }),
            });
            const data: {
                data: ISchema;
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
            const res = await fetch(`${url}/schemas/${id}`, {
                method: "DELETE",
            });
            const data:
                | {
                      error: false;
                      message: string;
                  }
                | {
                      error: true;
                      message: string;
                      data: UnarchivableSchemaResponse[];
                  } = await res.json();
            return data;
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    GET_ONE: async (id: string | string[]) => {
        try {
            const res = await fetch(`${url}/schemas/${id}`);
            const data: {
                data: {
                    description: string;
                    id: number;
                    name: string;
                    type?: { id: number; name: string };
                };
                error: boolean;
                message: string;
            } = await res.json();
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
        typeId,
    }: {
        id: number | string | string[];
        name: string;
        description: string;
        typeId?: number;
    }) => {
        try {
            const res = await fetch(`${url}/schemas/${id}`, {
                method: "PATCH",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ name, description, typeId }),
            });
            const data: {
                data: ISchema;
                error: boolean;
                message: string;
            } = await res.json();
            return data;
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    NODES: {
        DELETE_CHECK: async (nodeId: string | string[]) => {
            try {
                const res = await fetch(`${url}/nodes/${nodeId}/delete`, {
                    method: "OPTIONS",
                });
                const data: ResponseService<{
                    id: number;
                    name: string;
                    isSafeToDelete: boolean;
                }> = await res.json();
                return data;
            } catch (error) {
                console.error(error);
                return false;
            }
        },
        DOWN: async (id: number | string | string[]) => {
            try {
                const res = await fetch(`${url}/nodes/${id}/down`, {
                    method: "PATCH",
                });
                const data: INode[] = await res.json();
                return data;
            } catch (error) {
                console.error(error);
                return false;
            }
        },
        UP: async (id: number | string | string[]) => {
            try {
                const res = await fetch(`${url}/nodes/${id}/up`, {
                    method: "PATCH",
                });
                const data: INode[] = await res.json();
                return data;
            } catch (error) {
                console.error(error);
                return false;
            }
        },
        EDIT: async ({
            id,
            name,
            isStart,
            previous,
            requires,
        }: { id: number } & Node) => {
            try {
                const res = await fetch(`${url}/nodes/${id}`, {
                    method: "PATCH",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        name,
                        isStart,
                        previous,
                        requires,
                    }),
                });
                const data: INode = await res.json();
                return data;
            } catch (error) {
                console.error(error);
                return false;
            }
        },
        ADD: async ({ schemaId, name, isStart, previous, requires }: Node) => {
            try {
                const res = await fetch(`${url}/nodes/${schemaId}`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        name,
                        isStart,
                        previous,
                        requires,
                    }),
                });
                const data: INode = await res.json();
                return data;
            } catch (error) {
                console.error(error);
                return false;
            }
        },
        DELETE: async (id: number) => {
            try {
                const res = await fetch(`${url}/nodes/${id}`, {
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
        GET_ALL_MINI: async (schemaId: number) => {
            try {
                const res = await fetch(`${url}/nodes/${schemaId}/mini`);
                const data: BasicInfo[] = await res.json();
                return data;
            } catch (error) {
                console.error(error);
                return false;
            }
        },
        GET_ALL: async (schemaId: number) => {
            try {
                const res = await fetch(`${url}/nodes/${schemaId}`);
                const data: INode[] = await res.json();
                return data;
            } catch (error) {
                console.error(error);
                return false;
            }
        },
        STEPS: {
            ADD_ROLLBACK_POINT: async (
                stepId: number,
                rollbackPointId: number
            ) => {
                try {
                    const res = await fetch(
                        `${url}/steps/${stepId}/add-rollback-point`,
                        {
                            method: "PATCH",
                            headers: {
                                "Content-Type": "application/json",
                            },
                            body: JSON.stringify({
                                id: rollbackPointId,
                            }),
                        }
                    );
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
            REMOVE_ROLLBACK_POINT: async (
                stepId: number,
                rollbackPointId: number
            ) => {
                try {
                    const res = await fetch(
                        `${url}/steps/${stepId}/remove-rollback-point`,
                        {
                            method: "PATCH",
                            headers: {
                                "Content-Type": "application/json",
                            },
                            body: JSON.stringify({
                                id: rollbackPointId,
                            }),
                        }
                    );
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
            GET_ROLLBACK_POINTS: async (stepId: number) => {
                try {
                    const res = await fetch(
                        `${url}/steps/${stepId}/rollback-points`
                    );
                    const data: ResponseService<BasicInfo[]> = await res.json();
                    return data;
                } catch (error) {
                    console.error(error);
                    return false;
                }
            },
            AVAILABLE_ROLLBACK_OPTIONS: async (stepId: number) => {
                try {
                    const res = await fetch(
                        `${url}/steps/${stepId}/available-rollback-points`
                    );
                    const data: ResponseService<BasicInfo[]> = await res.json();
                    return data;
                } catch (error) {
                    console.error(error);
                    return false;
                }
            },
            UP: async (stepId: number | string | string[]) => {
                try {
                    const res = await fetch(`${url}/steps/${stepId}/up`, {
                        method: "PATCH",
                    });
                    const data: { error: boolean; message: string } =
                        await res.json();
                    return data;
                } catch (error) {
                    console.error(error);
                    return false;
                }
            },
            DOWN: async (stepId: number | string | string[]) => {
                try {
                    const res = await fetch(`${url}/steps/${stepId}/down`, {
                        method: "PATCH",
                    });
                    const data: { error: boolean; message: string } =
                        await res.json();
                    return data;
                } catch (error) {
                    console.error(error);
                    return false;
                }
            },
            GET_MULTIPLE: async (nodes: number[]) => {
                try {
                    const res = await fetch(
                        `${url}/steps/multiple?${nodes
                            .map((_) => `nodeId=${_}`)
                            .join("&")}`
                    );
                    const data: {
                        id: number;
                        name: string;
                        steps: BasicInfo[];
                    }[] = await res.json();
                    return data;
                } catch (error) {
                    console.error(error);
                    return false;
                }
            },
            ADD: async ({
                nodeId,
                taskBankItem,
                duration,
            }: {
                nodeId: string;
                taskBankItem: number;
                duration: number;
            }) => {
                try {
                    const res = await fetch(`${url}/steps/${nodeId}`, {
                        method: "POST",
                        headers: {
                            "Content-Type": "application/json",
                        },
                        body: JSON.stringify({
                            taskBankItem,
                            duration,
                        }),
                    });
                    const data: IStep = await res.json();
                    return data;
                } catch (error) {
                    console.error(error);
                    return false;
                }
            },
            UPDATE_PRIO: async ({
                stepId,
                priority,
            }: {
                stepId: string | number;
                priority: TaskPriority;
            }) => {
                try {
                    const res = await fetch(`${url}/steps/${stepId}/priority`, {
                        method: "PATCH",
                        headers: {
                            "Content-Type": "application/json",
                        },
                        body: JSON.stringify({
                            priority,
                        }),
                    });
                    const data: {
                        data: IStep;
                        error: boolean;
                        message: string;
                    } = await res.json();
                    return data;
                } catch (error) {
                    console.error(error);
                    return false;
                }
            },
            DELETE_CHECK: async (stepId: string | string[]) => {
                try {
                    const res = await fetch(`${url}/steps/${stepId}/delete`, {
                        method: "OPTIONS",
                    });
                    const data: ResponseService<{
                        id: number;
                        nodeId: number;
                        name: string;
                        isSafeToDelete: boolean;
                    }> = await res.json();
                    return data;
                } catch (error) {
                    console.error(error);
                    return false;
                }
            },
            EDIT: async ({
                stepId,
                taskBankItem,
                duration,
            }: {
                stepId: string;
                taskBankItem: number;
                duration: number;
            }) => {
                try {
                    const res = await fetch(`${url}/steps/${stepId}`, {
                        method: "PATCH",
                        headers: {
                            "Content-Type": "application/json",
                        },
                        body: JSON.stringify({
                            taskBankItem,
                            duration,
                        }),
                    });
                    const data: IStep = await res.json();
                    return data;
                } catch (error) {
                    console.error(error);
                    return false;
                }
            },
            REMOVE: async (id: number) => {
                try {
                    const res = await fetch(`${url}/steps/${id}`, {
                        method: "DELETE",
                    });
                    const data = await res.json();
                    return data;
                } catch (error) {
                    console.error(error);
                    return false;
                }
            },
        },
    },
};

export default SCHEMAS;

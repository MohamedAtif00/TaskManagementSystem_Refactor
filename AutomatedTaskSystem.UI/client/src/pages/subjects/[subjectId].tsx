import { useEffect, useState } from "react";
import Link from "next/link";
import Head from "next/head";
import { useRouter } from "next/router";
import PlusIcon from "../../assets/Icons/Plus";
import QueryButton from "../../components/button/queryButton";
import API from "../../lib/API";
import AddUnit from "../../components/forms/projects/addUnit";
import AddLesson from "../../components/forms/projects/addLesson";
import AddLearningObjective from "../../components/forms/projects/addLearningObjective";
import { useAppSelector } from "../../app/hooks";
import ProjectAssign from "../../components/forms/projects/projectAssign";
import ProjectUnassign from "../../components/forms/projects/projectUnassign";
import ProjectIcon from "../../assets/Icons/Project";
import EditUnit from "../../components/pageComponent/projects/editUnit";
import EditLesson from "../../components/pageComponent/projects/editLesson";
import Loader from "../../components/loader";
import EditLearningObjectiveForm from "../../components/forms/projects/editLearningObjective";

const LearningObjective = (
    props: LearningObjective & {
        remove: (id: number) => void;
        lightBg?: boolean;
    }
) => {
    const [deleting, setDeleting] = useState(false);
    const router = useRouter();

    return (
        <div
            className={`grid grid-cols-6 px-2 ${props.lightBg ? " bg-slate-100" : "bg-slate-200"
                }`}
            onMouseLeave={deleting ? () => setDeleting(false) : undefined}
        >
            <div className="bg-inherit">{props.name}</div>
            <div className="bg-inherit">{props.schema.name}</div>
            <div className="bg-inherit">{props.tag}</div>
            <div className="bg-inherit">{props.template}</div>
            <div className="bg-inherit">{props.environment}</div>
            <div className="bg-inherit flex justify-end">
                <Link
                    href={{
                        pathname: `/subjects/${router.query.subjectId}`,
                        query: {
                            "learning-objective": props.id,
                            form: "edit-learning-objective",
                        },
                    }}
                >
                    <button className="text-base text-blue-600 font-normal px-3 rounded flex items-center justify-center py-1 hover:underline">
                        Edit
                    </button>
                </Link>
                <button
                    className={`transition-all ease-out text-base gap-2 font-normal px-3 rounded flex items-center justify-center py-1 ${deleting ? "bg-rose-500 text-white" : "text-rose-500"
                        }`}
                    onClick={
                        deleting
                            ? () => props.remove(props.id)
                            : () => setDeleting(true)
                    }
                >
                    Delete
                </button>
            </div>
        </div>
    );
};

const Lesson = (
    props: Lesson & {
        removeLearningObjective: (id: number) => void;
        remove: (id: number) => void;
    }
) => {
    const router = useRouter();
    const [deleting, setDeleting] = useState(false);

    return (
        <div>
            <div
                onMouseLeave={deleting ? () => setDeleting(false) : undefined}
                className="flex justify-between bg-slate-300 p-2"
            >
                <h3 className="text-lg">{props.name}</h3>
                <div className="flex gap-2">
                    <button
                        className={`transition-all ease-out text-base gap-2 font-normal px-3 rounded flex items-center justify-center py-1 ${deleting
                                ? "bg-rose-500 text-white"
                                : "text-rose-500"
                            }`}
                        onClick={
                            deleting
                                ? () => props.remove(props.id)
                                : () => setDeleting(true)
                        }
                    >
                        Delete
                    </button>
                    <Link
                        href={{
                            pathname: `/subjects/${router.query.subjectId}`,
                            query: {
                                lesson: props.id,
                                form: "edit-lesson",
                            },
                        }}
                        className="text-base text-blue-600 font-normal px-3 rounded flex items-center justify-center py-1 hover:underline"
                    >
                        Edit
                    </Link>
                    <QueryButton
                        iconLeft
                        iconRight={false}
                        icon={<PlusIcon />}
                        text="Learning Objective"
                        url={{
                            pathname: `/subjects/${router.query.subjectId}`,
                            query: {
                                form: "learning-objective",
                                lessonId: props.id,
                            },
                        }}
                    />
                </div>
            </div>
            <div className="relative flex flex-col gap-1">
                <div className="grid px-2 grid-cols-6 sticky top-0 bg-slate-100">
                    <div>Name</div>
                    <div>Type</div>
                    <div>Tag</div>
                    <div>Template</div>
                    <div>Environment</div>
                    <div className="flex gap-2 justify-end ">Actions</div>
                </div>
                {props.learningObjectives.map((lo, i) => {
                    return (
                        <LearningObjective
                            remove={props.removeLearningObjective}
                            key={lo.id}
                            lightBg={i % 2 !== 0}
                            {...lo}
                        />
                    );
                })}
            </div>
        </div>
    );
};

const Unit = (
    props: Unit & {
        remove: (id: number) => void;
        removeLesson: (id: number) => void;
        removeLearningObjective: (id: number) => void;
    }
) => {
    const router = useRouter();
    const [deleting, setDeleting] = useState(false);

    return (
        <div className="overflow-hidden border border-solid border-slate-300 rounded-md">
            <div
                className="flex justify-between p-2"
                onMouseLeave={deleting ? () => setDeleting(false) : undefined}
            >
                <h3 className="text-xl">{props.name}</h3>
                <div className="flex gap-1">
                    <button
                        className={`transition-all ease-out text-base gap-2 font-normal px-3 rounded flex items-center justify-center py-1 ${deleting
                                ? "bg-rose-500 text-white"
                                : "text-rose-500"
                            }`}
                        onClick={
                            deleting
                                ? () => props.remove(props.id)
                                : () => setDeleting(true)
                        }
                    >
                        Delete
                    </button>
                    <Link
                        href={{
                            pathname: `/subjects/${router.query.subjectId}`,
                            query: {
                                unit: props.id,
                                form: "edit-unit",
                            },
                        }}
                        className="text-base text-blue-600 font-normal px-3 rounded flex items-center justify-center py-1 hover:underline"
                    >
                        Edit
                    </Link>
                    <QueryButton
                        iconLeft
                        iconRight={false}
                        icon={<PlusIcon />}
                        text="Lesson"
                        url={{
                            pathname: `/subjects/${router.query.subjectId}`,
                            query: {
                                form: "lesson",
                                unitId: props.id,
                            },
                        }}
                    />
                </div>
            </div>
            <div className="flex flex-col mt-2">
                {props.lessons.map((l) => (
                    <Lesson
                        removeLearningObjective={props.removeLearningObjective}
                        remove={props.removeLesson}
                        key={l.id}
                        {...l}
                    />
                ))}
            </div>
        </div>
    );
};

const Project = () => {
    const [project, setProject] = useState<ProjectDetails>();
    const router = useRouter();
    const auth = useAppSelector((s) => s.authSlice);
    const [activeUnit, setActiveUnit] = useState<Unit>();
    const [activeLesson, setActiveLesson] = useState<Lesson>();
    const [activeLO, setActiveLO] = useState<LearningObjective>();

    if (!auth.isAuth || (auth.role !== 0 && auth.role !== 4)) router.replace("/");

    useEffect(() => {
        if (router.query.subjectId)
            API.PROJECTS.GET_ONE_DETAILED(router.query.subjectId).then(
                (res) => {
                    if (res && !res.error) setProject(res.data);
                }
            );
    }, [router.query]);

    useEffect(() => {
        if (!project) return;
        const { form } = router.query;

        const loId = router.query["learning-objective"];
        if (loId && form === "edit-learning-objective") {
            const id = parseInt(loId.toString());
            if (!isNaN(id))
                for (const { lessons } of project.units)
                    for (const { learningObjectives } of lessons)
                        for (const lo of learningObjectives)
                            if (lo.id === id) return setActiveLO(lo);
        }

        const unitId = router.query["unit"];
        if (unitId && form === "edit-unit") {
            const id = parseInt(unitId.toString());
            if (!isNaN(id))
                for (const unit of project.units)
                    if (unit.id === id) return setActiveUnit(unit);
        }

        const lessonId = router.query["lesson"];
        if (lessonId && form === "edit-lesson") {
            const id = parseInt(lessonId.toString());
            if (!isNaN(id))
                for (const { lessons } of project.units)
                    for (const lesson of lessons)
                        if (lesson.id === id) return setActiveLesson(lesson);
        }
        setActiveLO(undefined);
        setActiveUnit(undefined);
        setActiveLesson(undefined);
    }, [router.query, project]);

    const handlers = {
        project: {
            assign: (userIds: number[], groupIds?: number[]) => {
                API.PROJECTS.ASSIGN({
                    projectId: router.query.subjectId!,
                    userIds,
                    groupIds
                }).then(() => {
                    router.push(`/subjects/${router.query.subjectId}`);
                });
            },
            unassign: (userIds: number[]) => {
                API.PROJECTS.UNASSIGN({
                    projectId: router.query.subjectId!,
                    userIds,
                }).then(() => {
                    router.push(`/subjects/${router.query.subjectId}`);
                });
            },
        },
        unit: {
            add: (name: string) => {
                if (project) {
                    API.PROJECTS.UNITS.ADD({
                        name,
                        projectId: project.id,
                    }).then((res) => {
                        if (res && !res.error) {
                            setProject((ps) => {
                                return {
                                    ...ps!,
                                    units: [...ps!.units, res.data],
                                };
                            });
                            router.push(`/subjects/${router.query.subjectId}`);
                        }
                    });
                }
            },
            remove: (id: number) => {
                if (project) {
                    API.PROJECTS.UNITS.REMOVE(id).then((res) => {
                        if (res) {
                            setProject((ps) => {
                                const newUnits: Unit[] = [];
                                ps!.units.forEach((u) => {
                                    if (u.id == id) {
                                        return;
                                    }
                                    newUnits.push(u);
                                });
                                return { ...ps!, units: newUnits };
                            });
                        }
                    });
                }
            },
            edit: (id: number, name: string) => {
                if (project) {
                    API.PROJECTS.UNITS.EDIT({ id, name }).then((res) => {
                        if (res && !res.error) {
                            setProject((ps) => {
                                const newUnits: Unit[] = [];
                                ps!.units.forEach((u) => {
                                    if (u.id == res.data.id)
                                        return newUnits.push({
                                            ...u,
                                            name: res.data.name,
                                        });
                                    newUnits.push(u);
                                });
                                return { ...ps!, units: newUnits };
                            });
                            router.push(`/subjects/${project.id}`);
                        }
                    });
                }
            },
        },
        lesson: {
            add: (name: string) => {
                const unitId = router.query.unitId;
                if (unitId) {
                    const id = parseInt(unitId.toString());
                    !isNaN(id) &&
                        API.PROJECTS.UNITS.LESSONS.ADD({
                            name,
                            unitId: id,
                        }).then((res) => {
                            if (res) {
                                setProject((ps) => {
                                    const units: Unit[] = [];
                                    ps!.units.forEach((u) => {
                                        if (u.id == id) {
                                            u.lessons.push(res);
                                        }
                                        units.push(u);
                                    });
                                    return { ...ps!, units };
                                });
                                router.push(
                                    `/subjects/${router.query.subjectId}`
                                );
                            }
                        });
                }
            },
            edit: (id: number, name: string) => {
                if (project) {
                    API.PROJECTS.UNITS.LESSONS.EDIT({ id, name }).then(
                        (res) => {
                            if (res && !res.error) {
                                setProject((ps) => {
                                    const newUnits: Unit[] = [];
                                    ps!.units.forEach((u) => {
                                        const lesson = u.lessons.find(
                                            (l) => l.id == res.data.id
                                        );
                                        if (lesson) lesson.name = res.data.name;
                                        newUnits.push(u);
                                    });
                                    return { ...ps!, units: newUnits };
                                });
                                router.push(`/subjects/${project.id}`);
                            }
                        }
                    );
                }
            },
            remove: (id: number) => {
                API.PROJECTS.UNITS.LESSONS.REMOVE(id).then((res) => {
                    if (res) {
                        setProject((ps) => {
                            const newUnits: Unit[] = [];
                            ps!.units.forEach((u) => {
                                const newLessons: Lesson[] = [];
                                u.lessons.forEach((l) => {
                                    if (l.id == id) {
                                        return;
                                    }
                                    newLessons.push(l);
                                });
                                newUnits.push({ ...u, lessons: newLessons });
                            });
                            return { ...ps!, units: newUnits };
                        });
                    }
                });
            },
        },
        learningObjective: {
            add: ({
                name,
                schemaId,
                environment,
                tag,
                template,
            }: {
                name: string;
                schemaId: number;
                tag: string;
                template: string;
                environment: string;
            }) => {
                const lessonId = router.query.lessonId;
                if (lessonId) {
                    const id = parseInt(lessonId.toString());
                    API.PROJECTS.UNITS.LESSONS.LEARNING_OBJECTIVES.CREATE({
                        lessonId: id,
                        name,
                        schemaId,
                        environment,
                        tag,
                        template,
                    }).then((res) => {
                        if (res) {
                            // setProject((ps) => {
                            //     const units: Unit[] = [];
                            //     ps!.units.forEach((u) => {
                            //         u.lessons.forEach((l) => {
                            //             if (l.id == id) {
                            //                 l.learningObjectives.push(res);
                            //             }
                            //         });
                            //         units.push(u);
                            //     });
                            //     return { ...ps!, units };
                            // });
                            //
                            router.push(`/subjects/${project!.id}`);
                        }
                    });
                }
            },
            edit: async (params: LearningObjective) => {
                setProject((ps) => {
                    const newUnits: Unit[] = [];
                    ps!.units.forEach((u) => {
                        const newLessons: Lesson[] = [];
                        u.lessons.forEach((l) => {
                            const newLOs: LearningObjective[] = [];
                            l.learningObjectives.forEach((lo) => {
                                if (lo.id == params.id)
                                    return newLOs.push(params);
                                newLOs.push(lo);
                            });
                            newLessons.push({
                                ...l,
                                learningObjectives: newLOs,
                            });
                        });
                        newUnits.push({
                            ...u,
                            lessons: newLessons,
                        });
                    });
                    return { ...ps!, units: newUnits };
                });
                router.push(`/subjects/${project!.id}`);
            },
            remove: (id: number) => {
                API.PROJECTS.UNITS.LESSONS.LEARNING_OBJECTIVES.REMOVE(id).then(
                    (res) => {
                        if (res) {
                            setProject((ps) => {
                                const newUnits: Unit[] = [];
                                ps!.units.forEach((u) => {
                                    const newLessons: Lesson[] = [];
                                    u.lessons.forEach((l) => {
                                        const newLOs: LearningObjective[] = [];
                                        l.learningObjectives.forEach((lo) => {
                                            if (lo.id == id) {
                                                return;
                                            }
                                            newLOs.push(lo);
                                        });
                                        newLessons.push({
                                            ...l,
                                            learningObjectives: newLOs,
                                        });
                                    });
                                    newUnits.push({
                                        ...u,
                                        lessons: newLessons,
                                    });
                                });
                                return { ...ps!, units: newUnits };
                            });
                        }
                    }
                );
            },
            assignment: (
                id: string | string[],
                type: "assign" | "unassign",
                userIds: number[]
            ) => {
                if (type == "assign")
                    API.PROJECTS.UNITS.LESSONS.LEARNING_OBJECTIVES.ASSIGN(
                        id,
                        userIds
                    ).then((res) => {
                        if (res) {
                            setProject((ps) => {
                                const newUnits: Unit[] = [];
                                ps!.units.forEach((u) => {
                                    const newLessons: Lesson[] = [];
                                    u.lessons.forEach((l) => {
                                        const newLOs: LearningObjective[] = [];
                                        l.learningObjectives.forEach((lo) => {
                                            if (lo.id.toString() === id) {
                                                return newLOs.push(res);
                                            }
                                            newLOs.push(lo);
                                        });
                                        newLessons.push({
                                            ...l,
                                            learningObjectives: newLOs,
                                        });
                                    });
                                    newUnits.push({
                                        ...u,
                                        lessons: newLessons,
                                    });
                                });
                                return { ...ps!, units: newUnits };
                            });
                        }
                        router.push(`/subjects/${project!.id}`);
                    });
                else
                    return API.PROJECTS.UNITS.LESSONS.LEARNING_OBJECTIVES.UNASSIGN(
                        id,
                        userIds
                    ).then((res) => {
                        if (res) {
                            setProject((ps) => {
                                const newUnits: Unit[] = [];
                                ps!.units.forEach((u) => {
                                    const newLessons: Lesson[] = [];
                                    u.lessons.forEach((l) => {
                                        const newLOs: LearningObjective[] = [];
                                        l.learningObjectives.forEach((lo) => {
                                            if (lo.id.toString() === id) {
                                                return newLOs.push(res);
                                            }
                                            newLOs.push(lo);
                                        });
                                        newLessons.push({
                                            ...l,
                                            learningObjectives: newLOs,
                                        });
                                    });
                                    newUnits.push({
                                        ...u,
                                        lessons: newLessons,
                                    });
                                });
                                return { ...ps!, units: newUnits };
                            });
                        }
                    });
            },
        },
    };

    if (project === undefined)
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Head>
                    <title>ATS - Loading</title>
                </Head>
                <Loader />
            </div>
        );

    return (
        <div className="w-full px-4">
            <Head>
                <title>ATS - {project.name}</title>
            </Head>
            <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
                <div className="flex items-center gap-3">
                    <div className="flex">
                        <div className="w-6 h-6">
                            <ProjectIcon color={"#29313d"} />
                        </div>
                    </div>
                    <div className="text-2xl font-bold text-slate-800">
                        {project.name}
                    </div>
                    <div className="text-slate-500">
                        {project.status === 0
                            ? "Open"
                            : project.status === 1
                                ? "Closed"
                                : project.status === 2
                                    ? "Hold"
                                    : "Reopened"}
                    </div>
                </div>
                <div className="flex gap-2">
                    <QueryButton
                        icon={<PlusIcon />}
                        text="Remove Users"
                        url={{
                            pathname: `/subjects/${project.id}`,
                            query: {
                                form: "unassign",
                            },
                        }}
                    />
                    <QueryButton
                        icon={<PlusIcon />}
                        text="Assign Users"
                        url={{
                            pathname: `/subjects/${project.id}`,
                            query: {
                                form: "assign",
                            },
                        }}
                    />
                </div>
            </div>
            <div className="py-4 flex flex-col gap-4">
                <div className="flex justify-between">
                    <h2 className="text-3xl">Units</h2>
                    <QueryButton
                        iconLeft
                        iconRight={false}
                        icon={<PlusIcon />}
                        text="Unit"
                        url={{
                            pathname: `/subjects/${project.id}`,
                            query: {
                                form: "unit",
                            },
                        }}
                    />
                </div>
                {project.units.map((u) => (
                    <Unit
                        key={u.id}
                        {...u}
                        removeLesson={handlers.lesson.remove}
                        removeLearningObjective={
                            handlers.learningObjective.remove
                        }
                        remove={handlers.unit.remove}
                    />
                ))}
            </div>
            <>
                <AddUnit
                    path={`/subjects/${project.id}`}
                    submit={handlers.unit.add}
                />
                <AddLesson
                    path={`/subjects/${project.id}`}
                    submit={handlers.lesson.add}
                />
                <AddLearningObjective
                    path={`/subjects/${project.id}`}
                    submit={handlers.learningObjective.add}
                />
                <ProjectAssign handler={handlers.project.assign} />
                <ProjectUnassign handler={handlers.project.unassign} />
                {activeLO && (
                    <EditLearningObjectiveForm
                        projectId={project.id}
                        update={handlers.learningObjective.edit}
                        learningObjective={activeLO}
                    />
                )}
                {activeUnit && (
                    <EditUnit
                        id={activeUnit.id}
                        name={activeUnit.name}
                        projectId={project.id}
                        onSubmit={handlers.unit.edit}
                    />
                )}
                {activeLesson && (
                    <EditLesson
                        id={activeLesson.id}
                        name={activeLesson.name}
                        projectId={project.id}
                        onSubmit={handlers.lesson.edit}
                    />
                )}
            </>
        </div>
    );
};

export default Project;

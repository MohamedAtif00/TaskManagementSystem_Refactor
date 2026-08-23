import Link from "next/link";
import { useEffect, useReducer, useState } from "react";
import CustomizedCombobox from "../../formComponents/Combobox";
import API from "../../../lib/API";
import EditLoSchemaForm from "./learning-objective/editSchema";

interface State {
    schema: BasicInfo;
    template: string;
    environment: string;
    tag: string;
    name: string;
}

type StringActions = {
    type: "update-env" | "update-tag" | "update-name" | "update-template";
    payload: string;
};

interface UpdateSchemaAction {
    type: "update-schema";
    payload: BasicInfo;
}

interface UpdateStepsAction {
    type: "update-steps";
    payload: number[];
}

const reducer = (
    state: State,
    action:
        | StringActions
        | UpdateSchemaAction
        | UpdateStepsAction
        | { type: "reset" }
): State => {
    switch (action.type) {
        case "update-name":
            return { ...state, name: action.payload };
        case "update-env":
            return { ...state, environment: action.payload };
        case "update-template":
            return { ...state, template: action.payload };
        case "update-tag":
            return { ...state, tag: action.payload };
        case "update-schema":
            return { ...state, schema: action.payload };
        default:
            throw Error("");
    }
};

interface Props {
    learningObjective: LearningObjective;
    update: (param: LearningObjective) => void;
    projectId: number;
}

const EditLearningObjectiveForm: React.FC<Props> = (props) => {
    const [state, dispatch] = useReducer(reducer, {
        schema: {
            name: props.learningObjective.schema.name,
            id: props.learningObjective.schema.id,
        },
        template: props.learningObjective.template,
        environment: props.learningObjective.environment,
        tag: props.learningObjective.tag,
        name: props.learningObjective.name,
    });
    const [schemaOptions, setSchemaOptions] = useState<BasicInfo[]>([]);
    const [schemaChange, setSchemaChange] = useState(false);
    const [editSchema, setEditSchema] = useState(false);

    useEffect(() => {
        API.SCHEMAS.GET_ALL_MINI().then((res) => res && setSchemaOptions(res));
    }, []);

    useEffect(() => {
        if (state.schema.id !== props.learningObjective.schema.id)
            return setSchemaChange(true);
        setSchemaChange(false);
    }, [state, props]);

    const handleSubmit: React.FormEventHandler<HTMLFormElement> = (e) => {
        e.preventDefault();
        API.PROJECTS.UNITS.LESSONS.LEARNING_OBJECTIVES.EDIT(
            props.learningObjective.id,
            {
                name: state.name,
                tag: state.tag,
                environment: state.environment,
                template: state.template,
                schemaId: state.schema.id,
                steps: [],
            }
        ).then((res) => {
            if (res) props.update(res);
        });
    };

    if (editSchema)
        return (
            <EditLoSchemaForm
                back={() => setEditSchema(false)}
                schemaId={state.schema.id}
				state={state}
				update={props.update}
				loId={props.learningObjective.id}
            />
        );

    return (
        <div className="z-30 fixed top-0 left-0 right-0 bottom-0 flex items-center justify-center bg-black/30">
            <div className="rounded-md bg-white border border-solid border-slate-300">
                <h2 className="font-bold text-lg px-8 py-4 border-b border-solid border-slate-300">
                    Edit Learning Objective
                </h2>
                <form onSubmit={handleSubmit} className="flex flex-col gap-2">
                    <div className="px-8 pt-2 pb-1 w-96">
                        <label>
                            <div className="pl-2 text-sm">Name:</div>
                            <input
                                type="text"
                                value={state.name}
                                className="border border-solid border-slate-300 w-full px-3 py-2 rounded-lg text-sm mt-1"
                                onChange={(e) =>
                                    dispatch({
                                        type: "update-name",
                                        payload: e.target.value,
                                    })
                                }
                            />
                        </label>
                    </div>
                    <div className="px-8 py-1 w-96">
                        <div className="pl-2 text-sm">Schema:</div>
                        <CustomizedCombobox
                            value={state.schema}
                            onChange={(e) =>
                                dispatch({ type: "update-schema", payload: e })
                            }
                            options={schemaOptions}
                        />
                    </div>
                    <div className="px-8 py-1 w-96">
                        <label>
                            <div className="pl-2 text-sm">Tag:</div>
                            <input
                                type="text"
                                value={state.tag}
                                className="border border-solid border-slate-300 w-full px-3 py-2 rounded-lg text-sm mt-1"
                                onChange={(e) =>
                                    dispatch({
                                        type: "update-tag",
                                        payload: e.target.value,
                                    })
                                }
                            />
                        </label>
                    </div>
                    <div className="px-8 py-1 w-96">
                        <label>
                            <div className="pl-2 text-sm">Template:</div>
                            <input
                                type="text"
                                value={state.template}
                                className="border border-solid border-slate-300 w-full px-3 py-2 rounded-lg text-sm mt-1"
                                onChange={(e) =>
                                    dispatch({
                                        type: "update-template",
                                        payload: e.target.value,
                                    })
                                }
                            />
                        </label>
                    </div>
                    <div className="px-8 py-1 w-96">
                        <label>
                            <div className="pl-2 text-sm">Environment:</div>
                            <input
                                type="text"
                                value={state.environment}
                                className="border border-solid border-slate-300 w-full px-3 py-2 rounded-lg text-sm mt-1"
                                onChange={(e) =>
                                    dispatch({
                                        type: "update-env",
                                        payload: e.target.value,
                                    })
                                }
                            />
                        </label>
                    </div>
                    <div className="grid grid-cols-2 gap-4 px-8 pt-2 pb-4">
                        <Link href={`/subjects/${props.projectId}`}>
                            <button
                                className="w-full py-2 bg-black text-white border-2 border-solid border-white/30"
                                type="button"
                            >
                                Cancel
                            </button>
                        </Link>
                        {schemaChange ? (
                            <button
                                type="button"
                                className="bg-blue-500 border-2 border-solid border-blue-300 py-2 text-white"
                                onClick={() => setEditSchema(true)}
                            >
                                Next
                            </button>
                        ) : (
                            <button
                                type="submit"
                                className="bg-blue-500 border-2 border-solid border-blue-300 py-2 text-white"
                            >
                                Save
                            </button>
                        )}
                    </div>
                </form>
            </div>
        </div>
    );
};

export default EditLearningObjectiveForm;

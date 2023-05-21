import Link from "next/link";
import { useRouter } from "next/router";
import { useState } from "react";
import { useAppDispatch, useAppSelector } from "../../app/hooks";
import PlusIcon from "../../assets/Icons/Plus";
import API from "../../lib/API";
import { removeStep } from "../../slices/nodesSlice";
import QueryButton from "../button/queryButton";
import styles from "./styles.module.scss";

interface Props {
    schemaId: number;
    id: number;
    name: string;
    previous: { name: string; id: number }[];
    requires: { name: string; id: number }[];
    isStart: boolean;
    steps: IStep[];
    updateNodes: () => void;
}

const NodeItem = ({
    schemaId,
    id,
    name,
    isStart,
    previous,
    requires,
    steps,
    updateNodes,
}: Props) => {
    const auth = useAppSelector((s) => s.authSlice);

    const handleDelete = () =>
        API.SCHEMAS.NODES.DELETE(id).then((res) => res && updateNodes());

    return (
        <div className={styles.node}>
            <div className={styles.nodeTitle}>
                <div>{name}</div>
                {auth.role == 1 ? (
                    <div className="flex gap-2">
                        <div>
                            <button
                                onClick={handleDelete}
                                className="text-base gap-2 font-normal px-3 rounded text-rose-500 flex items-center justify-center py-1 hover:text-white hover:bg-rose-500 transition-all ease-out"
                            >
                                Delete
                            </button>
                        </div>
                        <QueryButton
                            text="Edit"
                            url={{
                                pathname: `/schemas/${schemaId}`,
                                query: {
                                    form: "nodeEdit",
                                    nodeId: id,
                                },
                            }}
                        />
                        <QueryButton
                            icon={<PlusIcon />}
                            text="Step"
                            url={{
                                pathname: `/schemas/${schemaId}`,
                                query: {
                                    form: "step",
                                    nodeId: id,
                                },
                            }}
                            iconLeft
                            iconRight={false}
                        />
                    </div>
                ) : (
                    ""
                )}
            </div>
            <div>
                {isStart ? (
                    <div>Start Point</div>
                ) : (
                    <>
                        <div>Previous: {previous.map((p) => `${p.name} `)}</div>
                        {requires.length !== 0 ? (
                            <div>
                                Requires: {requires.map((r) => `${r.name} `)}
                            </div>
                        ) : (
                            ""
                        )}
                    </>
                )}
            </div>
            <div>
                <div>Steps</div>
                <table className={styles.steps}>
                    <thead>
                        <tr className={styles.tableHead}>
                            <th>Review</th>
                            <th>Team Leader</th>
                            <th>Name</th>
                            <th>Groups</th>
                            <th>Duration</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        {steps.map((s) => {
                            return (
                                <Step
                                    key={s.id}
                                    nodeId={id}
                                    TL={s.tl}
                                    group={s.group}
                                    id={s.id}
                                    name={s.name}
                                    reviewable={s.reviewable}
                                    duration={s.duration}
                                />
                            );
                        })}
                    </tbody>
                </table>
            </div>
        </div>
    );
};

const Step = ({
    nodeId,
    id,
    TL,
    group,
    name,
    reviewable,
    duration
}: {
    nodeId: number;
    id: number;
    TL: boolean;
    reviewable: boolean;
    name: string;
    group: { id: number; name: string };
    duration: number;
}) => {
    const router = useRouter();
    const [deleting, setDeleting] = useState(false);
    const [editing, setEditng] = useState(false);
    const dispatch = useAppDispatch();
    const auth = useAppSelector((s) => s.authSlice);

    const rest =
        deleting && editing
            ? () => {
                setDeleting(false);
                setEditng(false);
            }
            : deleting
                ? () => setDeleting(false)
                : editing
                    ? () => setEditng(false)
                    : undefined;

    return (
        <tr key={id} className={styles.step} onMouseLeave={rest}>
            <td>{reviewable ? "True" : ""}</td>
            <td>{TL ? "True" : ""}</td>
            <td className={styles.name}>{name}</td>
            <td>{group.name}</td>
            <td>{Math.floor(duration / 60)}:{duration % 60 < 10 ? `0${duration % 60}` : duration % 60}</td>
            <td>
                {auth.role == 1 ? (
                    <>
                        <Link
                            href={`${router.asPath}?form=stepEdit&stepId=${id}`}
                        >
                            <div
                                className={[
                                    styles.edit,
                                    editing ? styles.active : "",
                                ].join(" ")}
                                onClick={() => {
                                    if (editing) {
                                        // API.SCHEMAS.NODES.STEPS.REMOVE(id).then((res) => {
                                        // 	if (res) {
                                        // 		dispatch(
                                        // 			removeStep({ stepId: id, nodeId })
                                        // 		);
                                        // 	}
                                        // });
                                    } else {
                                        setEditng(true);
                                    }
                                }}
                            >
                                Edit
                            </div>
                        </Link>
                        <div
                            className={[
                                styles.delete,
                                deleting ? styles.active : "",
                            ].join(" ")}
                            onClick={() => {
                                if (deleting) {
                                    API.SCHEMAS.NODES.STEPS.REMOVE(id).then(
                                        (res) => {
                                            if (res) {
                                                dispatch(
                                                    removeStep({
                                                        stepId: id,
                                                        nodeId,
                                                    })
                                                );
                                            }
                                        }
                                    );
                                } else
                                    setDeleting(true);
                            }}
                        >
                            Delete
                        </div>
                    </>
                ) : (
                    ""
                )}
            </td>
        </tr>
    );
};

export default NodeItem;

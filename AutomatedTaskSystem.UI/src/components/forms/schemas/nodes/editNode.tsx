import styles from "../../styles.module.scss";
import Backdrop from "../../backdrop";
import React, { useEffect, useState } from "react";
import FormField from "../../field";
import SelectNodesWithToggle from "../../selectListWithToggle";
import API from "../../../../lib/API";
import { useRouter } from "next/router";

interface INodeLocal {
    id: number;
    name: string;
}

const EditNode = ({
    schemaId,
    node,
    nodes,
    updateList,
}: {
    schemaId: number;
    node: INode;
    nodes: INodeLocal[];
    updateList: () => void;
}) => {
    const [submittable, setSubmittable] = useState(false);
    const [name, setName] = useState(node.name);
    const [start, setStart] = useState(node.isStart);
    const [previous, setPrevious] = useState<number[]>(
        node.previous.map((n) => n.id)
    );
    const router = useRouter();

    const updatePrevious = (nodeId: number) => {
        const foundIndex = previous.findIndex((_n) => _n === nodeId);
        if (foundIndex === -1) {
            return setPrevious((ps) => [...ps, nodeId]);
        }
        return setPrevious((ps) => [
            ...ps.slice(0, foundIndex),
            ...ps.slice(foundIndex + 1),
        ]);
    };


    const updateStart = () => {
        setStart((ps) => !ps);
        if (!start)
            setPrevious([]);
    };

    useEffect(() => {
        if (name === "") setSubmittable(false);
        else if (!start && previous.length === 0) setSubmittable(false);
        else setSubmittable(true);
    }, [start, previous, name]);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (submittable)
            API.SCHEMAS.NODES.EDIT({
                id: node.id,
                name,
                isStart: start,
                previous,
                schemaId,
            }).then((res) => {
                if (res) {
                    updateList();
                    router.push(`/schemas/${schemaId}`);
                }
            });
    };

    return (
        <Backdrop mainRoute={`/schemas/${schemaId}`}>
            <div
                className={[styles.form, styles.center].join(" ")}
                onClick={(e) => e.stopPropagation()}
            >
                <form onSubmit={handleSubmit}>
                    <div className={styles.inputs}>
                        <FormField
                            label="Name"
                            onChange={setName}
                            value={name}
                        />
                        <SelectNodesWithToggle
                            label="Previous"
                            list={nodes}
                            updateList={updatePrevious}
                            selected={previous}
                            checkbox={start}
                            toggle={updateStart}
                        />
                    </div>
                    <div>
                        <input
                            type="submit"
                            value="Save"
                            className={[
                                styles.submit,
                                !submittable ? styles.inactive : "",
                            ].join(" ")}
                        />
                    </div>
                </form>
            </div>
        </Backdrop>
    );
};

export default EditNode;

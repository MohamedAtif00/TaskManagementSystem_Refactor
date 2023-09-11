import { useState } from "react";
import NodeRightArrow from "./NodeRightArrow";
import NodeCard from "./nodeCard";

interface NodeCardFocus {
    id: number;
    type: "current" | "previous" | "parallel";
}

interface Props {
    nodes: NodeAhead[];
	updateSelected: (value: NodeAhead[]) => void
}

const NodeCards: React.FC<Props> = ({  nodes, updateSelected }) => {
    const [mainNode, setMainNode] = useState<NodeAhead>();
    const [focus, setFocus] = useState<NodeCardFocus[]>([]);

    const handleNodeCardHover = (value: number) => {
        const newState: NodeCardFocus[] = [];

        const foundPoint = nodes.find((p) => p.id === value);

        if (!foundPoint) return;

        const tohandle: number[] = [];
        newState.push({
            id: foundPoint.id,
            type: "current",
        });
        foundPoint.previousNodes.forEach((p) => {
            tohandle.push(p.id);
        });
        while (tohandle.length > 0) {
            const id = tohandle.pop();

            const foundPoint = nodes.find((p) => p.id === id);
            if (!foundPoint) continue;

            newState.push({
                id: foundPoint.id,
                type: "previous",
            });

            foundPoint.previousNodes.forEach((p) => {
                tohandle.push(p.id);
            });
        }

        newState.forEach((p) => {
            if (p.type !== "previous") return;

            const foundPoint = nodes.find((_) => _.id == p.id);

            foundPoint?.nextNodes.forEach((p) => {
                const foundPoint = nodes.find((_) => _.id == p.id);

                foundPoint &&
                    !newState.some((_) => _.id === foundPoint.id) &&
                    newState.push({
                        type: "parallel",
                        id: foundPoint.id,
                    });
            });
        });

        setFocus(newState);
    };

    const handleOnClick = () => {
        // setSelectedSteps([]);
        const filtered = focus.filter(
            (p) => p.type === "current" || p.type === "parallel"
        );
        const newState: NodeAhead[] = [];
        filtered.forEach((point) => {
            const foundPoint = nodes.find((p) => p.id === point.id);

            if (point.type === "current") setMainNode(foundPoint);

            foundPoint &&
                !newState.includes(foundPoint) &&
                newState.push(foundPoint);
        });
        updateSelected(newState);
    };

    const handleNodeCardLeave = () =>
        mainNode ? handleNodeCardHover(mainNode.id) : setFocus([]);

    const cards = nodes.reduce((acc, current, i) => {
        const newAcc = [...acc];
        const pointType = focus.find((p) => p.id === current.id)?.type;
        i > 0 &&
            newAcc.push(
                <NodeRightArrow
                    isComplete={current.isComplete}
                    type={pointType}
                    key={`${current.id}-arrow`}
                />
            );
        newAcc.push(
            <NodeCard
                onClick={handleOnClick}
                type={pointType}
                onHover={handleNodeCardHover}
                onLeave={handleNodeCardLeave}
                name={current.name}
                isComplete={current.isComplete}
                id={current.id}
                key={current.id}
            />
        );
        return newAcc;
    }, [] as React.JSX.Element[]);

    return <div className="flex gap-4 items-center px-6 py-2">{cards}</div>;
};

export default NodeCards;

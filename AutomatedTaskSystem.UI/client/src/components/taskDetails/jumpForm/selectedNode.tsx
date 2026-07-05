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

    const hasSamePreviousNodes = (
        a: BasicInfo[],
        b: BasicInfo[]
    ): boolean => {
        const aIds = a.map((p) => p.id).sort((x, y) => x - y);
        const bIds = b.map((p) => p.id).sort((x, y) => x - y);
        return (
            aIds.length === bIds.length &&
            aIds.every((id, i) => id === bIds[i])
        );
    };

    const isParallelWith = (
        a: NodeAhead,
        b: NodeAhead
    ): boolean => hasSamePreviousNodes(a.previousNodes, b.previousNodes);

    const handleNodeCardHover = (value: number) => {
        const newState: NodeCardFocus[] = [];

        const currentNode = nodes.find((p) => p.id === value);

        if (!currentNode) return;

        const hasFocus = (nodeId: number) =>
            newState.some((entry) => entry.id === nodeId);

        newState.push({
            id: currentNode.id,
            type: "current",
        });

        const toHandle = currentNode.previousNodes.map((p) => p.id);
        while (toHandle.length > 0) {
            const id = toHandle.pop();
            if (id === undefined || hasFocus(id)) continue;

            const ancestorNode = nodes.find((p) => p.id === id);
            if (!ancestorNode) continue;

            newState.push({
                id: ancestorNode.id,
                type: "previous",
            });

            ancestorNode.previousNodes.forEach((p) => {
                toHandle.push(p.id);
            });
        }

        // Side branches from ancestors (e.g. VO when hovering Publish) are
        // upstream work, not parallel — only same-level siblings are parallel.
        [...newState].forEach((entry) => {
            if (entry.type !== "previous") return;

            const ancestor = nodes.find((n) => n.id === entry.id);
            ancestor?.nextNodes.forEach((next) => {
                const branchNode = nodes.find((n) => n.id === next.id);
                if (
                    !branchNode ||
                    branchNode.id === currentNode.id ||
                    hasFocus(branchNode.id) ||
                    isParallelWith(branchNode, currentNode)
                ) {
                    return;
                }

                newState.push({
                    id: branchNode.id,
                    type: "previous",
                });
            });
        });

        nodes.forEach((node) => {
            if (
                node.id !== currentNode.id &&
                isParallelWith(node, currentNode)
            ) {
                newState.push({
                    type: "parallel",
                    id: node.id,
                });
            }
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

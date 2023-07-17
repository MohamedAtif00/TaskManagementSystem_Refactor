import { createSlice, PayloadAction } from "@reduxjs/toolkit";

const initialState: INode[] = [];

export const nodesSlice = createSlice({
    name: "nodes",
    initialState,
    reducers: {
        editStep: (state, action: PayloadAction<{ step: IStep }>) => {
            const newState: INode[] = [];

            state.forEach((n) => {
                const steps = n.steps.map((s) => {
                    if (s.id == action.payload.step.id)
                        return action.payload.step;
                    return s;
                });
                const newNode: INode = {
                    order: n.order,
                    id: n.id,
                    isStart: n.isStart,
                    name: n.name,
                    previous: n.previous,
                    requires: n.requires,
                    steps,
                };

                newState.push(newNode);
            });

            return newState;
        },
        addStep: (
            state,
            action: PayloadAction<{ id: number; step: IStep }>
        ) => {
            const newState: INode[] = [];

            state.forEach((n) => {
                const newNode: INode = {
                    id: n.id,
                    isStart: n.isStart,
                    name: n.name,
                    steps:
                        n.id === action.payload.id
                            ? [...n.steps, action.payload.step]
                            : n.steps,
                    previous: n.previous,
                    requires: n.requires,
                    order: n.order,
                };
                newState.push(newNode);
            });

            return newState;
        },
        removeStep: (
            state,
            action: PayloadAction<{ nodeId: number; stepId: number }>
        ) => {
            const newState: INode[] = [];

            state.forEach((n) => {
                const newSteps: IStep[] = [];
                n.steps.forEach((s) => {
                    if (
                        n.id === action.payload.nodeId &&
                        s.id === action.payload.stepId
                    ) {
                        return;
                    }
                    newSteps.push(s);
                });
                const newNode: INode = {
                    id: n.id,
                    isStart: n.isStart,
                    name: n.name,
                    steps: newSteps,
                    previous: n.previous,
                    requires: n.requires,
                    order: n.order,
                };
                newState.push(newNode);
            });

            return newState;
        },
        edit: (state, action: PayloadAction<INode>) => {
            const newNodes: INode[] = [];

            state.forEach((n) => {
                if (n.id === action.payload.id) {
                    return newNodes.push(action.payload);
                }
                newNodes.push(n);
            });

            return newNodes;
        },
        add: (state, action: PayloadAction<INode>) => {
            return [...state, action.payload];
        },
        load: (state, action: PayloadAction<INode[]>) => {
            return [...action.payload];
        },
        clear: () => {
            return initialState;
        },
    },
});

export const { add, load, edit, clear, addStep, editStep, removeStep } =
    nodesSlice.actions;

export default nodesSlice.reducer;

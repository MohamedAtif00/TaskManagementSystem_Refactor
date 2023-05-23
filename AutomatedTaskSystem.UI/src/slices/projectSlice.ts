import { createSlice, PayloadAction } from "@reduxjs/toolkit";

const initialState: IProject[] = [];

export const projectsSlice = createSlice({
	name: "projects",
	initialState,
	reducers: {
		add: (state, action: PayloadAction<IProject>) => {
			return [...state, action.payload];
		},
		load: (state, action: PayloadAction<IProject[]>) => {
			return (state = [...action.payload]);
		},
		clear: (state) => {
			return (state = initialState);
		},
		remove: (state, action: PayloadAction<IProject>) => {
			const newState: IProject[] = [];

			state.forEach(
				(p) => p.id !== action.payload.id && newState.push(p)
			);

			return newState;
		},
	},
});

export const { add, load, clear, remove } = projectsSlice.actions;

export default projectsSlice.reducer;

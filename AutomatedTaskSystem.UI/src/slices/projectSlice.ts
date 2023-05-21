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
	},
});

export const { add, load, clear } = projectsSlice.actions;

export default projectsSlice.reducer;

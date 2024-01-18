import { createSlice, PayloadAction } from "@reduxjs/toolkit";

const initialState: IGroup[] = [];

export const groupsSlice = createSlice({
	name: "groups",
	initialState,
	reducers: {
		edit: (state, action: PayloadAction<IGroup>) => {
			const newSteps: IGroup[] = [];

			state.forEach((s) => {
				if (s.id == action.payload.id)
					return newSteps.push(action.payload);
				newSteps.push(s);
			});

			return newSteps;
		},
		add: (state, action: PayloadAction<IGroup>) => {
			return [...state, action.payload];
		},
		load: (state, action: PayloadAction<IGroup[]>) => {
			return (state = [...action.payload]);
		},
		clear: (state) => {
			return (state = initialState);
		},
	},
});

export const { add, load, clear, edit } = groupsSlice.actions;

export default groupsSlice.reducer;

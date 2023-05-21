import { createSlice, PayloadAction } from "@reduxjs/toolkit";

const initialState: ITeam[] = [];

export const teamSlice = createSlice({
	name: "teams",
	initialState,
	reducers: {
		edit: (state, action: PayloadAction<ITeam>) => {
			const newTeams: ITeam[] = [];

			state.forEach((t) => {
				if (action.payload.id === t.id) {
					return newTeams.push(action.payload);
				}
				newTeams.push(t);
			});

			return newTeams;
		},
		add: (state, action: PayloadAction<ITeam>) => {
			return [...state, action.payload];
		},
		load: (state, action: PayloadAction<ITeam[]>) => {
			return (state = [...action.payload]);
		},
		clear: (state) => {
			return (state = initialState);
		},
	},
});

export const { add, load, clear, edit } = teamSlice.actions;

export default teamSlice.reducer;

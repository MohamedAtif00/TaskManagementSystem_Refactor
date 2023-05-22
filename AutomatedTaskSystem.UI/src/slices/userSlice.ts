import { createSlice, PayloadAction } from "@reduxjs/toolkit";

const initialState: IUser[] = [];

export const usersSlice = createSlice({
	name: "users",
	initialState,
	reducers: {
		edit: (state, action: PayloadAction<IUser>) => {
			const newState: IUser[] = [];

			state.forEach((u) =>
				newState.push(u.id === action.payload.id ? action.payload : u)
			);

			return newState;
		},
		add: (state, action: PayloadAction<IUser>) => {
			return [...state, action.payload];
		},
		load: (state, action: PayloadAction<IUser[]>) => {
			return (state = [...action.payload]);
		},
		clear: (state) => {
			return (state = initialState);
		},
	},
});

export const { add, load, clear, edit } = usersSlice.actions;

export default usersSlice.reducer;

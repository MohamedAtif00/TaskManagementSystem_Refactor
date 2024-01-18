import { createSlice, PayloadAction } from "@reduxjs/toolkit";

const initialState: IUser[] = [];

export const usersSlice = createSlice({
	name: "users",
	initialState,
	reducers: {
		add: (state, action: PayloadAction<IUser>) => {
			return [...state, action.payload];
		},
		clear: (state) => {
			return (state = initialState);
		},
		edit: (state, action: PayloadAction<IUser>) => {
			const newState: IUser[] = [];

			state.forEach((u) =>
				newState.push(u.id === action.payload.id ? action.payload : u)
			);

			return newState;
		},
		load: (state, action: PayloadAction<IUser[]>) => {
			return (state = [...action.payload]);
		},
		remove: (state, action: PayloadAction<IUser>) => {
			const newState: IUser[] = [];

			state.forEach(
				(u) => u.id !== action.payload.id && newState.push(u)
			);

			return newState;
		},
	},
});

export const { add, load, clear, edit, remove } = usersSlice.actions;

export default usersSlice.reducer;

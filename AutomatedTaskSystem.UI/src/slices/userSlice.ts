import { createSlice, PayloadAction } from "@reduxjs/toolkit";

const initialState: IUser[] = [];

export const usersSlice = createSlice({
	name: "users",
	initialState,
	reducers: {
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

export const { add, load, clear } = usersSlice.actions;

export default usersSlice.reducer;

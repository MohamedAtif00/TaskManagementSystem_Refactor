import { createSlice, PayloadAction } from "@reduxjs/toolkit";

const initialState: ISchema[] = [];

export const schemasSlice = createSlice({
	name: "schemas",
	initialState,
	reducers: {
		add: (state, action: PayloadAction<ISchema>) => {
			return [...state, action.payload];
		},
		load: (state, action: PayloadAction<ISchema[]>) => {
			return (state = [...action.payload]);
		},
		clear: (state) => {
			return (state = initialState);
		},
	},
});

export const { add, load, clear } = schemasSlice.actions;

export default schemasSlice.reducer;

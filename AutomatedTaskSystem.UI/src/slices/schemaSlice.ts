import { createSlice, PayloadAction } from "@reduxjs/toolkit";

const initialState: ISchema[] = [];

export const schemasSlice = createSlice({
	name: "schemas",
	initialState,
	reducers: {
		add: (state, action: PayloadAction<ISchema>) => {
			return [...state, action.payload];
		},
		clear: (state) => {
			return (state = initialState);
		},
		edit: (state, action: PayloadAction<ISchema>) => {
			const newState: ISchema[] = [];

			state.forEach((s) => {
				if (s.id === action.payload.id)
					return newState.push(action.payload);
				newState.push(s);
			});

			return newState;
		},
		load: (state, action: PayloadAction<ISchema[]>) => {
			return (state = [...action.payload]);
		},
		remove: (state, action: PayloadAction<ISchema>) => {
			const newState: ISchema[] = [];

			state.forEach((s) => {
				if (s.id === action.payload.id) return;
				newState.push(s);
			});

			return newState;
		},
	},
});

export const { add, load, clear, edit, remove } = schemasSlice.actions;

export default schemasSlice.reducer;

import { createSlice, PayloadAction } from "@reduxjs/toolkit";

type AuthState = {
	isAuth: boolean;
	name: string;
	role: number;
	group: string;
    id: number;
};

const initialState: AuthState = {
	isAuth: false,
	group: "",
	name: "",
	role: 0,
	id: 0,
};

export const authSlice = createSlice({
	name: "auth",
	initialState,
	reducers: {
		login: (
			state,
			action: PayloadAction<{
				name: string;
				role: number;
				id: number;
				group: string;
			}>
		) => {
			return {
				isAuth: true,
				name: action.payload.name,
				role: action.payload.role,
				group: action.payload.group,
                id: action.payload.id
			};
		},
		logout: () => {
			return {
				isAuth: false,
				name: "",
				role: 0,
				id: 0,
				group: "",
			};
		},
	},
});

export const { login, logout } = authSlice.actions;

export default authSlice.reducer;

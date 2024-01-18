import { createSlice, PayloadAction } from "@reduxjs/toolkit";

type AuthState = {
	isAuth: boolean;
	name: string;
	role: UserRole;
	group: string;
    id: number;
};

const initialState: AuthState = {
	isAuth: false,
	group: "",
	name: "",
	role: 3,
	id: 0,
};

export const authSlice = createSlice({
	name: "auth",
	initialState,
	reducers: {
		login: (
			{},
			action: PayloadAction<{
				name: string;
				role: UserRole;
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
			return initialState;
		},
	},
});

export const { login, logout } = authSlice.actions;

export default authSlice.reducer;

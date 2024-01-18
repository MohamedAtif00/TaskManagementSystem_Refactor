import { configureStore } from "@reduxjs/toolkit";
import groupsSlice from "../slices/groupSlice";
import schemasSlice from "../slices/schemaSlice";
import usersSlice from "../slices/userSlice";
import nodesSlice from "../slices/nodesSlice";
import projectSlice from "../slices/projectSlice";
import authSlice from "../slices/authSlice";
import teamSlice from "../slices/teamSlice";

export const store = configureStore({
	reducer: {
		authSlice,
		groupsSlice,
		usersSlice,
		schemasSlice,
		nodesSlice,
		projectSlice,
		teamSlice,
	},
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

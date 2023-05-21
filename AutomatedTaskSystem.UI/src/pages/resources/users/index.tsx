import { useRouter } from "next/router";
import { useEffect } from "react";
import { useAppDispatch, useAppSelector } from "../../../app/hooks";
import AddUser from "../../../components/forms/addUser";
import API from "../../../lib/API";
import { clear, load } from "../../../slices/userSlice";

const Users = () => {
	const users = useAppSelector((states) => states.usersSlice);
	const dispatch = useAppDispatch();
	const auth = useAppSelector((s) => s.authSlice);
	const router = useRouter();

	if (!auth.isAuth || auth.role != 1) {
		router.replace("/");
	}

	useEffect(() => {
		API.RESOURCES.USERS.GET_ALL().then((res) => {
			if (res && !res.error) {
				dispatch(load(res.data));
			}
		});
		return () => {
			dispatch(clear());
		};
	}, [dispatch]);

	return (
		<div className="max-h-screen overflow-y-auto">
			<AddUser />
		</div>
	);
};

export default Users;

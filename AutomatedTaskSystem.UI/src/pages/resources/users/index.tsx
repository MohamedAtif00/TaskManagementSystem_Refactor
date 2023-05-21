import { useRouter } from "next/router";
import { useEffect } from "react";
import { useAppDispatch, useAppSelector } from "../../../app/hooks";
import PlusIcon from "../../../assets/Icons/Plus";
import QueryButton from "../../../components/button/queryButton";
import AddUser from "../../../components/forms/addUser";
import Header from "../../../components/header/header";
import UserItem from "../../../components/userItem/userItem";
import API from "../../../lib/API";
import { clear, load } from "../../../slices/userSlice";
import styles from "../../../styles/resources.module.scss";

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
		<div className="container">
			<Header text="Users" icon="Resources">
				<QueryButton
					icon={<PlusIcon />}
					iconLeft
					iconRight={false}
					text="Add"
					url={{
						pathname: "/resources/users",
						query: {
							form: "user",
						},
					}}
				/>
			</Header>
			<div className={styles.container}>
				{users.map((g) => (
					<UserItem key={g.id} group={g.group.name} name={g.name} />
				))}
			</div>
			<AddUser />
		</div>
	);
};

export default Users;

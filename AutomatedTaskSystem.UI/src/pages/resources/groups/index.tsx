import QueryButton from "../../../components/button/queryButton";
import Header from "../../../components/header/header";
import PlusIcon from "../../../assets/Icons/Plus";
import AddGroup from "../../../components/forms/groups/addGroup";
import styles from "../../../styles/resources.module.scss";
import GroupItem from "../../../components/groupItem/groupItem";
import { useAppSelector, useAppDispatch } from "../../../app/hooks";
import { clear, load } from "../../../slices/groupSlice";
import { useEffect } from "react";
import API from "../../../lib/API";
import EditGroup from "../../../components/forms/groups/editGroup";
import { useRouter } from "next/router";

const Groups = () => {
	const groups = useAppSelector((state) => state.groupsSlice);
	const dispatch = useAppDispatch();
	const auth = useAppSelector((s) => s.authSlice);
	const router = useRouter();

	if (!auth.isAuth || auth.role != 1) {
		router.replace("/");
	}

	useEffect(() => {
		API.RESOURCES.GROUPS.GET_ALL().then((res) => {
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
			<Header text="Groups" icon="Resources">
				<QueryButton
					icon={<PlusIcon />}
					iconLeft
					iconRight={false}
					text="Add"
					url={{
						pathname: "/resources/groups",
						query: {
							form: "group",
						},
					}}
				/>
			</Header>
			<div className={styles.container}>
				{groups.map((g) => (
					<GroupItem
						id={g.id}
						key={g.id}
						color={g.colorCode}
						members={g.members}
						name={g.name}
					/>
				))}
			</div>
			<AddGroup />
			<EditGroup groups={groups} />
		</div>
	);
};

export default Groups;

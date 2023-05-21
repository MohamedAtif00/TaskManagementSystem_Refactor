import { useRouter } from "next/router";
import { useEffect } from "react";
import { useAppDispatch, useAppSelector } from "../../../app/hooks";
import PlusIcon from "../../../assets/Icons/Plus";
import QueryButton from "../../../components/button/queryButton";
import AddTeam from "../../../components/forms/addTeam";
import Header from "../../../components/header/header";
import TeamItem from "../../../components/teamItem";
import API from "../../../lib/API";
import { clear, load } from "../../../slices/teamSlice";
import styles from "../../../styles/resources.module.scss";

const Teams = () => {
	const teams = useAppSelector((state) => state.teamSlice);
	const dispatch = useAppDispatch();
	const auth = useAppSelector((s) => s.authSlice);
	const router = useRouter();

	if (!auth.isAuth || auth.role != 1) {
		router.replace("/");
	}

	useEffect(() => {
		API.RESOURCES.TEAMS.GET_ALL().then((res) => {
			if (res) {
				dispatch(load(res));
			}
		});
		return () => {
			dispatch(clear());
		};
	}, [dispatch]);

	return (
		<div className="container">
			<Header text="Teams" icon="Resources">
				<QueryButton
					icon={<PlusIcon />}
					iconLeft
					iconRight={false}
					text="Add"
					url={{
						pathname: "/resources/teams",
						query: {
							form: 'team'
						}
					}}
				/>
			</Header>
			<div className={styles.container}>
				{teams.map((t) => (
					<TeamItem
						id={t.id}
						key={t.id}
						members={t.members}
						name={t.name}
					/>
				))}
			</div>
			<AddTeam />
		</div>
	);
};

export default Teams;

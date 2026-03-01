import { useRouter } from "next/router";
import styles from "../styles.module.scss";
import Backdrop from "../backdrop";
import React, { useEffect, useState } from "react";
import API from "../../../lib/API";
import CrossIcon from "../../../assets/Icons/Cross";
import PlusIcon from "../../../assets/Icons/Plus";
import QueryButton from "../../button/queryButton";

interface Props {
	handler: (userIds: number[]) => void;
}

interface loUser {
	id: number;
	name: string;
	group: { id: number; name: string };
	role: UserRole;
}

enum ListType{
	USER,
	GROUP
}

const List = ({
	values,
	selected,
	handler,
	listType,
	users
}: {
	values: loUser[] | { id: number; name: string }[];
	selected: number[];
	handler: (id: number) => void;
	listType: ListType;
	users?: loUser[];
}) => {
	return (
		<div className={styles.listSelect}>
			<ul className={styles.add}>
				{values.map((u) => {
					 const isSelected = listType === ListType.GROUP && users
                        ? users.filter(usr => usr.group.id === u.id).some(usr => selected.includes(usr.id))
                        : selected.includes(u.id);
					return (
						<li
							key={u.id}
							className={isSelected ? styles.selected : ""}
							onClick={() => handler(u.id)}
						>
							<div>
								<div className={styles.name}>{u.name}</div>
								<div>{listType === ListType.USER ? (u as loUser).group.name : ''}</div>
							</div>
							<div>
								{isSelected ? (
									<CrossIcon className={styles.cross} />
								) : (
									<PlusIcon className={styles.plus} />
								)}
							</div>
						</li>
					);
				})}
			</ul>
		</div>
	);
};

const ProjectAssign = ({ handler }: Props) => {
	const router = useRouter();
	const [active, setActive] = useState(false);
	const [values, setValues] = useState<number[]>([]);
	const [users, setUsers] = useState<loUser[]>([]);
	const [groups, setGroups] = useState<{ id: number; name: string }[]>([]);
	const [listType, setListType] = useState<ListType>(ListType.USER);

	useEffect(() => {
		const _active =
			router.query.form === "assign" && router.query.projectId !== undefined;
		setActive(_active);
		if (!_active) {
			setValues([]);
		}
	}, [router]);

	useEffect(() => {
		const id = router.query.projectId;
		if (id) {
			API.PROJECTS.USERS_UNASSIGNED(id).then((res) => {
				if (res && !res.error) {
					const formattedUsers = res.data.map((user: IUser): loUser => ({
						...user,
						group: user.group ?? { id: 0, name: 'No Group' }, // or skip 'group' if it's optional in loUser
					}));
					setUsers(formattedUsers);
					// const formatedGroups = res.data.map((user: IUser): { id: number; name: string } => {
					// 	if(groups.find(g => g.id === user.group?.id)){
					// 		return ;
					// 	}
					// 	return {id: user.group?.id ?? 0, name: user.group?.name ?? 'No Group'};
					// });
					// setGroups(formatedGroups);

					setGroups(prevGroups => {
						const allPotentialGroups = [
							...prevGroups,
							...formattedUsers.map(user => ({
								id: user.group?.id ?? 0,
								name: user.group?.name ?? 'No Group'
							}))
						];

						// Create a Map to filter duplicates by ID
						const uniqueGroupsMap = new Map(
							allPotentialGroups.map(group => [group.id, group])
						);

						return Array.from(uniqueGroupsMap.values());
					});
			}
			});
		}
	}, [setUsers, router,setGroups]);

	const updateValues = (value: number) => {
		if(listType === ListType.USER){
			if (values.includes(value)) {
				return setValues((ps) => {
					const newState: number[] = [];
					ps.forEach((s) => {
						if (s !== value) {
							newState.push(s);
						}
					});
					return newState;
				});
			}
			setValues((ps) => {
				return [...ps, value];
			});
		}
		else{
			// if group is selected, add all users in that group to values, if group is deselected, remove all users in that group from values
			const groupUsers = users.filter(u => u.group.id === value).map(u => u.id);
			if (groupUsers.length > 0 && groupUsers.some(id => values.includes(id))) {
				return setValues((ps) => {
					const newState: number[] = [];
					ps.forEach((s) => {
						if (!groupUsers.includes(s)) {
							newState.push(s);
						}
					});
					return newState;
				});
			}
			setValues((ps) => {
				return [...ps, ...groupUsers];
			});
		}
	};

	const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
		e.preventDefault();
		handler(values);
	};

	if (!active) {
		return <></>;
	}

	return (
		<Backdrop mainRoute={`/projects/${router.query.projectId}`}>
			<div className={[styles.form, styles.center].join(" ")}>
				<form onSubmit={handleSubmit}>
					<div className="flex justify-between mx-3">
						<button onClick={(e) => { e.preventDefault(); setListType(ListType.GROUP); }} className="w-24 bg-blue-500 text-white rounded"> Group</button>
						<button onClick={(e) => { e.preventDefault(); setListType(ListType.USER); }} className="w-24 bg-blue-500 text-white rounded"> User</button>
					</div>
					<div className={styles.inputs}>
						{
							listType === ListType.USER ? (
								<List selected={values} values={users} handler={updateValues} listType={ListType.USER} />
							) : (
								<List selected={values} values={groups} handler={updateValues} listType={ListType.GROUP} users={users} />							)
						}
						{/* <List selected={values} values={users} handler={updateValues} listType={ListType.USER} />
						<List selected={values} values={groups} handler={updateValues} listType={ListType.GROUP} /> */}
					</div>
					<div>
						<input
							type="submit"
							value="Save"
							className={[styles.submit].join(" ")}
						/>
					</div>
				</form>
			</div>
		</Backdrop>
	);
};

export default ProjectAssign;

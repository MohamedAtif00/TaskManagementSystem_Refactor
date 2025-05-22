import { useRouter } from "next/router";
import styles from "../styles.module.scss";
import Backdrop from "../backdrop";
import React, { useEffect, useState } from "react";
import API from "../../../lib/API";
import CrossIcon from "../../../assets/Icons/Cross";
import PlusIcon from "../../../assets/Icons/Plus";

interface Props {
	handler: (userIds: number[]) => void;
}

interface loUser {
	id: number;
	name: string;
	group: { id: number; name: string };
	role: UserRole;
}

const List = ({
	values,
	selected,
	handler,
}: {
	values: loUser[];
	selected: number[];
	handler: (id: number) => void;
}) => {
	return (
		<div className={styles.listSelect}>
			<ul className={styles.add}>
				{values.map((u) => {
					const isSelected = selected.includes(u.id);
					return (
						<li
							key={u.id}
							className={isSelected ? styles.selected : ""}
							onClick={() => handler(u.id)}
						>
							<div>
								<div className={styles.name}>{u.name}</div>
								<div>{u.group.name}</div>
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
				}

			});
		}
	}, [setUsers, router]);

	const updateValues = (value: number) => {
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
					<div className={styles.inputs}>
						<List selected={values} values={users} handler={updateValues} />
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

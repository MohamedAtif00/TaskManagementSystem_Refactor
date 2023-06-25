import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import API from "../../../lib/API";
import Backdrop from "../backdrop";
import styles from "../styles.module.scss";

interface Props {
	taskId: string | string[];
	refreshTask: () => void;
}

const AssignTask = (props: Props) => {
	const [state, setState] = useState<{
		assignedUser?: { id: number; name: string };
		assignableUsers: { id: number; name: string }[];
	}>({
		assignableUsers: [],
	});
	const [selectedUser, setSelectedUser] = useState(0);
	const router = useRouter();

	useEffect(() => {
		API.TASKS.TASK_ASSIGNMENT(props.taskId).then((res) => {
			if (res && !res.error) {
				setState(() => res.data);
			}
		});
	}, [props.taskId]);

	const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
		e.preventDefault();
		API.TASKS.ASSIGN_TO_TASK({
			userId: selectedUser,
			taskId: props.taskId,
		}).then(() => {
			window.history.length < 2
				? router.back()
				: router.push(`/tasks/${router.query.projectId}`);
			props.refreshTask();
		});
	};

	return (
		<Backdrop mainRoute={`/tasks/${router.query.projectId}`}>
			<div className={[styles.form, styles.center].join(" ")}>
				<div>
					Assigned User:{" "}
					{state.assignedUser ? state.assignedUser.name : "None"}
				</div>
				<form onSubmit={handleSubmit}>
					<div className={styles.inputs}>
						<select
							value={selectedUser}
							onChange={(e) => {
								const value = parseInt(e.target.value);
								setSelectedUser(isNaN(value) ? 0 : value);
							}}
						>
							<option hidden value={0}>
								None
							</option>
							{state.assignableUsers.map((u) => (
								<option key={u.id} value={u.id}>
									{u.name}
								</option>
							))}
						</select>
					</div>
					<div>
						<input
							type="submit"
							value={selectedUser === 0 ? "Unassign" : "Assign"}
							className={[styles.submit].join(" ")}
						/>
					</div>
				</form>
			</div>
		</Backdrop>
	);
};

export default AssignTask;

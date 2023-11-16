import UserTasksDetailsHeader from "./user-tasks-details-header";
import TaskInfo from "./task-info";
import { useEffect, useState } from "react";
import API from "../../../lib/API";

interface Props {
	userId: string | string[];
}

const UserTasksDetails = (props: Props) => {
	const [userData, setUserData] = useState<UserTaskInfo>();

	useEffect(() => {
		API.RESOURCES.USERS.GET_USER_TASKS(props.userId).then(
			(res) => res && !res.error && setUserData(res.data)
		);
	}, [props.userId]);

	if (userData === undefined) return <></>;

	return (
		<div className="fixed top-0 left-0 right-0 bottom-0 z-30 bg-black/25 flex justify-end">
			<div className="bg-white rounded-l-lg flex flex-col">
				<UserTasksDetailsHeader
					name={userData.name}
					group={userData.group.name}
					tasks={{
						backlog: userData.backlogCount,
						todo: userData.todoTasks.length,
						doing: userData.doingTasks.length,
					}}
				/>
				<div className="grow overflow-y-hidden">
					<div className="overflow-y-auto max-h-full">
						<div className="p-6 w-full overflow-x-hidden flex flex-col gap-4">
							<div>
								<h4 className="mb-2 text-xl">To Do Tasks:</h4>
								<div className="grid grid-cols-4 gap-4">
									{userData.todoTasks.map((t) => (
										<TaskInfo
											key={t.id}
											loName={t.learningObjective.name}
											name={t.name}
											projectId={t.projectId}
											taskId={t.id}
										/>
									))}
								</div>
							</div>
							<div>
								<h4 className="mb-2 text-xl">Doing Tasks:</h4>
								<div className="grid grid-cols-4 gap-4">
									{userData.doingTasks.map((t) => (
										<TaskInfo
											key={t.id}
											loName={t.learningObjective.name}
											name={t.name}
											projectId={t.projectId}
											taskId={t.id}
										/>
									))}
								</div>
							</div>
						</div>
					</div>
				</div>
			</div>
		</div>
	);
};

export default UserTasksDetails;

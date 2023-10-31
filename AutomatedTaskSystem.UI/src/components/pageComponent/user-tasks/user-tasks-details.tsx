import UserTasksDetailsHeader from "./user-tasks-details-header";
import TaskInfo from "./task-info";

interface Props {
	userId: string | string[];
}

const UserTasksDetails = (props: Props) => {
	return (
		<div className="fixed top-0 left-0 right-0 bottom-0 z-30 bg-black/25 flex justify-end">
			<div className="bg-white rounded-l-lg flex flex-col">
				<UserTasksDetailsHeader
					name="Mohamed Hesham"
					group="System Maintainer"
					tasks={{ backlog: 20, doing: 2, todo: 10 }}
				/>
				<div className="grow">
					<div className="p-6 overflow-y-auto w-full overflow-x-hidden flex flex-col gap-4">
						<div>
							<h4 className="mb-2 text-xl">Doing Tasks:</h4>
							<div className="grid grid-cols-4 gap-4">
								<TaskInfo
									loName="ara_01_01_10"
									name="SB Writing"
									projectId={2065}
									taskId={50804}
								/>
								<TaskInfo
									loName="ara_01_01_10"
									name="SB Writing"
									projectId={2065}
									taskId={50804}
								/>
								<TaskInfo
									loName="ara_01_01_10"
									name="SB Writing"
									projectId={2065}
									taskId={50804}
								/>
								<TaskInfo
									loName="ara_01_01_10"
									name="SB Writing"
									projectId={2065}
									taskId={50804}
								/>
								<TaskInfo
									loName="ara_01_01_10"
									name="SB Writing"
									projectId={2065}
									taskId={50804}
								/>
								<TaskInfo
									loName="ara_01_01_10"
									name="SB Writing"
									projectId={2065}
									taskId={50804}
								/>
								<TaskInfo
									loName="ara_01_01_10"
									name="SB Writing"
									projectId={2065}
									taskId={50804}
								/>
								<TaskInfo
									loName="ara_01_01_10"
									name="SB Writing"
									projectId={2065}
									taskId={50804}
								/>
								<TaskInfo
									loName="ara_01_01_10"
									name="SB Writing"
									projectId={2065}
									taskId={50804}
								/>
								<TaskInfo
									loName="ara_01_01_10"
									name="SB Writing"
									projectId={2065}
									taskId={50804}
								/>
							</div>
						</div>
						<div>
							<h4 className="mb-2 text-xl">To Do Tasks:</h4>
							<div className="grid grid-cols-4 gap-4">
								<TaskInfo
									loName="ara_01_01_10"
									name="SB Writing"
									projectId={2065}
									taskId={50804}
								/>
								<TaskInfo
									loName="ara_01_01_10"
									name="SB Writing"
									projectId={2065}
									taskId={50804}
								/>
							</div>
						</div>
					</div>
				</div>
			</div>
		</div>
	);
};

export default UserTasksDetails;

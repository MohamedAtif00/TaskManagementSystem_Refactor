import Head from "next/head";
import ResourcesIcon from "../../assets/Icons/Resources";
import { useEffect, useState } from "react";
import API from "../../lib/API";
import Loader from "../../components/loader";
import TableAction from "../../components/TableComponents/TableActionButton";
import UserTasksDetails from "../../components/pageComponent/user-tasks/user-tasks-details";
import { useRouter } from "next/router";

const TableHeader = () => (
	<div className="grid grid-cols-12 text-sm border border-solid border-gray-300">
		<div className="col-span-3 px-3 py-3">User Name</div>
		<div className="col-span-3 px-3 py-3">Group</div>
		<div className="col-span-2 px-3 py-3">To Do Tasks</div>
		<div className="col-span-2 px-3 py-3">Doing Tasks</div>
		<div className="col-span-2 px-3 py-3 text-end">Actions</div>
	</div>
);

interface TableRowProps {
	name: string;
	group: string;
	tasks: {
		todo: number;
		doing: number;
	};
	id: number;
}

const TableRow: React.FC<TableRowProps> = ({ name, group, tasks, id }) => (
	<div className="grid grid-cols-12 text-sm border-t-0 border border-solid border-gray-300 hover:bg-gray-100">
		<div className="col-span-3 px-3 py-3">{name}</div>
		<div className="col-span-3 px-3 py-3">{group}</div>
		<div className="col-span-2 px-3 py-3">{tasks.todo}</div>
		<div className="col-span-2 px-3 py-3">{tasks.doing}</div>
		<div className="col-span-2 px-3 py-3 flex justify-end gap-4">
			<TableAction
				text="View"
				url={{
					pathname: `/user-tasks`,
					query: {
						"user-id": id,
					},
				}}
				type="eye"
			/>
		</div>
	</div>
);

const UserTasksPage = () => {
	const [users, setUsers] = useState<UserTaskCount[]>();
	const router = useRouter();

	useEffect(() => {
		API.RESOURCES.USERS.GET_ALL_USER_TASKS().then(
			(res) => res && !res.error && setUsers(res.data)
		);
	}, []);

	if (users === undefined)
		return (
			<div className="flex items-center justify-center mx-auto h-screen bg-slate-200">
				<Head>
					<title>ATS - Loading</title>
				</Head>
				<Loader />
			</div>
		);

	return (
		<>
			<div className="mx-4 flex flex-col w-full gap-4">
				<Head>
					<title>ATS - User Tasks</title>
				</Head>
				<div className="py-4 px-8 bg-white rounded-b-lg border border-gray-300 border-solid flex justify-between">
					<div className="flex gap-4">
						<ResourcesIcon />
						<div className="font-bold text-xl whitespace-nowrap">
							User Tasks
						</div>
					</div>
				</div>
				<div className="bg-white">
					<TableHeader />
					{users.map((u) => (
						<TableRow
							id={u.id}
							key={u.id}
							name={u.name}
							group={u.group.name}
							tasks={u.tasks}
						/>
					))}
				</div>
			</div>
			{router.query["user-id"] !== undefined && <UserTasksDetails />}
		</>
	);
};

export default UserTasksPage;

import { useRouter } from "next/router";
import { useEffect } from "react";
import { useAppDispatch, useAppSelector } from "../../../app/hooks";
import API from "../../../lib/API";
import { clear, load } from "../../../slices/userSlice";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import AddUser from "../../../components/pageComponent/users/addUser";
import Link from "next/link";

const columns: GridColDef[] = [
	{ field: "col0", headerName: "ID", width: 100 },
	{
		field: "col1",
		headerName: "Name",
		width: 200,
		cellClassName: "relative",
	},
	{ field: "col2", headerName: "Group", width: 200 },
	{ field: "col3", headerName: "Role", width: 200 },
];

const Users = () => {
	const users = useAppSelector((states) => states.usersSlice);
	const dispatch = useAppDispatch();
	const auth = useAppSelector((s) => s.authSlice);
	const router = useRouter();

	if (!auth.isAuth || auth.role != 1) router.replace("/");

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
		<div className="mx-auto relative max-h-screen overflow-y-auto pr-4">
			<div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
				<h1 className="font-bold text-2xl ">Users</h1>
				<Link
					href={{
						pathname: "/resources/users",
						query: {
							form: "add-user",
						},
					}}
				>
					<button className="px-4 py-1 rounded bg-blue-600 text-white">
						Add user
					</button>
				</Link>
			</div>
			<div className="pb-4 mt-4">
				<DataGrid
					className="bg-white relative h-full"
					rows={users.map((u) => {
						return {
							id: u.id,
							col0: u.id,
							col1: u.name,
							col2: u.group.name,
							col3: u.role.name,
						};
					})}
					columns={columns}
				/>
			</div>
			<AddUser />
		</div>
	);
};

export default Users;

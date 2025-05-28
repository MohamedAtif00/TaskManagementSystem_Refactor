    import { useRouter } from "next/router";
    import { useEffect } from "react";
    import { useAppDispatch, useAppSelector } from "../../../app/hooks";
    import API from "../../../lib/API";
    import { clear, load } from "../../../slices/userSlice";
    import { DataGrid, GridColDef } from "@mui/x-data-grid";
    import AddUser from "../../../components/pageComponent/users/addUser";
    import Link from "next/link";
    import EditUser from "../../../components/pageComponent/users/editUser";
    import RemoveUser from "../../../components/pageComponent/users/removeUser";
    import ResourcesIcon from "../../../assets/Icons/Resources";
    import TableAction from "../../../components/TableComponents/TableActionButton";
    import Head from "next/head";

    const columns: GridColDef[] = [
        { field: "col0", headerName: "ID", width: 100 },
        {
            field: "col1",
            headerName: "Name",
            width: 200,
            cellClassName: "relative",
            sortComparator: (A, B) => {
                const a = A.toLowerCase(),
                    b = B.toLowerCase();
                return a > b ? 1 : b > a ? -1 : 0;
            },
        },
        { field: "col2", headerName: "Group", width: 200 },
        { field: "col3", headerName: "Role", width: 200 },
        {
            field: "col4",
            headerName: "Actions",
            width: 125,
            renderCell: (c) => (
                <div className="flex justify-end gap-4">
                    <TableAction
                        text="Edit"
                        url={{
                            pathname: "/resources/users",
                            query: {
                                form: "edit-user",
                                userId: c.id,
                            },
                        }}
                        type="edit"
                    />
                    <TableAction
                        text="Archive"
                        url={{
                            pathname: "/resources/users",
                            query: {
                                form: "remove-user",
                                userId: c.id,
                            },
                        }}
                        type="archive"
                    />
                </div>
            ),
            filterable: false,
            disableColumnMenu: true,
            sortable: false,
        },
    ];

    const Users = () => {
        const users = useAppSelector((states) => states.usersSlice);
        const dispatch = useAppDispatch();
        const auth = useAppSelector((s) => s.authSlice);
        const router = useRouter();

        // if (!auth.isAuth || auth.role !== 0) router.replace("/");

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
            <>
                <Head>
                    <title>ATS - Users</title>
                </Head>
                <div className="mx-auto relative max-h-screen overflow-y-auto pr-4">
                    <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
                        <div className="flex gap-2 items-center">
                            <div className="basis-6 h-6">
                                <ResourcesIcon />
                            </div>
                            <h1 className="font-bold text-2xl ">Users</h1>
                        </div>
                        <Link
                            href={{
                                pathname: "/resources/users",
                                query: {
                                    form: "add-user",
                                }
                            }}
                        >
                            <button className="px-4 py-1 rounded bg-blue-600 text-white">
                                Add User
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
                                    col2: u.group?.name,
                                    col3:
                                        u.role === 0
                                            ? "Project Manager"
                                            : u.role === 1
                                            ? "Section Head"
                                            : u.role === 2
                                            ? "Team Leader"
                                            : "Member",
                                };
                            })}
                            onRowClick={(params, event) => {
                                const isButton = (event.target as HTMLElement).closest("button");
                                if (!isButton) {
                                    router.push(`/resources/users/${params.id}`);
                                }
                            }}
                            
                            columns={columns}
                            initialState={{
                                sorting: {
                                    sortModel: [{ field: "col1", sort: "asc" }],
                                },
                            }}
                        />
                    </div>
                    <AddUser />
                    <RemoveUser />
                    <EditUser />
                </div>
            </>
        );
    };

    export default Users;

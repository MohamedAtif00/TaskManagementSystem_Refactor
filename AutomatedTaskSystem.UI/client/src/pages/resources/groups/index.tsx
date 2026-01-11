import { useAppSelector, useAppDispatch } from "../../../app/hooks";
import { clear, load } from "../../../slices/groupSlice";
import { useEffect } from "react";
import API from "../../../lib/API";
import { useRouter } from "next/router";
import ResourcesIcon from "../../../assets/Icons/Resources";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import TableAction from "../../../components/TableComponents/TableActionButton";
import AddGroup from "../../../components/pageComponent/resources/addGroup";
import Link from "next/link";
import EditGroup from "../../../components/pageComponent/resources/editGroup";
import Head from "next/head";

const columns: GridColDef[] = [
    { field: "col0", headerName: "ID", width: 100 },
    {
        field: "col1",
        headerName: "Name",
        width: 300,
        cellClassName: "relative",
        renderCell: (c) => {
            return (
                <div className="flex gap-2">
                    <div className="flex items-center">
                        <div
                            className="p-2 rounded-full"
                            style={{ background: c.value.color }}
                        ></div>
                    </div>
                    <div>{c.value.name}</div>
                </div>
            );
        },
        disableColumnMenu: true,
        sortComparator: (A, B) => {
            const a = A.name.toLowerCase(),
                b = B.name.toLowerCase();
            return a > b ? 1 : b > a ? -1 : 0;
        },
    },
    {
        field: "col2",
        headerName: "Actions",
        width: 100,
        renderCell: (c) => (
            <div className="flex justify-end gap-4">
                <TableAction
                    text="Edit"
                    url={{
                        pathname: "/resources/groups",
                        query: {
                            form: "edit-group",
                            groupId: c.id,
                        },
                    }}
                    type="edit"
                />
            </div>
        ),
        filterable: false,
        disableColumnMenu: true,
        sortable: false,
    },
];

const Groups = () => {
    const groups = useAppSelector((state) => state.groupsSlice);
    const dispatch = useAppDispatch();
    const auth = useAppSelector((s) => s.authSlice);
    const router = useRouter();

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

    if (!auth.isAuth ||auth.role == 1 || auth.role == 2 || auth.role == 3) return router.replace("/");

    return (
        <div className="mx-auto relative max-h-screen overflow-y-auto pr-4">
            <Head>
                <title>ATS - Groups</title>
            </Head>
            <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
                <div className="flex gap-2 items-center">
                    <div className="w-6">
                        <ResourcesIcon color="black" />
                    </div>
                    <h1 className="font-bold text-2xl ">Groups</h1>
                </div>
                <Link
                    href={{
                        pathname: "/resources/groups",
                        query: {
                            form: "add-group",
                        },
                    }}
                >
                    <button className="px-4 py-1 rounded bg-blue-600 text-white">
                        Add Group
                    </button>
                </Link>
            </div>
            <div className="pb-4 mt-4">
                <DataGrid
                    className="bg-white relative h-full"
                    rows={groups.map((g) => {
                        return {
                            id: g.id,
                            col0: g.id,
                            col1: {
                                name: g.name,
                                color: g.colorCode,
                            },
                        };
                    })}
                    columns={columns}
                    initialState={{
                        sorting: {
                            sortModel: [{ field: "col1", sort: "asc" }],
                        },
                    }}
                />
            </div>
            <AddGroup />
            <EditGroup />
        </div>
    );
};

export default Groups;

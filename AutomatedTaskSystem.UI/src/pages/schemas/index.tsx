import { useAppDispatch, useAppSelector } from "../../app/hooks";
import { useEffect } from "react";
import API from "../../lib/API";
import { clear, load } from "../../slices/schemaSlice";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import Link from "next/link";
import SchemaIcon from "../../assets/Icons/Schema";
import CreateSchema from "../../components/pageComponent/schemas/createSchema";
import TableAction from "../../components/TableComponents/TableActionButton";
import EditSchema from "../../components/pageComponent/schemas/editSchema";
import DuplicateSchema from "../../components/pageComponent/schemas/duplicateSchema";
import RemoveSchema from "../../components/pageComponent/schemas/removeSchema";

const columns: GridColDef[] = [
    { field: "col0", headerName: "ID", width: 100 },
    {
        field: "col1",
        headerName: "Name",
        width: 300,
    },
    { field: "col2", headerName: "Description", width: 300 },
    {
        field: "col3",
        headerName: "Actions",
        width: 220,
        renderCell: (c) => (
            <div className="flex justify-end gap-4">
                <TableAction
                    text="View"
                    url={{
                        pathname: `/schemas/${c.id}`,
                    }}
                    type="eye"
                />
                <TableAction
                    text="Edit"
                    url={{
                        pathname: "/schemas",
                        query: {
                            form: "edit-schema",
                            schemaId: c.id,
                        },
                    }}
                    type="edit"
                />
                <TableAction
                    text="Duplicate"
                    url={{
                        pathname: `/schemas`,
                        query: {
                            form: "duplicate-schema",
                            schemaId: c.id,
                        },
                    }}
                    type="duplicate"
                />
                <TableAction
                    text="Delete"
                    url={{
                        pathname: "/schemas",
                        query: {
                            form: "remove-schema",
                            schemaId: c.id,
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

const Schemas = () => {
    const schemas = useAppSelector((states) => states.schemasSlice);
    const dispatch = useAppDispatch();

    useEffect(() => {
        API.SCHEMAS.GET_ALL().then((res) => {
            if (res) {
                dispatch(load(res));
            }
        });

        return () => {
            dispatch(clear());
        };
    }, [dispatch]);

    return (
        <div className="mx-auto relative max-h-screen overflow-y-auto pr-4">
            <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
                <div className="flex gap-2 items-center">
                    <div className="w-6">
                        <SchemaIcon />
                    </div>
                    <h1 className="font-bold text-2xl ">Schema</h1>
                </div>
                <Link
                    href={{
                        pathname: "/schemas",
                        query: {
                            form: "add-schema",
                        },
                    }}
                >
                    <button className="px-4 py-1 rounded bg-blue-600 text-white">
                        Create Schema
                    </button>
                </Link>
            </div>
            <div className="pb-4 mt-4">
                <DataGrid
                    className="bg-white relative h-full"
                    rows={schemas.map((p) => {
                        return {
                            id: p.id,
                            col0: p.id,
                            col1: p.name,
                            col2: p.description,
                        };
                    })}
                    columns={columns}
                />
            </div>
            <CreateSchema />
            <EditSchema />
            <DuplicateSchema />
            <RemoveSchema />
        </div>
    );
};

export default Schemas;

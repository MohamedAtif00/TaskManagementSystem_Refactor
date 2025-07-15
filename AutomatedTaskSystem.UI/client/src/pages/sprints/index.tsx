import { useEffect, useState } from "react";
import API from "../../lib/API";
import Link from "next/link"; // Ensure Link is imported
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import TaskIcon from "../../assets/Icons/Task"; // Adjust path as needed
import Head from "next/head";
import Loader from "../../components/loader"; // Adjust path as needed
import CreateSprint from "../../components/sprintComponents/createSprint"; // Adjust path as needed
import { format } from "date-fns";

// Columns for Sprints
const sprintColumns: GridColDef[] = [
    { field: "col0", headerName: "ID", width: 90 },
    { field: "col1", headerName: "Sprint Name", width: 300 },
    { field: "col2", headerName: "Start Date", width: 200 },
    { field: "col3", headerName: "End Date", width: 200 }
];

const Sprints = () => {
    const [sprints, setSprints] = useState<ISprint[]>();

    // Fetch Sprints
    useEffect(() => {
        API.SPRINTS.GET_ALL_SPRINTS().then((res) => {
            if (res && !res.error && res.data) {
                setSprints(res.data);
            } else {
                console.error("Error fetching sprints:", res?.message || "Unknown error");
                setSprints([]); // Set to empty array on error to stop loading
            }
        });
    }, []);

    if (sprints === undefined)
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Head>
                    <title>ATS - Loading</title>
                </Head>
                <Loader />
            </div>
        );

    return (
        <>
            <Head>
                <title>ATS - Sprints</title>
            </Head>
            <div className="mx-auto relative max-h-screen overflow-y-auto pr-4">
                <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
                    <div className="flex gap-2 items-center">
                        <TaskIcon className="stroke-black" />
                        <h1 className="font-bold text-2xl">Sprints</h1>
                    </div>
                    {/* Fixed: Link wrapping a button for navigation */}
                    <Link
                        href={{
                            pathname: "/sprints",
                            query: {
                                form: "create-sprint",
                            },
                        }}
                    >
                        <button className="px-4 py-1 rounded bg-blue-600 text-white">
                            Add Sprint
                        </button>
                    </Link>
                </div>
                <div className="pb-4 mt-4">
                    <DataGrid
                        className="bg-white relative h-full"
                        initialState={{
                            sorting: {
                                sortModel: [{ field: "col1", sort: "asc" }],
                            },
                        }}
                        slots={{
                            row: (r) => {
                                return (
                                    // Fixed: Move props directly to Link
                                    <Link
                                        href={`/sprints/${r.row.id}`} // Simpler template literal for dynamic routes
                                        key={r.rowId} // Key usually goes on the outermost element of a list item
                                        style={{ height: r.rowHeight }}
                                        className="group hover:bg-slate-50 flex border-solid border-b border-slate-200"
                                    >
                                        {/* Now, the Link component itself will render the <a> tag with these styles */}
                                        {r.visibleColumns.map((c: any) => {
                                            if (c.field === "col1")
                                                return (
                                                    <div
                                                        key={c.headerName}
                                                        style={{
                                                            minWidth: c.width,
                                                            maxWidth: c.width,
                                                        }}
                                                        className="px-[0.625rem] group-hover:pl-4 transition-all ease-in text-base flex items-center group-hover:text-blue-700"
                                                    >
                                                        {r.row[c.field]}
                                                    </div>
                                                );

                                            return (
                                                <div
                                                    key={c.headerName}
                                                    style={{
                                                        minWidth: c.width,
                                                        maxWidth: c.width,
                                                    }}
                                                    className="px-[0.625rem] group-hover:pl-4 transition-all ease-in text-sm flex items-center group-hover:text-blue-700"
                                                >
                                                    {r.row[c.field]}
                                                </div>
                                            );
                                        })}
                                    </Link>
                                );
                            },
                        }}
                        rows={sprints.map((s) => {
                            return {
                                id: s.id,
                                col0: s.id,
                                col1: s.name,
                                col2: format(new Date(s.startDate), 'yyyy-MM-dd'),
                                col3: format(new Date(s.endDate), 'yyyy-MM-dd')
                            };
                        })}
                        columns={sprintColumns}
                    />
                </div>
                <CreateSprint />
            </div>
        </>
    );
};

export default Sprints;
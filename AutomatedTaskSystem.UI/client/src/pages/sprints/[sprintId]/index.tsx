import { GridColDef, DataGrid } from "@mui/x-data-grid";
import Head from "next/head";
import Link from "next/link";
import { useRouter } from "next/router";
import { format } from "date-fns";
import { useState, useEffect } from "react";
import TaskIcon from "../../../assets/Icons/Task"; // Adjust path as needed
import API from "../../../lib/API";
import Loader from "../../../components/loader"; // Adjust path as needed
import { useAppSelector } from "../../../app/hooks";
import EditSprint from "../../../components/sprintComponents/editSprint";
// Assuming these interfaces are defined in your project:

// Columns for Learning Objectives
const loColumns: GridColDef[] = [
    { field: "id", headerName: "LO ID", width: 90 },
    { field: "name", headerName: "LO Name", flex: 1, minWidth: 250 },
    // Add more columns here if your ILearningObjective interface includes more fields
    // { field: "tag", headerName: "Tag", width: 150 },
];

const SingleSprintPage = () => {
    const router = useRouter();
    // Ensure that the dynamic route segment in your file system matches this name:
    // pages/sprints/[sprintId].tsx, so the key is 'sprintId'
    const { sprintId } = router.query;
    const { role } = useAppSelector((s) => s.authSlice);
    const [sprint, setSprint] = useState<ISprint | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const refreshSprintDetails = () => {
        if (sprintId && typeof sprintId === "string") {
            setLoading(true);
            setError(null);
            API.SPRINTS.GET_ONE(sprintId)
                .then((res) => {
                    if (res && !res.error && res.data) {
                        setSprint(res.data);
                    } else {
                        setError(res?.message || "Failed to load sprint.");
                    }
                })
                .catch((err) => {
                    console.error("Error fetching sprint:", err); setError("An unexpected error occurred while fetching sprint details.");
                })
                .finally(() => {
                    setLoading(false);
                });
        }
    };

    useEffect(() => {
        if (router.isReady) {
            refreshSprintDetails();
        }
    }, [sprintId, router.isReady]); // Dependency array: re-run effect when sprintId changes

    if (loading) {
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Head>
                    <title>TMS - Loading Sprint</title>
                </Head>
                {/* Assuming Loader is a React component */}
                <Loader />
            </div>
        );
    }

    if (error) {
        return (
            <div className="flex items-center justify-center mx-auto h-full text-red-500">
                <Head>
                    <title>TMS - Error</title>
                </Head>
                <p>{error}</p>
            </div>
        );
    }

    if (!sprint) {
        return (
            <div className="flex items-center justify-center mx-auto h-full text-gray-500">
                <Head>
                    <title>TMS - Sprint Not Found</title>
                </Head>
                <p>Sprint not found.</p>
            </div>
        );
    }

    // Prepare rows for Learning Objectives DataGrid
    // DataGrid requires a unique 'id' property for each row
    const loRows = sprint.learningObjects.map((lo) => ({
        id: lo.id,
        name: lo.name,
        // Add other properties if you want them accessible in the row object for custom rendering
        // tag: lo.tag,
    }));

    return (
        <>
            <Head>
                <title>TMS - {sprint.name}</title>
            </Head>
            <div className="mx-auto relative max-h-screen overflow-y-auto pr-4 w-11/12">
                <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
                    <div className="flex gap-2 items-center">
                        {/* <TaskIcon className="stroke-black" /> */}
                        <Link href="/sprints">
                            <svg width="10" height="17" viewBox="0 0 10 17" fill="none" xmlns="http://www.w3.org/2000/svg">
                                <path fill-rule="evenodd" clip-rule="evenodd" d="M2.41379 8.485L9.48479 15.556L8.07079 16.97L0.292786 9.192C0.105315 9.00447 0 8.75016 0 8.485C0 8.21984 0.105315 7.96553 0.292786 7.778L8.07079 0L9.48479 1.414L2.41379 8.485Z" fill="black"/>
                            </svg>
                        </Link>
                        <h1 className="font-bold text-2xl">Sprint: {sprint.name}</h1>
                    </div>
                    <div className="flex items-center gap-4">
                        {(role === 0 || role === 4) && (
                            <Link href={{ pathname: router.pathname, query: { ...router.query, form: "edit-sprint" } }}>
                                <button className="px-4 py-1 rounded bg-blue-600 text-white hover:bg-blue-700 transition-colors">
                                    Edit Sprint
                                </button>
                            </Link>
                        )}
                        {/* <Link href="/sprints">
                            <button className="px-4 py-1 rounded bg-gray-600 text-white hover:bg-gray-700 transition-colors">
                                Back to Sprints
                            </button>
                        </Link> */}
                    </div>
                </div>

                <div className="p-8">
                    <div className="bg-white p-6 rounded-md shadow-sm mb-6">
                        <h2 className="text-xl font-semibold mb-4">Sprint Details</h2>
                        <p className="text-gray-700 mb-2"><strong>Description:</strong> {sprint.description}</p>
                        {/* Ensure dates are handled as strings from backend for consistency with format() */}
                        <p className="text-gray-700 mb-2"><strong>Start Date:</strong> {format(new Date(sprint.startDate), 'yyyy-MM-dd')}</p>
                        <p className="text-gray-700 mb-2"><strong>End Date:</strong> {format(new Date(sprint.endDate), 'yyyy-MM-dd')}</p>
                    </div>

                    <div className="bg-white p-6 rounded-md shadow-sm">
                        <h2 className="text-xl font-semibold mb-4">Associated Learning Objectives</h2>
                        {sprint.learningObjects.length > 0 ? (
                            <div style={{ width: '100%' }}>
                                <DataGrid
                                    rows={loRows}
                                    columns={loColumns}
                                    autoHeight // Adjusts height based on content, avoiding fixed height
                                    initialState={{ pagination: { paginationModel: { pageSize: 5 } } }}
                                    pageSizeOptions={[5, 10, 25]}
                                    // Make LO rows clickable
                                    slots={{
                                        row: (rowParams) => {
                                            // Construct the href for the LO detail page
                                            const loDetailHref = `/sprints/${sprint.id}/${rowParams.row.id}/board`;
                                            return (
                                                <Link
                                                    href={loDetailHref}
                                                    key={rowParams.row.id} // Use the LO's ID as the key for the row
                                                    // Apply your desired styling to the Link component
                                                    className="group hover:bg-slate-50 flex border-solid border-b border-slate-200 cursor-pointer"
                                                    style={{ height: rowParams.rowHeight }}
                                                >
                                                    {/* Map over visible columns to render content within the Link */}
                                                    {rowParams.visibleColumns.map((col: any) => (
                                                        <div
                                                            key={col.field}
                                                            style={{
                                                                minWidth: col.width || 0, // Fallback for flex columns
                                                                maxWidth: col.width || '100%',
                                                                flexGrow: col.flex || 0,
                                                            }}
                                                            className="px-[0.625rem] group-hover:pl-4 transition-all ease-in text-sm flex items-center group-hover:text-blue-700"
                                                        >
                                                            {rowParams.row[col.field]}
                                                        </div>
                                                    ))}
                                                </Link>
                                            );
                                        },
                                    }}
                                />
                            </div>
                        ) : (
                            <p className="text-gray-500">No learning objectives associated with this sprint.</p>
                        )}
                    </div>
                </div>
            </div>
            {(role === 0 || role === 4) && sprintId && typeof sprintId === 'string' && (
                <EditSprint sprintId={sprintId} onSprintUpdated={refreshSprintDetails} />
            )}
        </>
    );
};

export default SingleSprintPage;
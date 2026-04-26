import {
    useEffect,
    useState,
    forwardRef,
    type CSSProperties,
    type ReactNode,
} from "react";
import API from "../../lib/API";
import {
    DataGrid,
    GridColDef,
    useGridApiContext,
    useGridSelector,
    gridColumnsTotalWidthSelector,
} from "@mui/x-data-grid";
import type { GridRowProps } from "@mui/x-data-grid";
import TaskIcon from "../../assets/Icons/Task";
import PieChartIcon from "../../assets/Icons/PieChart";
import Head from "next/head";
import Loader from "../../components/loader";
import { useRouter } from "next/router";
import ProgressDonut from "../../components/charts/ProgressDonut";

/** Pixel width per cell — must match `unstable_getCellColSpanInfo` so headers and body stay aligned. */
function alignedCellStyle(width: number): CSSProperties {
    return {
        width,
        minWidth: width,
        maxWidth: width,
        flexShrink: 0,
        overflow: "hidden",
        boxSizing: "border-box",
    };
}

/**
 * Custom row that uses the same column widths as the DataGrid header (via grid API),
 * instead of CSS flex on colDef, which desyncs from the header.
 */
const ProjectsDataGridRow = forwardRef<HTMLDivElement, GridRowProps>(
    function ProjectsDataGridRow(props, ref) {
        const router = useRouter();
        const apiRef = useGridApiContext();
        const columnsTotalWidth = useGridSelector(
            apiRef,
            gridColumnsTotalWidthSelector
        );

        const {
            rowId,
            rowHeight,
            containerWidth,
            firstColumnToRender,
            visibleColumns,
            renderedColumns,
            focusedCell,
            focusedCellColumnIndexNotInRange,
            row,
            style,
            className,
            isNotVisible,
            index: _i,
            lastColumnToRender: _l,
            position: _p,
            selected: _s,
            tabbableCell: _t,
            isLastVisible: _lv,
            hovered: _h,
            ...rest
        } = props;

        if (!row) {
            return null;
        }

        const rowNode = apiRef.current.getRowNode(rowId);
        if (!rowNode) {
            return null;
        }

        if (isNotVisible) {
            return (
                <div
                    ref={ref}
                    style={{ opacity: 0, width: 0, height: 0 }}
                    aria-hidden
                />
            );
        }

        const emptyCellWidth = containerWidth - columnsTotalWidth;
        const cells: ReactNode[] = [];

        for (let i = 0; i < renderedColumns.length; i += 1) {
            const column = renderedColumns[i];
            let indexRelativeToAllColumns = firstColumnToRender + i;

            if (
                focusedCellColumnIndexNotInRange !== undefined &&
                focusedCell
            ) {
                if (
                    visibleColumns[focusedCellColumnIndexNotInRange]?.field ===
                    column.field
                ) {
                    indexRelativeToAllColumns =
                        focusedCellColumnIndexNotInRange;
                } else {
                    indexRelativeToAllColumns -= 1;
                }
            }

            const cellColSpanInfo = apiRef.current.unstable_getCellColSpanInfo(
                rowId,
                indexRelativeToAllColumns
            );

            if (!cellColSpanInfo || cellColSpanInfo.spannedByColSpan) {
                continue;
            }

            const { width } = cellColSpanInfo.cellProps;
            const cellStyle = alignedCellStyle(width);
            const c = column;

            if (c.field === "col6") {
                const pct = (row as { progressPercent?: number })
                    .progressPercent ?? 0;
                cells.push(
                    <div
                        key={c.field}
                        style={cellStyle}
                        className="px-[0.625rem] flex items-center justify-center"
                        onClick={(e) => e.stopPropagation()}
                    >
                        <ProgressDonut percentage={pct} size={70} />
                    </div>
                );
                continue;
            }

            if (c.field === "col7") {
                cells.push(
                    <div
                        key={c.field}
                        style={cellStyle}
                        className="px-[0.625rem] flex items-center justify-center"
                        onClick={(e) => e.stopPropagation()}
                    >
                        <button
                            type="button"
                            title="Analytics"
                            onClick={(e) => {
                                e.stopPropagation();
                                router.push(`/tasks/charts/${row.id}`);
                            }}
                            className="p-2 rounded-md hover:bg-gray-100 transition-colors opacity-70 hover:opacity-100"
                        >
                            <div className="w-4 h-4">
                                <PieChartIcon />
                            </div>
                        </button>
                    </div>
                );
                continue;
            }

            if (c.field === "col1") {
                cells.push(
                    <div
                        key={c.field}
                        style={cellStyle}
                        className="min-w-0 px-[0.625rem] group-hover:pl-4 transition-all ease-in text-base flex items-center group-hover:text-blue-700"
                    >
                        <span className="truncate">
                            {row[c.field as keyof typeof row] as string}
                        </span>
                    </div>
                );
                continue;
            }

            cells.push(
                <div
                    key={c.field}
                    style={cellStyle}
                    className="min-w-0 px-[0.625rem] group-hover:pl-4 transition-all ease-in text-sm flex items-center group-hover:text-blue-700"
                >
                    <span className="truncate w-full">
                        {String(row[c.field as keyof typeof row] ?? "")}
                    </span>
                </div>
            );
        }

        const mergedStyle: CSSProperties = {
            ...(typeof style === "object" && style !== null
                ? (style as CSSProperties)
                : {}),
            display: "flex",
            flexDirection: "row",
            alignItems: "stretch",
            width: containerWidth,
            minWidth: containerWidth,
            maxWidth: containerWidth,
            boxSizing: "border-box",
        };
        if (typeof rowHeight === "number") {
            mergedStyle.minHeight = rowHeight;
            mergedStyle.maxHeight = rowHeight;
        }

        return (
            <div
                ref={ref}
                {...rest}
                style={mergedStyle}
                className={`group hover:bg-slate-50 border-solid border-b border-slate-200 cursor-pointer ${className ?? ""}`}
                role="button"
                tabIndex={0}
                onClick={() => router.push(`/tasks/${rowId}`)}
                onKeyDown={(e) => {
                    if (e.key === "Enter" || e.key === " ") {
                        e.preventDefault();
                        router.push(`/tasks/${rowId}`);
                    }
                }}
            >
                {cells}
                {emptyCellWidth > 0 && (
                    <div
                        role="presentation"
                        style={{
                            width: emptyCellWidth,
                            minWidth: emptyCellWidth,
                            flexShrink: 0,
                        }}
                    />
                )}
            </div>
        );
    }
);

const projectColumns: GridColDef[] = [
    { field: "col0", headerName: "ID", width: 70, minWidth: 56, description: 'Unique identifier for the project' },
    {
        field: "col1",
        headerName: "Name",
        hideable:false,
        flex: 1,
        minWidth: 120,
        description: 'Name of the project',
        sortComparator: (A, B) => {
            const a = A.toLowerCase(),
                b = B.toLowerCase();
            return a > b ? 1 : b > a ? -1 : 0;
        },
    },
    { field: "col2", headerName: "Description", description: 'Description of the project', flex: 1, minWidth: 120 },
    { field: "col3", headerName: "Year",description: 'Year of the project', width: 88, minWidth: 72 },
    { field: "col4", headerName: "Term", description: 'Term of the project', width: 88, minWidth: 72 },
    { field: "col5", headerName: "Tasks Num", description: 'Number of tasks in the project', width: 96, minWidth: 88 },
    {
        field: "col6",
        headerName: "Progress",
        description: "Overall project progress",
        width: 100,
        minWidth: 88,
        sortable: false,
        filterable: false,
        disableColumnMenu: true,
    },
    {
        field: "col7",
        headerName: "Analytics",
        description: "Project analytics and learning objectives overview",
        width: 100,
        minWidth: 88,
        sortable: false,
        filterable: false,
        disableColumnMenu: true,
    },
];

const sprintColumns: GridColDef[] = [
    { field: "id", headerName: "ID", width: 70 },
    { field: "name", headerName: "Name", width: 300 },
    { field: "startDate", headerName: "Start Date", width: 150 },
    { field: "endDate", headerName: "End Date", width: 150 },
    { field: "status", headerName: "Status", width: 120 },
];

const Projects = () => {
    // const [view, setView] = useState<"projects">("projects");
    const [projects, setProjects] = useState<IProject[]>();
    const [sprints, setSprints] = useState<any[]>();

    useEffect(() => {
        API.TASKS.PROJECTS().then((res) => {
            if (res && !res.error) setProjects(res.data);
        });
    }, []);

    // // Fetch sprints when switching to sprints view (only non-archived sprints)
    // useEffect(() => {
    //     if (view === "sprints" && !sprints) {
    //         API.SPRINTS.GET_ALL_SPRINTS(false).then((res: any) => {
    //             if (res && !res.error) setSprints(res.data);
    //         });
    //     }
    // }, [view, sprints]);

    const renderLoading = () => (
        <div className="flex items-center justify-center mx-auto h-full">
            <Head>
                <title>ATS - Loading</title>
            </Head>
            <Loader />
        </div>
    );

    if (projects === undefined) {
        return renderLoading();
    }

    return (
        <>
            <Head>
                <title>
                    ATS - Projects
                </title>
            </Head>
            <div className="mx-auto relative max-h-screen w-full min-w-0 overflow-y-auto overflow-x-hidden px-2 sm:px-4 box-border">
                <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
                    <div className="flex gap-2 items-center">
                        <TaskIcon className="stroke-black" />
                        <h1 className="font-bold text-2xl ">
                            Projects
                        </h1>
                    </div>
                    {/* <div className="flex gap-4">
                        <button
                            onClick={() => setView("projects")}
                            className={`px-4 py-2 rounded-md font-semibold transition-colors ${
                                view === "projects"
                                    ? "bg-blue-600 text-white"
                                    : "bg-gray-200 text-gray-700 hover:bg-gray-300"
                            }`}
                        >
                            Projects
                        </button>
                        { <button
                            onClick={() => setView("sprints")}
                            className={`px-4 py-2 rounded-md font-semibold transition-colors ${
                                view === "sprints"
                                    ? "bg-blue-600 text-white"
                                    : "bg-gray-200 text-gray-700 hover:bg-gray-300"
                            }`}
                        >
                            Sprints
                        </button> }
                    </div> */}
                </div>
                {projects && (
                    <div className="pb-4 mt-4 w-full min-w-0">
                        <DataGrid
                            className="bg-white relative h-full w-full min-w-0"
                            initialState={{
                                sorting: {
                                    sortModel: [{ field: "col1", sort: "asc" }],
                                },
                            }}
                            slots={{
                                row: ProjectsDataGridRow,
                            }}
                            rows={projects.map((p: any) => {
                                return {
                                    id: p.id,
                                    col0: p.id,
                                    col1: p.name,
                                    col2: p.description,
                                    col3: p.year.name,
                                    col4: p.term ? "Term 2" : "Term 1",
                                    col5: p.count || 0,
                                    col6: "",
                                    progressPercent:
                                        p.progressPercent ?? 0,
                                    col7: "",
                                };
                            })}
                            columns={projectColumns}
                            autoHeight
                            sx={{
                                width: "100%",
                                minWidth: 0,
                                "& .MuiDataGrid-main": { width: "100%" },
                                "& .MuiDataGrid-virtualScroller": {
                                    overflowX: "hidden",
                                },
                            }}
                        />
                    </div>
                )}

                {/* {view === "sprints" && (
                    <div className="pb-4 mt-4">
                        {sprints === undefined ? (
                            renderLoading()
                        ) : (
                            <>
                                <h2 className="text-xl font-semibold mb-2 px-1">
                                    Select a Sprint
                                </h2>
                                <DataGrid
                                    className="bg-white relative h-full"
                                    rows={sprints.map((s: any) => ({
                                        id: s.id,
                                        name: s.name,
                                        startDate: new Date(
                                            s.startDate
                                        ).toLocaleDateString(),
                                        endDate: new Date(
                                            s.endDate
                                        ).toLocaleDateString(),
                                        status: s.status,
                                    }))}
                                    columns={sprintColumns}
                                    onRowClick={(params) =>
                                        router.push(`/tasks/sprint/${params.id}`)
                                    }
                                    getRowClassName={(params) =>
                                        `cursor-pointer hover:bg-gray-50`
                                    }
                                    autoHeight
                                />
                            </>
                        )}
                    </div> }
                )*/}
            </div>
        </>
    );
};



export default Projects;

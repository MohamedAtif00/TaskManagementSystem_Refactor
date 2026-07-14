import {
    forwardRef,
    type CSSProperties,
    type ReactNode,
} from "react";
import {
    DataGrid,
    GridColDef,
    useGridApiContext,
    useGridSelector,
    gridColumnsTotalWidthSelector,
} from "@mui/x-data-grid";
import type { GridRowProps } from "@mui/x-data-grid";
import PieChartIcon from "../../assets/Icons/PieChart";
import { useRouter } from "next/router";
import ProgressDonut from "../charts/ProgressDonut";

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

const TaskSubjectsDataGridRow = forwardRef<HTMLDivElement, GridRowProps>(
    function TaskSubjectsDataGridRow(props, ref) {
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

export const taskSubjectColumns: GridColDef[] = [
    { field: "col0", headerName: "ID", width: 70, minWidth: 56, description: "Unique identifier for the subject" },
    {
        field: "col1",
        headerName: "Name",
        hideable: false,
        flex: 1,
        minWidth: 120,
        description: "Name of the subject",
        sortComparator: (A, B) => {
            const a = A.toLowerCase(),
                b = B.toLowerCase();
            return a > b ? 1 : b > a ? -1 : 0;
        },
    },
    { field: "col2", headerName: "Description", description: "Description of the subject", flex: 1, minWidth: 120 },
    { field: "col3", headerName: "Year", description: "Year", width: 88, minWidth: 72 },
    { field: "col4", headerName: "Term", description: "Term", width: 88, minWidth: 72 },
    { field: "col5", headerName: "Tasks Num", description: "Number of tasks", width: 96, minWidth: 88 },
    {
        field: "col6",
        headerName: "Progress",
        description: "Overall progress",
        width: 100,
        minWidth: 88,
        sortable: false,
        filterable: false,
        disableColumnMenu: true,
    },
    {
        field: "col7",
        headerName: "Analytics",
        description: "Subject analytics",
        width: 100,
        minWidth: 88,
        sortable: false,
        filterable: false,
        disableColumnMenu: true,
    },
];

type TaskSubjectRow = {
    id: number;
    col0: number;
    col1: string;
    col2: string;
    col3: string;
    col4: string;
    col5: number;
    col6: string;
    progressPercent: number;
    col7: string;
};

type Props = {
    subjects: IProject[];
};

const parseYearTerm = (folderPath?: string) => {
    const parts = (folderPath ?? "")
        .split(">")
        .map((x) => x.trim())
        .filter(Boolean);

    return {
        year: parts[1] ?? "",
        term: parts[2] ?? "",
    };
};

const TaskSubjectsDataGrid = ({ subjects }: Props) => {
    const rows: TaskSubjectRow[] = subjects.map((p) => {
        const { year, term } = parseYearTerm(p.folderPath);
        return {
            id: p.id,
            col0: p.id,
            col1: p.name,
            col2: p.description,
            col3: year,
            col4: term,
            col5: p.count || 0,
            col6: "",
            progressPercent: p.progressPercent ?? 0,
            col7: "",
        };
    });

    return (
        <div className="pb-4 mt-4 w-full min-w-0">
            <DataGrid
                className="bg-white relative h-full w-full min-w-0"
                initialState={{
                    sorting: {
                        sortModel: [{ field: "col1", sort: "asc" }],
                    },
                }}
                slots={{
                    row: TaskSubjectsDataGridRow,
                }}
                rows={rows}
                columns={taskSubjectColumns}
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
    );
};

export default TaskSubjectsDataGrid;

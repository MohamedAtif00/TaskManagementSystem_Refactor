import { useEffect, useState } from "react";
import API from "../../lib/API";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import Link from "next/link";
import Head from "next/head";
import Loader from "../../components/loader";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { DatePicker as MUIDatePicker } from "@mui/x-date-pickers/DatePicker";
import ProjectIcon from "../../assets/Icons/Project";
import "dayjs/locale/en-gb";

export function DatePicker({
    setValue,
    label,
}: {
    setValue: (d: Date) => void;
    label: string;
}) {
    return (
        <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale="en-gb">
            <MUIDatePicker
                disableHighlightToday
                label={label}
                onChange={(e: any) => setValue(e.$d)}
            />
        </LocalizationProvider>
    );
}

const columns: GridColDef[] = [
    { field: "col0", headerName: "ID", width: 90 },
    {
        field: "col1",
        headerName: "Name",
        width: 300,
        sortComparator: (A, B) => {
            const a = A.toLowerCase(),
                b = B.toLowerCase();
            console.log(a, b);
            return a > b ? 1 : b > a ? -1 : 0;
        },
    },
    { field: "col2", headerName: "Description", width: 200 },
    { field: "col3", headerName: "Year", width: 100 },
    { field: "col4", headerName: "Term", width: 100 },
    { field: "col5", headerName: "Idle", width: 100 },
    { field: "col6", headerName: "Running", width: 100 },
    { field: "col7", headerName: "Done", width: 100 },
    { field: "col8", headerName: "Total", width: 100 },
];

const Reports = () => {
    const [reports, setReports] = useState<Report[]>([]);
    const [loading, setLoading] = useState(true);
    const [start, setStart] = useState<Date>();
    const [end, setEnd] = useState<Date>();

    useEffect(() => {
        API.PROJECTS.REPORTS.GET_ALL({ start, end }).then((res) => {
            if (res && !res.error) setReports(res.data);
            setLoading(false);
        });
    }, [start, end]);

    if (loading)
        return (
            <>
                <Head>
                    <title>ATS - Loading</title>
                </Head>
                <div className="flex items-center justify-center mx-auto">
                    <Loader />
                </div>
            </>
        );

    if (reports === undefined)
        return (
            <div className="flex items-center justify-center mx-auto">
                <Head>
                    <title>ATS - Page not found</title>
                </Head>
                <div>Report is not found</div>
            </div>
        );

    return (
        <>
            <Head>
                <title>ATS - Reports</title>
            </Head>
            <div className="mx-auto relative max-h-screen overflow-y-auto pr-4">
                <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
                    <div className="flex items-center gap-3">
                        <div className="flex">
                            <ProjectIcon color={"#29313d"} />
                        </div>
                        <div className="text-2xl font-bold text-slate-800">
                            Reports
                        </div>
                    </div>
                </div>
                <div className="pb-4 mt-4 bg-white flex flex-col gap-2 rounded">
                    <div className="px-3 flex justify-between pt-4">
                        <div>
                            <DatePicker
                                label="Start Date"
                                setValue={setStart}
                            />
                        </div>
                        <div>
                            <DatePicker setValue={setEnd} label="End Date" />
                        </div>
                    </div>
                    <div>
                        <DataGrid
                            className="relative h-full"
                            initialState={{
                                sorting: {
                                    sortModel: [{ field: "col1", sort: "asc" }],
                                },
                            }}
                            slots={{
                                row: (r) => {
                                    return (
                                        <Link
                                            href={`/project-overview/${r.rowId}`}
                                            key={r.rowId}
                                        >
                                            <div
                                                style={{ height: r.rowHeight }}
                                                className="group hover:bg-slate-50 flex border-solid border-b border-slate-200"
                                            >
                                                {r.visibleColumns.map(
                                                    (c: any) => {
                                                        return (
                                                            <div
                                                                key={
                                                                    c.headerName
                                                                }
                                                                style={{
                                                                    minWidth:
                                                                        c.width,
                                                                    maxWidth:
                                                                        c.width,
                                                                }}
                                                                className="px-[0.625rem] group-hover:pl-4 transition-all ease-in text-sm flex items-center group-hover:text-blue-700"
                                                            >
                                                                {r.row[c.field]}
                                                            </div>
                                                        );
                                                    }
                                                )}
                                            </div>
                                        </Link>
                                    );
                                },
                            }}
                            rows={reports.map((p) => {
                                return {
                                    id: p.id,
                                    col0: p.id,
                                    col1: p.name,
                                    col2: p.description,
                                    col3: p.year,
                                    col4: p.term,
                                    col5: p.idleLearningObjectives,
                                    col6: p.runningLearningObjectives,
                                    col7: p.doneLearningObjectives,
                                    col8: p.totalLearningObjectives,
                                };
                            })}
                            columns={columns}
                        />
                    </div>
                </div>
            </div>
        </>
    );
};

export default Reports;

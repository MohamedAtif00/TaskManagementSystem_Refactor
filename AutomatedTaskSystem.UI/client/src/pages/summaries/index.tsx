import { useEffect, useState } from "react";
import { useAppDispatch, useAppSelector } from "../../app/hooks";
import Header from "../../components/header/header";
import API from "../../lib/API";
import { load } from "../../slices/projectSlice";
import Link from "next/link";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import Head from "next/head";
import Loader from "../../components/loader";

const columns: GridColDef[] = [
    { field: "col0", headerName: "ID", width: 90 },
    {
        field: "col1",
        headerName: "Name",
        width: 300,
        sortComparator: (A, B) => {
            const a = A.toLowerCase(),
                b = B.toLowerCase();
            return a > b ? 1 : b > a ? -1 : 0;
        },
    },
    { field: "col2", headerName: "Description", width: 300 },
    { field: "col3", headerName: "Year", width: 100 },
    { field: "col4", headerName: "Term", width: 100 },
];

const Projects = () => {
    const projects = useAppSelector((states) => states.projectSlice);
    const dispatch = useAppDispatch();
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        API.TASKS.PROJECTS().then((res) => {
            if (res && !res.error) {
                dispatch(load(res.data));
                setIsLoading(false);
            }
        });
    }, [dispatch]);

    if (isLoading)
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

    return (
        <>
            <Head>
                <title>ATS - Summaries</title>
            </Head>
            <div className="mx-auto relative max-h-screen overflow-y-auto pr-4">
                <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
                    <Header text="Summaries" icon="Project" />
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
                                    <Link
                                        href={{
                                            pathname: `/summaries/${r.rowId}`,
                                        }}
                                    >
                                        <div
                                            key={r.rowId}
                                            style={{ height: r.rowHeight }}
                                            className="group hover:bg-slate-50 flex border-solid border-b border-slate-200"
                                        >
                                            {r.visibleColumns.map((c: any) => {
                                                if (c.field === "col1")
                                                    return (
                                                        <div
                                                            key={c.headerName}
                                                            style={{
                                                                minWidth:
                                                                    c.width,
                                                                maxWidth:
                                                                    c.width,
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
                                        </div>
                                    </Link>
                                );
                            },
                        }}
                        rows={projects.map((p) => {
                            return {
                                id: p.id,
                                col0: p.id,
                                col1: p.name,
                                col2: p.description,
                                col3: p.year.name,
                                col4: p.term ? "Term 2" : "Term 1",
                            };
                        })}
                        columns={columns}
                    />
                </div>
            </div>
        </>
    );
};

export default Projects;

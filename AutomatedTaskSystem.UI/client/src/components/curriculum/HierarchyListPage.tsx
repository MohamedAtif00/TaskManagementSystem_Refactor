import Head from "next/head";
import Link from "next/link";
import { useRouter } from "next/router";
import { ReactNode } from "react";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import { UrlObject } from "url";
import ProjectIcon from "../../assets/Icons/Project";
import TaskIcon from "../../assets/Icons/Task";
import LeftArrowIcon from "../../assets/Icons/LeftArrow";
import TableAction from "../TableComponents/TableActionButton";
import CurriculumBreadcrumb, { Crumb } from "./CurriculumBreadcrumb";
import RightArrowIcon from "../../assets/Icons/RightArrow";

export type HierarchyRow = {
    id: number;
    col0: number | string;
    col1: string;
    col2?: string;
    col3?: string;
    col4?: string;
};

type Props = {
    title: string;
    pageTitle: string;
    breadcrumbs?: Crumb[];
    backHref?: string;
    addLabel?: string;
    addHref?: { pathname: string; query?: Record<string, string | number> };
    showAdd?: boolean;
    icon?: "project" | "task";
    rows: HierarchyRow[];
    columns: GridColDef[];
    rowHref: (id: number) => string;
    editHref?: (id: number) => UrlObject;
    children?: ReactNode;
};

const HierarchyListPage = ({
    title,
    pageTitle,
    breadcrumbs,
    backHref,
    addLabel,
    addHref,
    showAdd = true,
    icon = "project",
    rows,
    columns,
    rowHref,
    editHref,
    children,
}: Props) => {
    const router = useRouter();
    const HeaderIcon = icon === "task" ? TaskIcon : ProjectIcon;

    return (
        <div className="mx-auto relative max-h-screen overflow-y-auto pr-4">
            <Head>
                <title>TMS - {pageTitle}</title>
            </Head>
            {breadcrumbs && breadcrumbs.length > 0 && (
                <div className="pt-4 px-2">
                    <CurriculumBreadcrumb items={breadcrumbs} />
                </div>
            )}
            <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
                <div className="flex gap-3 items-center min-w-0">
                    {backHref && (
                        <Link
                            href={backHref}
                            className="shrink-0 p-1 rounded hover:bg-slate-100 text-slate-700"
                            aria-label="Go back"
                        >
                            <RightArrowIcon className="w-6 h-6 stroke-black" />
                        </Link>
                    )}
                    <div className="w-6 h-6 shrink-0">
                        <HeaderIcon className={icon === "task" ? "stroke-black" : undefined} />
                    </div>
                    <h1 className="font-bold text-2xl truncate">{title}</h1>
                </div>
                {showAdd && addHref && addLabel && (
                    <Link href={addHref}>
                        <button type="button" className="px-4 py-1 rounded bg-blue-600 text-white shrink-0">
                            {addLabel}
                        </button>
                    </Link>
                )}
            </div>
            <div className="pb-4 mt-4">
                <DataGrid
                    className="bg-white relative h-full"
                    rows={rows}
                    columns={columns}
                    initialState={{
                        sorting: { sortModel: [{ field: "col1", sort: "asc" }] },
                    }}
                    slots={{
                        row: (r) => {
                            const id = Number(r.rowId);
                            const href = rowHref(id);

                            return (
                                <div
                                    key={r.rowId}
                                    role="link"
                                    tabIndex={0}
                                    className="group hover:bg-slate-50 flex border-solid border-b border-slate-200 cursor-pointer"
                                    style={{ height: r.rowHeight, minHeight: r.rowHeight }}
                                    onClick={() => router.push(href)}
                                    onKeyDown={(e) => {
                                        if (e.key === "Enter" || e.key === " ") {
                                            e.preventDefault();
                                            router.push(href);
                                        }
                                    }}
                                >
                                    {r.visibleColumns.map((c: GridColDef) => {
                                        if (c.field === "colActions" && editHref) {
                                            return (
                                                <div
                                                    key="colActions"
                                                    style={{
                                                        minWidth: c.width,
                                                        maxWidth: c.width,
                                                        flex: c.flex ? 1 : undefined,
                                                    }}
                                                    className="px-[0.625rem] flex items-center justify-end"
                                                    onClick={(e) => e.stopPropagation()}
                                                >
                                                    <TableAction
                                                        text="Edit"
                                                        url={editHref(id)}
                                                        type="edit"
                                                    />
                                                </div>
                                            );
                                        }

                                        return (
                                            <div
                                                key={String(c.field)}
                                                style={{
                                                    minWidth: c.width,
                                                    maxWidth: c.width,
                                                    flex: c.flex ? 1 : undefined,
                                                }}
                                                className="px-[0.625rem] group-hover:pl-4 transition-all ease-in text-sm flex items-center group-hover:text-blue-700 min-w-0"
                                            >
                                                <span className="truncate">
                                                    {r.row[c.field as keyof typeof r.row] as string}
                                                </span>
                                            </div>
                                        );
                                    })}
                                </div>
                            );
                        },
                    }}
                    autoHeight
                />
            </div>
            {children}
        </div>
    );
};

export default HierarchyListPage;

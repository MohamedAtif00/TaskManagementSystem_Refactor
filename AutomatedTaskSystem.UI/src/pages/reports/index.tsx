import { useEffect, useState } from "react";
import API from "../../lib/API";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import Header from "../../components/header/header";
import Link from "next/link";

const columns: GridColDef[] = [
	{ field: "col0", headerName: "ID", width: 90 },
	{
		field: "col1",
		headerName: "Name",
		width: 300,
	},
	{ field: "col2", headerName: "Description", width: 300 },
	{ field: "col3", headerName: "Year", width: 100 },
	{ field: "col4", headerName: "Term", width: 100 },
	{ field: "col5", headerName: "Done", width: 100 },
	{ field: "col6", headerName: "Running", width: 100 },
];

const Reports = () => {
	const [reports, setReports] = useState<Report[]>([]);

	useEffect(() => {
		API.PROJECTS.REPORTS.GET_ALL().then(res => {
			if (res && !res.error) {
				setReports(res.data);
			}
		})
	}, []);

	return (
		<div className="mx-auto relative max-h-screen overflow-y-auto pr-4">
			<div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
				<Header text="Reports" icon="Project" />
			</div>
			<div className="pb-4 mt-4">
				<DataGrid
					className="bg-white relative h-full"
					slots={{
						row: (r) => {
							return (
								<Link
									href={`/reports/${r.rowId}`}
									key={r.rowId}
								>
									<div
										style={{ height: r.rowHeight }}
										className="group hover:bg-slate-50 flex border-solid border-b border-slate-200"
									>
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
							col5: p.doneLearningObjectives,
							col6: p.runningLearningObjectives
						};
					})}
					columns={columns}
				/>
			</div>
		</div>
	);
}

export default Reports;

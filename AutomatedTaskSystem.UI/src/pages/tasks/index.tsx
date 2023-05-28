import { useEffect, useState } from "react";
import API from "../../lib/API";
import Link from "next/link";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import TaskIcon from "../../assets/Icons/Task";

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
];

const Projects = () => {
	const [projects, setProjects] = useState<IProject[]>([]);

	useEffect(() => {
		API.TASKS.PROJECTS().then((res) => {
			if (res && !res.error) setProjects(res.data);
		});
	}, []);

	return (
		<div className="mx-auto relative max-h-screen overflow-y-auto pr-4">
			<div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
				<div className="flex gap-2 items-center">
					<TaskIcon className="stroke-black" />
					<h1 className="font-bold text-2xl ">Projects</h1>
				</div>
			</div>
			<div className="pb-4 mt-4">
				<DataGrid
					className="bg-white relative h-full"
					slots={{
						row: (r) => {
							return (
								<Link href={{ pathname: `/tasks/${r.rowId}` }}>
									<div
										key={r.rowId}
										style={{ height: r.rowHeight }}
										className="group hover:bg-slate-50 flex border-solid border-b border-slate-200"
									>
										{r.visibleColumns.map((c: any) => {
											if (c.field === "col1")
												return (
													<div
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
	);
};

export default Projects;

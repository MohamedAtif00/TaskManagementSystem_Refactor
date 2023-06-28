import { useEffect, useState } from "react";
import TaskIcon from "../../assets/Icons/Task";
import API from "../../lib/API";

const Reports = () => {
	const [reports, setReports] = useState<Report[]>([]);

	useEffect(() => {
		API.PROJECTS.REPORTS.GET_ALL().then(res => {
			if (res && !res.error) {
				setReports(res.data);
			}
		})
	}, []);

	useEffect(() => console.log(reports), [reports]);

	return (
		<div className="mx-auto relative max-h-screen overflow-y-hidden overflow-x-visible pr-4 flex flex-col">
			<div className="bg-white shrink-0 border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
				<div className="flex gap-2 items-center">
					<TaskIcon className="stroke-black" />
					<h1 className="font-bold text-2xl ">Reports</h1>
				</div>
			</div>
			<div className="grow overflow-y-scroll overflow-x-visible text-sm">
				<div className="relative my-12 py-4 bg-white rounded-md border-b border-solid border-slate-300 shadow-lg">
					<div className="sticky grid-cols-12 px-8 grid gap-4 items-center h-12 font-bold border-b border-solid border-slate-200">
						<div className="col-span-3">Name</div>
						<div className="col-span-3">Description</div>
						<div>Year</div>
						<div>Term</div>
						<div className="col-span-2 flex justify-end">Done</div>
						<div className="col-span-2 flex justify-end">Running</div>
					</div>
					{reports.map(r => <div key={r.id} className={"grid-cols-12 px-8 grid gap-4 items-center h-12 border-b border-solid border-slate-200"}>
						<div className="col-span-3">{r.name}</div>
						<div className="col-span-3">{r.description}</div>
						<div>{r.year}</div>
						<div>{r.term}</div>
						<div className="col-span-2 flex justify-end">{r.doneLearningObjectives}</div>
						<div className="col-span-2 flex justify-end">{r.runningLearningObjectives}</div>
					</div>)}
				</div>
			</div>
		</div>
	);
}

export default Reports;

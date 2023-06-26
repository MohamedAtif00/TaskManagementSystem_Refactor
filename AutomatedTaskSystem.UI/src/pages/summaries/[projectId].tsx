import Link from "next/link";
import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import NonActionableTaskDetails from "../../components/taskDetails/NonActionableTask";
import API from "../../lib/API";

interface _Task {
	id: number;
	name: string;
	status: string;
	statusId: number;
}

interface _LearningObjective {
	id: number;
	name: string;
	tag: string;
	environment: string;
	template: string;
	schema: { id: number; name: string };
	tasks: _Task[];
}

interface _Lesson {
	id: number;
	name: string;
	learningObjectives: _LearningObjective[];
}

interface _Unit {
	id: number;
	name: string;
	lessons: _Lesson[];
}

export interface IReport {
	id: number;
	name: string;
	description: string;
	units: _Unit[];
}

const Report = () => {
	const router = useRouter();
	const [report, setReport] = useState<IReport>();

	useEffect(() => {
		const id = router.query.projectId;
		if (id) API.PROJECTS.REPORT(id).then((res) => res && setReport(res));
	}, [router.query.projectId]);

	if (!report) {
		return <></>;
	}

	return (
		<div className="pl-8 overflow-hidden">
			<div className="pr-8 h-screen overflow-y-auto grow">
				<h1 className="text-3xl py-4">{report.name}</h1>
				<div className="relative whitespace-nowrap">
					<div className="sticky top-0 bg-slate-200 flex">
						<div className="basis-60 relative">
							<div className="sticky left-64">
								Learning Objective Name
							</div>
						</div>
						<div className="basis-40 border-gray-900">Template</div>
						<div className="basis-40 border-gray-900">Tag</div>
						<div className="basis-40 border-gray-900">
							Environment
						</div>
						<div className="basis-40 border-gray-900">Type</div>
						<div className="grow relative">
							<div className="sticky left-[30rem]">Tasks</div>
						</div>
					</div>
					{report.units.map((u) => {
						return (
							<div key={u.id + "name"}>
								<div className="text-xl">{u.name}</div>
								{u.lessons.map((l) => {
									return (
										<div key={l.id}>
											<div className="text-lg">
												{l.name}
											</div>
											{l.learningObjectives.map((lo) => (
												<div
													className="relative py-2 whitespace-nowrap transition-all ease-out"
													key={lo.id}
												>
													<div className="pr-4 py-1 sticky left-0 bg-slate-200 inline-flex items-center w-60 shrink-0">
														{lo.name}
													</div>
													<div className="px-4 py-1 sticky left-60 bg-slate-200 inline-flex items-center w-40 shrink-0">
														{lo.schema.name}
													</div>
													<div className="bg-slate-200 inline-flex items-center w-40 shrink-0">
														{lo.template
															? lo.template
															: "None"}
													</div>
													<div className="bg-slate-200 inline-flex items-center w-40 shrink-0">
														{lo.tag
															? lo.tag
															: "None"}
													</div>
													<div className="bg-slate-200 inline-flex items-center w-40 shrink-0">
														{lo.environment
															? lo.environment
															: "None"}
													</div>
													<div className="bg-slate-200 pr-4 inline-flex grow gap-3 flex-nowrap whitespace-nowrap">
														{lo.tasks.map((t) => (
															<Link
																key={t.id}
																href={{
																	pathname: `/summary/${report.id}`,
																	query: {
																		taskId: t.id,
																	},
																}}
															>
																<div
																	className={`px-4 py-1 rounded-3xl ${t.statusId ===
																			3
																			? "bg-orange-600 text-white"
																			: t.statusId ===
																				4
																				? "bg-emerald-400 text-white"
																				: "bg-slate-300"
																		}`}
																	key={t.id}
																>
																	{t.name}
																</div>
															</Link>
														))}
													</div>
												</div>
											))}
										</div>
									);
								})}
							</div>
						);
					})}
				</div>
				<NonActionableTaskDetails projectId={report.id} />
			</div>
		</div>
	);
};

export default Report;

import { useEffect, useState } from "react";
import API from "../../lib/API";
import { useRouter } from "next/router";
import Loader from "../../components/loader";
import ReportHeader from "../../components/pageComponent/reports/header";
import Head from "next/head";
import UnitReportItem from "../../components/pageComponent/reports/unitItem";

const ProjectReport = () => {
	const router = useRouter();
	const [loading, setLoading] = useState(true);
	const [report, setReport] = useState<DetailedReport>();

	useEffect(() => {
		const id = router.query.projectId;

		id && API.PROJECTS.REPORTS.GET_ONE(id).then(res => {
			if (res && !res.error)
				setReport(res.data);
			setLoading(false);
		}).catch(err => {
			console.error(err);
			setLoading(false);
		});
	}, [router.query])

	if (loading)
		return <div className="flex items-center justify-center mx-auto">
			<Head>
				<title>ATS - Loading</title>
			</Head>
			<Loader />
		</div>;

	if (report === undefined)
		return <div className="flex items-center justify-center mx-auto">
			<Head>
				<title>ATS - Page not found</title>
			</Head>
			<div>Report is not found</div>
		</div>;

	return (
		<div className="mx-auto w-10/12 bg-white flex flex-col gap-1">
			<Head>
				<title>{`ATS - ${report.name} Report`}</title>
			</Head>
			<ReportHeader name={report.name} running={report.runningLearningObjectives} done={report.doneLearningObjectives} />
			<div className="grow overflow-y-auto">
				{report.units.map(u => (
					<UnitReportItem
						key={u.id}
						name={u.name}
						running={u.runningLearningObjectives}
						done={u.doneLearningObjectives}
						lessons={u.lessons}
					/>
				))}
			</div>
		</div>
	);
}

export default ProjectReport;

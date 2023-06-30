import { useEffect, useState } from "react";
import API from "../../lib/API";
import { useRouter } from "next/router";
import Loader from "../../components/loader";
import ReportHeader from "../../components/pageComponent/reports/header";

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
			<Loader />
		</div>;

	if (report === undefined)
		return <div className="flex items-center justify-center mx-auto">
			<Loader />
		</div>;

	return (
		<div className="mx-auto w-10/12 bg-white flex">
			<ReportHeader name={report.name} running={report.runningLearningObjectives} done={report.doneLearningObjectives} />
		</div>
	);
}

export default ProjectReport;

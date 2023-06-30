import { useEffect } from "react";
import API from "../../lib/API";
import { useRouter } from "next/router";

const ProjectReport = () => {
	const router = useRouter();

	useEffect(() => {
		const id = router.query.projectId;

		id && API.PROJECTS.REPORTS.GET_ONE(id).then(res => {
			if (res && !res.error) {
				console.log(res.data);
			}
		});
	}, [router.query])

	return <div className="mx-auto w-10/12 bg-white"></div>;
}

export default ProjectReport;

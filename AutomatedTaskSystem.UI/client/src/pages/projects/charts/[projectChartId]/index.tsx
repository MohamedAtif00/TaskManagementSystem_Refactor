import { useEffect } from "react";
import { useRouter } from "next/router";
import Head from "next/head";

/**
 * Legacy URL: /projects/charts/[id] → analytics now live under the Tasks module.
 */
const ProjectChartsRedirect = () => {
  const router = useRouter();
  const { projectChartId } = router.query;

  useEffect(() => {
    if (!router.isReady || projectChartId === undefined) return;
    const id = Array.isArray(projectChartId) ? projectChartId[0] : projectChartId;
    router.replace(`/tasks/charts/${id}`);
  }, [router, router.isReady, projectChartId]);

  return (
    <>
      <Head>
        <title>ATS - Redirect</title>
      </Head>
    </>
  );
};

export default ProjectChartsRedirect;

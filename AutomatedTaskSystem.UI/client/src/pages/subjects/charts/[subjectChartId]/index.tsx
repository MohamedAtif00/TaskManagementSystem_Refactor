import { useEffect } from "react";
import { useRouter } from "next/router";
import Head from "next/head";

/**
 * Legacy URL: /subjects/charts/[id] → analytics now live under the Tasks module.
 */
const ProjectChartsRedirect = () => {
  const router = useRouter();
  const { subjectChartId } = router.query;

  useEffect(() => {
    if (!router.isReady || subjectChartId === undefined) return;
    const id = Array.isArray(subjectChartId) ? subjectChartId[0] : subjectChartId;
    router.replace(`/tasks/charts/${id}`);
  }, [router, router.isReady, subjectChartId]);

  return (
    <>
      <Head>
        <title>TMS - Redirect</title>
      </Head>
    </>
  );
};

export default ProjectChartsRedirect;

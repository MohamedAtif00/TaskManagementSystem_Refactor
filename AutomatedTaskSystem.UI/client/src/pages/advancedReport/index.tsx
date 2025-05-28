import Head from "next/head";
import { url } from "../../lib/API";
import { useAppSelector } from "../../app/hooks";

export default function AdvancedReport() {
    const auth = useAppSelector(x =>x.authSlice)
    return (
        <>
            <Head>
                <title>ATS - Advanced Report</title>
            </Head>
            <div className="w-full h-screen">
                <iframe

                    // src="http://172.20.9.30/Reports/report/Dashboard"    
                    // src="http://172.20.9.30/ssrs-proxy/Reports/report/Dashboard"
                    // src="http://172.20.9.30/api/Dashboard/ssrs-report"
                    // src="https://:44381/SSRSProxy?path=Dashboard"
                    // src="http://172.20.9.30/SSRSproxy/SSRSProxy"
                    src={`http://172.20.9.30/SSRSproxy/SSRSProxy?UserCode=${auth.id}&reportPath=/Navigations`}
                    // src="http://172.20.9.30/ReportServer/Pages/ReportViewer.aspx?/Dashboard&rs:Command=Render&rs:Format=HTML4.0"
                    // src={`${url}/dashboards/ssrs-report`}
                    // src={`${url}/dashboards/ssrs-report`}
                    //  src="http://test-app/Reports/report/Dashboard"
                    frameBorder="0"
                    className="w-full h-full"
                />
            </div>
        </>
    );  
}

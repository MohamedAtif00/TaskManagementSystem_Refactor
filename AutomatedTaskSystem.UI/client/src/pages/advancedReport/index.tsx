import Head from "next/head";
import { url } from "../../lib/API";

export default function AdvancedReport() {
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
                    // src="http://172.20.9.30/api/dashboards/ssrs-report"
                    // src="http://172.20.9.30/ReportServer/Pages/ReportViewer.aspx?/Dashboard&rs:Command=Render&rs:Format=HTML4.0"
                    // src={`${url}/dashboards/ssrs-report`}
                    src={`${url}/dashboards/ssrs-report`}
                    //  src="http://test-app/Reports/report/Dashboard"
                    frameBorder="0"
                    className="w-full h-full"
                />
            </div>
        </>
    );  
}

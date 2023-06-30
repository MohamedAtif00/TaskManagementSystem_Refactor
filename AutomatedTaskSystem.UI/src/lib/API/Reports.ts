import { url } from ".";

const Reports = {
	GET_ALL: async () => {
		try {
			const res = await fetch(`${url}/projects/reports`);
			const data: ResponseService<Report[]> = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	GET_ONE: async (id: number | string | string[]) => {
		try {
			const res = await fetch(`${url}/projects/${id}/reports`);
			const data: ResponseService<DetailedReport> = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	}
}

export default Reports;

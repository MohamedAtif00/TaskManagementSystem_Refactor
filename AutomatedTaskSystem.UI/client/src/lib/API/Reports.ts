import { url } from ".";

interface GET_ALL_PARAMS { start?: Date; end?: Date };

const Reports = {
	GET_ALL: async (params?: GET_ALL_PARAMS) => {
		try {
			const urlArr: string[] = [`${url}/projects/reports`];

			if (params) {
				const { start, end } = params;
				if (start !== undefined || end !== undefined)
					urlArr.push("?");

				if (
					start instanceof Date
					&& !isNaN(start.valueOf())
					&& end instanceof Date
					&& !isNaN(end.valueOf())
				) {
					urlArr.push(`start=${start.getMonth() + 1}-${start.getDate()}-${start.getFullYear()}`);
					urlArr.push("&");
					urlArr.push(`end=${end.getMonth() + 1}-${end.getDate()}-${end.getFullYear()}`);
				} else if (start instanceof Date
					&& !isNaN(start.valueOf()))
					urlArr.push(`start=${start.getMonth() + 1}-${start.getDate()}-${start.getFullYear()}`);
				else if (end instanceof Date
					&& !isNaN(end.valueOf()))
					urlArr.push(`end=${end.getMonth() + 1}-${end.getDate()}-${end.getFullYear()}`);
			}

			// console.log(urlArr.join(""))

			const res = await fetch(urlArr.join(""));
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

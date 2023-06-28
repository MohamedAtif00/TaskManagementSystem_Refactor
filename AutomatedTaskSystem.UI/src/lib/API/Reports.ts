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
	}
}

// public int Id { get; set; }
// public string Name { get; set; } = string.Empty;
// public string Description { get; set; } = string.Empty;
// public string Term { get; set; } = string.Empty;
// public string Year { get; set; } = string.Empty;
// public int RunningLearningObjectives { get; set; }
// public int DoneLearningObjectives { get; set; }

export default Reports;

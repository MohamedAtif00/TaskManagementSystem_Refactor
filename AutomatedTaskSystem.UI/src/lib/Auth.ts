import { url } from "./API";

const login = async (code: string) => {
	const res = await fetch(`${url}/auth/login`, {
		method: "POST",
		body: JSON.stringify({ code }),
		headers: {
			"Content-Type": "application/json",
		},
	});
	if (res.status >= 400) {
		return false;
	}
	const data: { data: string; error: boolean; message: string } =
		await res.json();
	if (!data.error) localStorage.setItem("access-token", data.data);
	return true;
};

const authHeader = () => {
	const token = localStorage.getItem("access-token");

	if (token) return { Authorization: `Bearer ${token}` };
	return false;
};

const logout = async () => {
	await fetch(`${url}/auth/logout`, {
		method: "POST",
	});
	localStorage.removeItem("access-token");
};

const getUser = async () => {
	try {
		const _authHeader = authHeader();
		if (_authHeader) {
			const res = await fetch(`${url}/auth/about-me`, {
				method: "POST",
				headers: {
					"Content-Type": "application/json",
					..._authHeader,
				},
			});
			if (res.status == 404) {
				await logout();
				return false;
			}
			const data: {
				data: {
					name: string;
					role: number;
					id: number;
					group: string;
				};
				error: boolean;
				message: string;
			} = await res.json();
			return data;
		}
		return false;
	} catch (error) {
		console.error(error);
		return false;
	}
};

const authService = {
	login,
	authHeader,
	logout,
	getUser,
};

export default authService;

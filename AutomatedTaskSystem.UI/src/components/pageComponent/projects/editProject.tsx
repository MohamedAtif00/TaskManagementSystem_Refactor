import { useRouter } from "next/router";
import InputTextField from "../../formComponents/InputTextField";
import { useEffect, useState } from "react";
import FormConclusion from "../../formComponents/FormConclusion";
import API from "../../../lib/API";
import { motion } from "framer-motion";
import { useAppDispatch } from "../../../app/hooks";
import { edit } from "../../../slices/projectSlice";
import Dropdown from "../../formComponents/DropDown";

const EditProject = () => {
	const dispatch = useAppDispatch();
	const { query, pathname, push: routerPush } = useRouter();
	const [active, setActive] = useState<boolean>(false);
	const [name, setName] = useState("");
	const [description, setDescription] = useState("");
	const [term, setTerm] = useState<null | { id: number; name: string }>(null);
	const [year, setYear] = useState<null | { id: number; name: string }>(null);
	const [years, setYears] = useState<{ id: number; name: string }[]>([]);
	const [error, setError] = useState("");

	useEffect(() => {
		if (query.form === "edit-project" && query.projectId) {
			API.PROJECTS.GET_ONE(query.projectId).then((res) => {
				if (res && !res.error) {
					setName(res.data.name);
					setDescription(res.data.description);
					setTerm(
						res.data.term
							? { id: 2, name: "Term 2" }
							: { id: 1, name: "Term 1" }
					);
					setYear(res.data.year);
				}
			});
			API.PROJECTS.YEARS.GET_ALL().then((res) => {
				if (res && !res.error) {
					setYears(res.data);
				}
			});
			return setActive(true);
		}
		setActive(false);
	}, [query]);

	const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
		e.preventDefault();
		setError("");

		if (name === "") return setError("Please enter name");
		if (year === null) return setError("Please select a year");
		if (term === null) return setError("Please select a term");

		API.PROJECTS.EDIT({
			id: query.projectId!,
			name,
			description,
			term: term.id === 1 ? false : true,
			year: year.id,
		}).then((res) => {
			if (res && !res.error) {
				dispatch(edit(res.data));
				routerPush(pathname);
			}
		});
	};

	if (active)
		return (
			<motion.div
				initial={{ backgroundColor: "#00000000" }}
				animate={{ backgroundColor: "#00000055", height: "auto" }}
				className="z-50 flex items-center justify-center fixed top-0 left-0 right-0 min-h-screen"
			>
				<motion.div
					initial={{ opacity: 0.1 }}
					animate={{ opacity: 1 }}
					className="bg-white px-5 py-4 basis-80 rounded-lg"
				>
					<h2 className="text-lg mb-5">Edit project</h2>
					<form
						onSubmit={handleSubmit}
						className="flex flex-col gap-8"
					>
						<div className="flex flex-col gap-2">
							<div className="text-red-600">{error}</div>
							<InputTextField
								label="Name"
								value={name}
								handleChange={setName}
							/>
							<InputTextField
								label="Description"
								value={description}
								handleChange={setDescription}
							/>
							<div className="grid grid-cols-2 gap-2">
								<div>
									<Dropdown
										value={year}
										handleChange={setYear}
										label="Year"
										options={years}
									/>
								</div>
								<div>
									<Dropdown
										value={term}
										handleChange={setTerm}
										label="Term"
										options={[
											{ id: 1, name: "Term 1" },
											{ id: 2, name: "Term 2" },
										]}
									/>
								</div>
							</div>
						</div>
						<FormConclusion submittable={true} />
					</form>
				</motion.div>
			</motion.div>
		);

	return <></>;
};

export default EditProject;

import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import FormConclusion from "../../formComponents/FormConclusion";
import API from "../../../lib/API";
import { motion } from "framer-motion";
import { useAppDispatch } from "../../../app/hooks";
import { remove } from "../../../slices/projectSlice";

const RemoveProject = () => {
	const dispatch = useAppDispatch();
	const { query, pathname, push: routerPush } = useRouter();
	const [active, setActive] = useState<boolean>(false);
	const [project, setProject] = useState<IProject>();

	useEffect(() => {
		if (query.form === "remove-project" && query.projectId) {
			API.PROJECTS.GET_ONE(query.projectId.toString()).then((res) => {
				if (res && !res.error) setProject(res.data);
			});
			return setActive(true);
		}
		setActive(false);
	}, [query]);

	const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
		e.preventDefault();

		project &&
			API.PROJECTS.DELETE(project.id).then((res) => {
				if (res && !res.error) {
					dispatch(remove(project));
					routerPush(pathname);
				}
			});
	};

	if (active)
		return (
			<motion.div
				initial={{ backgroundColor: "#00000000" }}
				animate={{ backgroundColor: "#00000055" }}
				className="z-50 flex items-center justify-center fixed top-0 left-0 right-0 min-h-screen"
			>
				<motion.div
					initial={{ opacity: 0.1, height: "10rem" }}
					animate={{ opacity: 1, height: "12rem" }}
					className="bg-white px-5 py-4 basis-80 rounded-lg flex flex-col justify-between"
				>
					{project ? (
						<>
							<h2 className="text-lg">Delete Project</h2>
							<div>
								About to delete{" "}
								<span className="font-bold text-red-700">
									{project.name}
								</span>
							</div>
							<form onSubmit={handleSubmit}>
								<FormConclusion
									submittable={true}
									type="danger"
									text={{
										save: "Archive",
									}}
								/>
							</form>
						</>
					) : (
						<></>
					)}
				</motion.div>
			</motion.div>
		);

	return <></>;
};

export default RemoveProject;

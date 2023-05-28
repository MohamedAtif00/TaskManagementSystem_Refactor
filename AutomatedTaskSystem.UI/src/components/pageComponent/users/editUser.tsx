import { useRouter } from "next/router";
import InputTextField from "../../formComponents/InputTextField";
import { useEffect, useState } from "react";
import FormConclusion from "../../formComponents/FormConclusion";
import Dropdown from "../../formComponents/DropDown";
import API from "../../../lib/API";
import { edit } from "../../../slices/userSlice";
import { motion } from "framer-motion";
import { useAppDispatch } from "../../../app/hooks";

const EditUser = () => {
	const dispatch = useAppDispatch();
	const { query, push: routerPush, pathname } = useRouter();
	const [active, setActive] = useState<boolean>(false);
	const [name, setName] = useState("");
	const [error, setError] = useState("");
	const [group, setGroup] = useState<{ id: number; name: string } | null>(
		null
	);
	const [groups, setGroups] = useState<{ id: number; name: string }[]>([]);
	const [role, setRole] = useState<{ id: number; name: string } | null>(null);

	useEffect(() => {
		if (query.form === "edit-user" && query.userId) {
			API.RESOURCES.USERS.GET_ONE(query.userId.toString()).then((res) => {
				if (res && !res.error) {
					setName(res.data.name);
					setGroup(res.data.group);
					setRole(res.data.role);
				}
			});
			return setActive(true);
		}
		setActive(false);
	}, [query]);

	useEffect(() => {
		active &&
			API.RESOURCES.GROUPS.GET_ALL_MINI().then((res) => {
				if (res && !res.error) return setGroups(res.data);
			});
	}, [active]);

	const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
		e.preventDefault();
		setError("");

		if (name === "") return setError("Please enter name");
		if (group === null) return setError("Please select a group");
		if (role === null) return setError("Please select a role");

		API.RESOURCES.USERS.EDIT({
			id: query.userId!.toString(),
			name,
			groupId: group.id,
			roleId: role.id,
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
					<h2 className="text-lg mb-5">Edit user</h2>
					<form
						onSubmit={handleSubmit}
						className="flex flex-col gap-8"
					>
						<div className="gap-2 flex flex-col">
							<div className="text-red-600">{error}</div>
							<InputTextField
								label="Name"
								value={name}
								handleChange={setName}
							/>
							<Dropdown
								label="Group"
								value={group}
								options={groups}
								handleChange={setGroup}
							/>
							<Dropdown
								label="Role"
								value={role}
								options={[
									{ id: 1, name: "Project Manager" },
									{ id: 2, name: "Section Head" },
									{ id: 3, name: "Team Leader" },
									{ id: 4, name: "Member" },
								]}
								handleChange={setRole}
							/>
						</div>
						<FormConclusion submittable={true} />
					</form>
				</motion.div>
			</motion.div>
		);

	return <></>;
};

export default EditUser;

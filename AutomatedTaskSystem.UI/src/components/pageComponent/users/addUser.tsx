import { useRouter } from "next/router";
import InputTextField from "../../formComponents/InputTextField";
import { useEffect, useState } from "react";
import FormConclusion from "../../formComponents/FormConclusion";
import Dropdown from "../../formComponents/DropDown";
import API from "../../../lib/API";
import { motion } from "framer-motion";
import { useAppDispatch } from "../../../app/hooks";
import { add } from "../../../slices/userSlice";
import Link from "next/link";

const AddUser = () => {
	const dispatch = useAppDispatch();
	const { query, pathname } = useRouter();
	const [active, setActive] = useState<boolean>(false);
	const [name, setName] = useState("");
	const [error, setError] = useState("");
	const [group, setGroup] = useState<{ id: number; name: string } | null>(
		null
	);
	const [groups, setGroups] = useState<{ id: number; name: string }[]>([]);
	const [role, setRole] = useState<{ id: number; name: string } | null>(null);

	const [done, setDone] = useState<{ code: string; user: IUser } | null>(
		null
	);

	useEffect(() => {
		if (query.form === "add-user") return setActive(true);
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

		API.RESOURCES.USERS.CREATE({
			name,
			groupId: group.id,
			roleId: role.id,
		}).then((res) => {
			if (res && !res.error) {
				setDone(res.data);
				dispatch(add(res.data.user));
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
					{done ? (
						<div className="flex flex-col gap-4">
							<h3>
								{done.user.role.name}{" "}
								<span className="font-bold">
									{done.user.name}
								</span>{" "}
								is added as an{" "}
								<span className="font-bold">
									{done.user.group.name}
								</span>
							</h3>
							<div>
								<div>Code:</div>
								<div className="font-bold text-2xl text-center">
									{done.code}
								</div>
							</div>
							<div className="flex justify-center">
								<Link href={{ pathname }}>
									<button className="h-10 bg-black text-white font-bold w-1/2">
										Done
									</button>
								</Link>
							</div>
						</div>
					) : (
						<>
							<h2 className="text-lg mb-5">Add new user</h2>
							<form
								onSubmit={handleSubmit}
								className="flex flex-col gap-2"
							>
								<motion.div>
									{error !== "" && (
										<div className="text-red-600">
											{error}
										</div>
									)}
								</motion.div>
								<InputTextField
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
								<FormConclusion submittable={true} />
							</form>
						</>
					)}
				</motion.div>
			</motion.div>
		);

	return <></>;
};

export default AddUser;

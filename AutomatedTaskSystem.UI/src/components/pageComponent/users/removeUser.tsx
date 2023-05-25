import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import FormConclusion from "../../formComponents/FormConclusion";
import API from "../../../lib/API";
import { motion } from "framer-motion";
import { useAppDispatch } from "../../../app/hooks";
import { remove } from "../../../slices/userSlice";

const RemoveUser = () => {
	const dispatch = useAppDispatch();
	const { query, pathname, push: routerPush } = useRouter();
	const [active, setActive] = useState<boolean>(false);
	const [user, setUser] = useState<IUser>();

	useEffect(() => {
		if (query.form === "remove-user" && query.userId) {
			API.RESOURCES.USERS.GET_ONE(query.userId.toString()).then((res) => {
				if (res && !res.error) setUser(res.data);
			});
			return setActive(true);
		}
		setActive(false);
	}, [query]);

	const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
		e.preventDefault();

		user &&
			API.RESOURCES.USERS.DELETE({ id: user.id }).then((res) => {
				if (res && !res.error) {
					dispatch(remove(user));
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
					initial={{ opacity: 0.1, height: "10rem" }}
					animate={{ opacity: 1, height: "12rem" }}
					className="bg-white px-5 py-4 basis-80 rounded-lg flex-col flex justify-between"
				>
					{user ? (
						<>
							<h2 className="text-lg">Delete user</h2>
							<div>
								About to delete{" "}
								<span className="font-bold text-red-700">
									{user.name}
								</span>
							</div>
							<form onSubmit={handleSubmit}>
								<FormConclusion
									submittable={true}
									danger
									text={{
										save: "DELETE",
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

export default RemoveUser;

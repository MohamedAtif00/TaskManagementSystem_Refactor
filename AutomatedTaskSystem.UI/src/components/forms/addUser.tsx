import styles from "./styles.module.scss";
import Backdrop from "./backdrop";
import React, { useEffect, useState } from "react";
import FormField from "./field";
import { useRouter } from "next/router";
import API from "../../lib/API";
import { useAppDispatch, useAppSelector } from "../../app/hooks";
import * as groupSlice from "../../slices/groupSlice";
import * as userSlice from "../../slices/userSlice";
import Dropdown from "./dropdown";

const AddUser = () => {
	const [submittable, setSubmittable] = useState(false);
	const [name, setName] = useState("");
	const [groupId, setGroupId] = useState(0);
	const [roleId, setRoleId] = useState(0);
	const [active, setActive] = useState(false);
	const router = useRouter();
	const dispatch = useAppDispatch();
	const groups = useAppSelector((state) => state.groupsSlice);
	const [done, setDone] = useState({
		status: false,
		code: "",
		name: "",
		group: "",
	});

	useEffect(() => {
		const _active = router.query.form === "user";
		setActive(_active);
		if (!_active) {
			setName("");
			setGroupId(0);
			setRoleId(0);
		}
	}, [router]);

	useEffect(() => {
		API.RESOURCES.GROUPS.GET_ALL().then((res) => {
			if (res && !res.error) {
				dispatch(groupSlice.load(res.data));
			}
		});
	}, [dispatch]);

	useEffect(() => {
		if (name === "" || groupId === 0) {
			setSubmittable(false);
		} else {
			setSubmittable(true);
		}
	}, [setSubmittable, name, groupId]);

	const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
		e.preventDefault();
		if (submittable)
			API.RESOURCES.USERS.CREATE({ name, groupId, roleId }).then(
				(res) => {
					if (res && !res.error) {
						dispatch(userSlice.add(res.data.user));
						setDone({
							code: res.data.code,
							name: res.data.user.name,
							group: res.data.user.group.name,
							status: true,
						});
					}
				}
			);
	};

	if (active)
		return (
			<Backdrop mainRoute="/resources/users">
				{done.status ? (
					<div className={styles.form}>
						<h2>{done.name}</h2>
						<p className={styles.center}>was added</p>
						<p className={styles.userInfo}>
							{done.name} is a{" "}
							<span className={styles.bold}>{done.group}</span>
						</p>
						<p className={styles.loginCode}>Log in Code</p>
						<h1>
							{done.code.slice(0, done.code.length / 2)}-
							{done.code.slice(done.code.length / 2)}
						</h1>
						<button
							className={styles.submit}
							onClick={() => {
								router.back();
								setDone({
									status: false,
									code: "",
									name: "",
									group: "",
								});
							}}
						>
							Okay
						</button>
					</div>
				) : (
					<div className={styles.form}>
						<form onSubmit={handleSubmit}>
							<div className={styles.inputs}>
								<FormField
									label="Name"
									onChange={setName}
									value={name}
								/>
								<Dropdown
									label="Group"
									options={groups}
									id={groupId}
									handleChange={setGroupId}
								/>
								<Dropdown
									label="Role"
									options={[
										{
											id: 1,
											name: "Project Manager",
										},
										{
											id: 2,
											name: "Section Head",
										},
										{
											id: 3,
											name: "Team Leader",
										},
										{
											id: 4,
											name: "Member",
										},
									]}
									id={roleId}
									handleChange={setRoleId}
								/>
							</div>
							<div>
								<input
									type="submit"
									value="Add"
									className={[
										styles.submit,
										submittable ? "" : styles.inactive,
									].join(" ")}
								/>
							</div>
						</form>
					</div>
				)}
			</Backdrop>
		);

	return <></>;
};

export default AddUser;

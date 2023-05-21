import { useRouter } from "next/router";
import React, { useEffect, useState } from "react";
import Backdrop from "./backdrop";
import Dropdown from "./dropdown";
import FormField from "./field";
import SelectList from "./selectList";
import styles from "./styles.module.scss";

interface Props {
	users: { id: number; name: string }[];
	groups: { id: number; name: string }[];
	handleSubmit: (data: {
		name: string;
		headId: number;
		groups: number[];
	}) => void;
}

const AddSection = (props: Props) => {
	const router = useRouter();
	const [active, setActive] = useState(false);
	const [submittable, setSubmittable] = useState(false);
	const [head, setHead] = useState(0);
	const [groups, setGroups] = useState<number[]>([]);
	const [name, setName] = useState("");

	useEffect(() => {
		const _active = router.query.form === "section";
		setActive(_active);
		if (!_active) {
			setName("");
			setGroups([]);
			setHead(0);
			setSubmittable(false);
		}
	}, [router]);

	useEffect(() => {
		if (name === "" || groups.length === 0 || head === 0)
			setSubmittable(false);
		else setSubmittable(true);
	}, [groups, name, head]);

	if (active)
		return (
			<Backdrop mainRoute="/resources/sections">
				<div className={styles.form}>
					<form
						onSubmit={(e) => {
							e.preventDefault();
							props.handleSubmit({ groups, headId: head, name });
						}}
					>
						<div className={styles.inputs}>
							<FormField
								label="Name"
								onChange={setName}
								value={name}
							/>
							<Dropdown
								handleChange={setHead}
								id={head}
								label="Section Head"
								options={props.users}
							/>
							<SelectList
								label="Groups"
								list={props.groups}
								selected={groups}
								updateList={(n: number) =>
									setGroups((ps) => {
										const newState: number[] = [];
										const foundIndex = ps.findIndex(
											(g) => g == n
										);
										if (foundIndex >= 0) {
											ps.forEach((v, i) => {
												if (i !== foundIndex) {
													newState.push(v);
												}
											});
											return newState;
										}
										ps.forEach((g) => {
											newState.push(g);
										});
										newState.push(n);
										return newState;
									})
								}
							/>
						</div>
						<div>
							<input
								type="submit"
								value="Add"
								className={[
									styles.submit,
									!submittable ? styles.inactive : "",
								].join(" ")}
							/>
						</div>
					</form>
				</div>
			</Backdrop>
		);

	return <></>;
};

export default AddSection;

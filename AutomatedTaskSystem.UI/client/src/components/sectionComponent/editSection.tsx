import React, { useEffect, useState } from "react";
import FormField from "../forms/field";
import Dropdown from "../forms/dropdown";
import SelectList from "../forms/selectList";
import styles from "../forms/styles.module.scss";

interface Props {
	section: {
		id: number;
		name: string;
		head: { id: number; name: string };
		groups: { id: number; name: string }[];
	};
	users: { id: number; name: string }[];
	groups: { id: number; name: string }[];
	handleSubmit: (data: {
		id: number;
		name: string;
		headId: number;
		groups: number[];
	}) => void;
	onClose: () => void;
}

const EditSection = ({
	section,
	users,
	groups,
	handleSubmit,
	onClose,
}: Props) => {
	const [name, setName] = useState(section.name);
	const [head, setHead] = useState(section.head.id);
	const [selectedGroups, setSelectedGroups] = useState<number[]>(
		section.groups.map((g) => g.id)
	);
	const [submittable, setSubmittable] = useState(false);

	useEffect(() => {
		setName(section.name);
		setHead(section.head.id);
		setSelectedGroups(section.groups.map((g) => g.id));
	}, [section]);

	useEffect(() => {
		setSubmittable(name.trim() !== "" && head > 0 && selectedGroups.length > 0);
	}, [name, head, selectedGroups]);

	return (
		<div
			id={styles.modal}
			onClick={(e) => {
				if (e.currentTarget === e.target) onClose();
			}}
		>
			<div className={[styles.form, styles.elevated].join(" ")}>
				<h2>Edit Section</h2>
				<form
					onSubmit={(e) => {
						e.preventDefault();
						handleSubmit({
							id: section.id,
							name,
							headId: head,
							groups: selectedGroups,
						});
					}}
				>
					<div className={styles.inputs}>
						<FormField label="Name" onChange={setName} value={name} />
						<Dropdown
							handleChange={setHead}
							id={head}
							label="Section Head"
							options={users}
						/>
						<SelectList
							label="Groups"
							list={groups}
							selected={selectedGroups}
							updateList={(n: number) =>
								setSelectedGroups((ps) => {
									const foundIndex = ps.findIndex((g) => g === n);
									if (foundIndex >= 0) {
										return ps.filter((g) => g !== n);
									}
									return [...ps, n];
								})
							}
						/>
					</div>
					<div className={styles.actions}>
						<button
							type="button"
							onClick={onClose}
							className={styles.cancel}
						>
							Cancel
						</button>
						<input
							type="submit"
							value="Save"
							disabled={!submittable}
							className={[
								styles.submit,
								!submittable ? styles.inactive : "",
							].join(" ")}
						/>
					</div>
				</form>
			</div>
		</div>
	);
};

export default EditSection;

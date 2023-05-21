import styles from "../../styles.module.scss";
import Backdrop from "../../backdrop";
import React, { useEffect, useState } from "react";
import FormField from "../../field";
import SelectNodesWithToggle from "../../selectListWithToggle";
import SelectList from "../../selectList";
import API from "../../../../lib/API";
import { useRouter } from "next/router";

interface INodeLocal {
	id: number;
	name: string;
}

const EditNode = ({
	schemaId,
	node,
	nodes,
	updateList
}: {
	schemaId: number;
	node: INode;
	nodes: INodeLocal[];
	updateList: () => void;
}) => {
	const [submittable, setSubmittable] = useState(false);
	const [name, setName] = useState(node.name);
	const [start, setStart] = useState(node.isStart);
	const [previous, setPrevious] = useState<number[]>(node.previous.map(n => n.id));
	const [requires, setRequires] = useState<number[]>(node.requires.map(n => n.id));
	const router = useRouter();

	const updatePrevious = (nodeId: number) => {
		const foundIndex = previous.findIndex((_n) => _n === nodeId);
		if (foundIndex === -1) {
			return setPrevious((ps) => [...ps, nodeId]);
		}
		const foundRequires = requires.findIndex((_n) => _n === nodeId);
		if (foundRequires !== -1) {
			setRequires((ps) => [
				...ps.slice(0, foundRequires),
				...ps.slice(foundRequires + 1),
			]);
		}
		return setPrevious((ps) => [
			...ps.slice(0, foundIndex),
			...ps.slice(foundIndex + 1),
		]);
	};

	const updateRequires = (nodeId: number) =>
		setRequires((ps) => {
			const foundIndex = ps.findIndex((_n) => _n == nodeId);
			if (foundIndex == -1) {
				return [...ps, nodeId];
			}
			return [...ps.slice(0, foundIndex), ...ps.slice(foundIndex + 1)];
		});

	const updateStart = () => {
		setStart((ps) => !ps);
		if (!start) {
			setPrevious([]);
			setRequires([]);
		}
	};

	useEffect(() => {
		if (name === "") setSubmittable(false);
		else if (!start && previous.length === 0) setSubmittable(false);
		else setSubmittable(true);
	}, [start, previous, name]);

	const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
		e.preventDefault();
		if (submittable)
			API.SCHEMAS.NODES.EDIT({
				id: node.id,
				name,
				isStart: start,
				previous,
				requires,
				schemaId,
			}).then((res) => {
				if (res) {
					updateList();
					router.back();
				}
			});
	};

	return (
		<Backdrop mainRoute={`/schemas/${schemaId}`}>
			<div
				className={[styles.form, styles.center].join(" ")}
				onClick={(e) => e.stopPropagation()}
			>
				<form onSubmit={handleSubmit}>
					<div className={styles.inputs}>
						<FormField
							label="Name"
							onChange={setName}
							value={name}
						/>
						<SelectNodesWithToggle
							label="Previous"
							list={nodes}
							updateList={updatePrevious}
							selected={previous}
							checkbox={start}
							toggle={updateStart}
						/>
						<SelectList
							label="Requires"
							list={nodes.filter((_n) =>
								previous.includes(_n.id)
							)}
							selected={requires}
							updateList={updateRequires}
						/>
					</div>
					<div>
						<input
							type="submit"
							value="Save"
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
};

export default EditNode;

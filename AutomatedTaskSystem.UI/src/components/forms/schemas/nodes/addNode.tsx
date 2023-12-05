import styles from "../../styles.module.scss";
import Backdrop from "../../backdrop";
import React, { useEffect, useState } from "react";
import FormField from "../../field";
import { useRouter } from "next/router";
import API from "../../../../lib/API";
import SelectNodesWithToggle from "../../selectListWithToggle";

interface INodeLocal {
	id: number;
	name: string;
}

const AddNode = ({
	schemaId,
	nodes,
	updateList
}: {
	schemaId: number;
	nodes: INodeLocal[];
	updateList: () => void
}) => {
	const [submittable, setSubmittable] = useState(false);
	const [active, setActive] = useState(false);
	const [name, setName] = useState("");
	const [start, setStart] = useState(false);
	const [previous, setPrevious] = useState<number[]>([]);
	const router = useRouter();

	const updatePrevious = (nodeId: number) => {
		const foundIndex = previous.findIndex((_n) => _n === nodeId);
		if (foundIndex === -1) {
			return setPrevious((ps) => [...ps, nodeId]);
		}
		return setPrevious((ps) => [
			...ps.slice(0, foundIndex),
			...ps.slice(foundIndex + 1),
		]);
	};


	const updateStart = () => {
		setStart((ps) => !ps);
		if (!start)
			setPrevious([]);
	};

	useEffect(() => {
		if (name === "") {
			setSubmittable(false);
		} else if (!start && previous.length === 0) {
			setSubmittable(false);
		} else {
			setSubmittable(true);
		}
	}, [start, previous, name]);

	useEffect(() => {
		const _active = router.query.form === "node";
		setActive(_active);
		if (!_active) {
			setName("");
			setStart(false);
			setPrevious([]);
		}
	}, [router]);

	const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
		e.preventDefault();
		if (submittable)
			API.SCHEMAS.NODES.ADD({
				name,
				isStart: start,
				previous,
				schemaId,
			}).then((res) => {
				if (res) {
					updateList();
					router.push(`/schemas/${schemaId}`);
				}
			});
	};

	if (active)
		return (
			<Backdrop mainRoute={`/schemas/${schemaId}`}>
				<div
					className={styles.form}
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

export default AddNode;

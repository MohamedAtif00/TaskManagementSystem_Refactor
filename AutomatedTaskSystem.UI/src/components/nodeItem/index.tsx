import Link from "next/link";
import { useRouter } from "next/router";
import { useEffect, useRef, useState } from "react";
import { useAppDispatch, useAppSelector } from "../../app/hooks";
import PlusIcon from "../../assets/Icons/Plus";
import API from "../../lib/API";
import { editStep, removeStep } from "../../slices/nodesSlice";
import QueryButton from "../button/queryButton";
import styles from "./styles.module.scss";
import React from "react";

interface Props {
	schemaId: number;
	id: number;
	name: string;
	previous: { name: string; id: number }[];
	requires: { name: string; id: number }[];
	isStart: boolean;
	steps: IStep[];
	updateNodes: () => void;
}

const NodeItem = ({
	schemaId,
	id,
	name,
	isStart,
	previous,
	requires,
	steps,
	updateNodes,
}: Props) => {
	const auth = useAppSelector((s) => s.authSlice);

	const handleDelete = () =>
		API.SCHEMAS.NODES.DELETE(id).then((res) => res && updateNodes());

	return (
		<div className={styles.node}>
			<div className="p-2">
				<div className={styles.nodeTitle}>
					<div>{name}</div>
					{auth.role == 1 ? (
						<div className="flex gap-2">
							<div>
								<button
									onClick={handleDelete}
									className="text-base gap-2 font-normal px-3 rounded text-rose-500 flex items-center justify-center py-1 hover:text-white hover:bg-rose-500 transition-all ease-out"
								>
									Delete
								</button>
							</div>
							<QueryButton
								text="Edit"
								url={{
									pathname: `/schemas/${schemaId}`,
									query: {
										form: "nodeEdit",
										nodeId: id,
									},
								}}
							/>
							<QueryButton
								icon={<PlusIcon />}
								text="Step"
								url={{
									pathname: `/schemas/${schemaId}`,
									query: {
										form: "step",
										nodeId: id,
									},
								}}
								iconLeft
								iconRight={false}
							/>
						</div>
					) : (
						""
					)}
				</div>
				<div>
					{isStart ? (
						<div>Start Point</div>
					) : (
						<>
							<div>
								Previous: {previous.map((p) => `${p.name} `)}
							</div>
							{requires.length !== 0 ? (
								<div>
									Requires:{" "}
									{requires.map((r) => `${r.name} `)}
								</div>
							) : (
								""
							)}
						</>
					)}
				</div>
			</div>
			<div>
				<table className={styles.steps}>
					<thead>
						<tr className={styles.tableHead}>
							<th>Review</th>
							<th>Team Leader</th>
							<th>Name</th>
							<th>Priority</th>
							<th>Groups</th>
							<th>Duration</th>
							<th></th>
						</tr>
					</thead>
					<tbody>
						{steps.map((s) => {
							return (
								<Step
									priority={s.priority}
									key={s.id}
									nodeId={id}
									TL={s.tl}
									group={s.group}
									id={s.id}
									name={s.name}
									reviewable={s.reviewable}
									duration={s.duration}
								/>
							);
						})}
					</tbody>
				</table>
			</div>
		</div>
	);
};

interface TogglePrioProps {
	x: number;
	y: number;
	id: number;
	exit: () => void;
	parent: EventTarget;
}

const TogglePriorityForm = (props: TogglePrioProps): React.JSX.Element => {
	const dropDownRef = useRef<HTMLDivElement>(null);
	const dispatch = useAppDispatch();
	const [{ x, y }, setPos] = useState<{ x: number; y: number }>({
		x: props.x,
		y: props.y,
	});

	useEffect(() => {
		const handleScrollAndResize = () => {
			if (props.parent instanceof Element) {
				const newBounds = props.parent.getBoundingClientRect();

				setPos({
					x: newBounds.x + newBounds.width,
					y: newBounds.y,
				});
			}
		};

		const container = document.querySelector(".mainContainer");

		if (!container) return;

		container.addEventListener("scroll", handleScrollAndResize);
		container.addEventListener("resize", handleScrollAndResize);

		return () => {
			container.removeEventListener("scroll", handleScrollAndResize);
			container.removeEventListener("resize", handleScrollAndResize);
		};
	}, [props.parent]);

	const updatePrio = (priority: number | null) => {
		API.SCHEMAS.NODES.STEPS.UPDATE_PRIO({
			stepId: props.id,
			priority,
		}).then((res) => {
			if (res && !res.error) {
				dispatch(editStep({ step: res.data }));
			}
		});
		props.exit();
	};

	return (
		<div
			ref={dropDownRef}
			className="font-bold text-white fixed px-1 py-1 z-50 bg-white flex flex-col gap-1 w-28 rounded-xl border border-solid border-slate-500 cursor-auto"
			style={{ top: y, left: x }}
			onClick={(e) => e.stopPropagation()}
		>
			<div
				onClick={() => updatePrio(null)}
				className="rounded-full py-1 text-black bg-white hover:bg-slate-200 text-center"
			>
				None
			</div>
			<div
				onClick={() => updatePrio(3)}
				className="rounded-full py-1 bg-blue-600 hover:bg-blue-500 text-center"
			>
				Low
			</div>
			<div
				onClick={() => updatePrio(2)}
				className="rounded-full py-1 bg-orange-600 hover:bg-orange-500 text-center"
			>
				Medium
			</div>
			<div
				onClick={() => updatePrio(1)}
				className="rounded-full py-1 bg-rose-600 hover:bg-rose-500 text-center"
			>
				High
			</div>
		</div>
	);
};

const Step = ({
	priority,
	nodeId,
	id,
	TL,
	group,
	name,
	reviewable,
	duration,
}: {
	priority: number | null;
	nodeId: number;
	id: number;
	TL: boolean;
	reviewable: boolean;
	name: string;
	group: { id: number; name: string };
	duration: number;
}) => {
	const router = useRouter();
	const [deleting, setDeleting] = useState(false);
	const [editing, setEditng] = useState(false);
	const [prioEdit, setPrioEdit] = useState<TogglePrioProps>();
	const dispatch = useAppDispatch();
	const auth = useAppSelector((s) => s.authSlice);

	const rest =
		deleting && editing
			? () => {
				setDeleting(false);
				setEditng(false);
			}
			: deleting
				? () => setDeleting(false)
				: editing
					? () => setEditng(false)
					: undefined;

	return (
		<tr key={id} className={styles.step} onMouseLeave={rest}>
			<td>{reviewable ? "True" : ""}</td>
			<td>{TL ? "True" : ""}</td>
			<td className={styles.name}>{name}</td>
			<td
				className="flex items-center justify-start select-none cursor-pointer"
				onClick={(e) => {
					if (prioEdit) return setPrioEdit(undefined);

					const bounds = e.currentTarget.getBoundingClientRect();
					setPrioEdit({
						id,
						x: bounds.x + bounds.width,
						y: bounds.y,
						exit: () => setPrioEdit(undefined),
						parent: e.currentTarget,
					});
				}}
			>
				{priority === 1 ? (
					<div className="font-bold px-3 bg-rose-600 text-white rounded-full border-2 border-white border-opacity-50 border-solid">
						High
					</div>
				) : priority === 2 ? (
					<div className="font-bold px-3 bg-orange-500 text-white rounded-full border-2 border-white border-opacity-50 border-solid">
						Medium
					</div>
				) : priority === 3 ? (
					<div className="font-bold px-3 bg-blue-600 text-white rounded-full border-2 border-white border-opacity-50 border-solid">
						Low
					</div>
				) : (
					<div>None</div>
				)}
				{prioEdit && <TogglePriorityForm {...prioEdit} />}
			</td>
			<td>{group.name}</td>
			<td>
				{Math.floor(duration / 60)}:
				{duration % 60 < 10 ? `0${duration % 60}` : duration % 60}
			</td>
			<td>
				{auth.role == 1 ? (
					<>
						<Link
							href={`${router.asPath}?form=stepEdit&stepId=${id}`}
						>
							<div
								className={[
									styles.edit,
									editing ? styles.active : "",
								].join(" ")}
								onClick={() => !editing && setEditng(true)}
							>
								Edit
							</div>
						</Link>
						<div
							className={[
								styles.delete,
								deleting ? styles.active : "",
							].join(" ")}
							onClick={() => {
								if (deleting) {
									API.SCHEMAS.NODES.STEPS.REMOVE(id).then(
										(res) => {
											if (res) {
												dispatch(
													removeStep({
														stepId: id,
														nodeId,
													})
												);
											}
										}
									);
								} else setDeleting(true);
							}}
						>
							Delete
						</div>
					</>
				) : (
					""
				)}
			</td>
		</tr>
	);
};

export default NodeItem;

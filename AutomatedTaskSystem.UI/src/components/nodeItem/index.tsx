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
					<div className="flex gap-2 items-center">
						<div>{name}</div>
						<div className="flex gap-4">
							<div
								onClick={() => API.SCHEMAS.NODES.UP(id)
									.then(res => res && updateNodes())}
							>
								<svg
									width="20"
									height="22"
									viewBox="0 0 20 22"
									xmlns="http://www.w3.org/2000/svg"
									className="fill-slate-400 cursor-pointer hover:fill-cyan-600"
								>
									<path
										fillRule="evenodd"
										clipRule="evenodd"
										d="M0.883516 8.02006L7.88151 0.893616C9.05152 -0.297872 10.9485 -0.297872 12.1185 0.893616L19.1165 8.02006C21.0039 9.94208 19.6671 13.2284 16.998 13.2284H12.996V20.4745C12.996 21.317 12.3253 22 11.498 22H7.75301C6.92567 22 6.25501 21.317 6.25501 20.4745V13.2284H3.00201C0.332853 13.2284 -1.00386 9.94208 0.883516 8.02006Z"
									/>
								</svg>
							</div>
							<div
								className="rotate-180"
								onClick={() => API.SCHEMAS.NODES.DOWN(id)
									.then(res => res && updateNodes())}
							>
								<svg
									width="20"
									height="22"
									viewBox="0 0 20 22"
									fill="none"
									xmlns="http://www.w3.org/2000/svg"
									className="fill-slate-400 cursor-pointer hover:fill-cyan-600"
								>
									<path
										fillRule="evenodd"
										clipRule="evenodd"
										d="M0.883516 8.02006L7.88151 0.893616C9.05152 -0.297872 10.9485 -0.297872 12.1185 0.893616L19.1165 8.02006C21.0039 9.94208 19.6671 13.2284 16.998 13.2284H12.996V20.4745C12.996 21.317 12.3253 22 11.498 22H7.75301C6.92567 22 6.25501 21.317 6.25501 20.4745V13.2284H3.00201C0.332853 13.2284 -1.00386 9.94208 0.883516 8.02006Z"
									/>
								</svg>
							</div>
						</div>
					</div>
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
									updateNodes={updateNodes}
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
	updateNodes
}: {
	priority: number | null;
	nodeId: number;
	id: number;
	TL: boolean;
	reviewable: boolean;
	name: string;
	group: { id: number; name: string };
	duration: number;
	updateNodes: () => void;
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
						<div className="flex gap-2 pr-2">
							<div
								onClick={() => API.SCHEMAS.NODES.STEPS.UP(id).then(res => res && !res.error && updateNodes())}
							>
								<svg width="12" height="14" viewBox="0 0 12 14" fill="none" xmlns="http://www.w3.org/2000/svg"
									className="fill-slate-400 cursor-pointer hover:fill-cyan-600"
								>
									<path
										fillRule="evenodd"
										clipRule="evenodd"
										d="M0.530109 5.10367L4.72891 0.568665C5.43091 -0.189555 6.56909 -0.189555 7.27109 0.568665L11.4699 5.10367C12.6023 6.32678 11.8003 8.41808 10.1988 8.41808H7.7976V13.0292C7.7976 13.5654 7.3952 14 6.8988 14H4.6518C4.1554 14 3.75301 13.5654 3.75301 13.0292V8.41808H1.8012C0.199712 8.41808 -0.602316 6.32678 0.530109 5.10367Z"
									/>
								</svg>
							</div>
							<div
								className="rotate-180"
								onClick={() => API.SCHEMAS.NODES.STEPS.DOWN(id).then(res => res && !res.error && updateNodes())}
							>
								<svg width="12" height="14" viewBox="0 0 12 14" fill="none" xmlns="http://www.w3.org/2000/svg"
									className="fill-slate-400 cursor-pointer hover:fill-cyan-600"
								>
									<path
										fillRule="evenodd"
										clipRule="evenodd"
										d="M0.530109 5.10367L4.72891 0.568665C5.43091 -0.189555 6.56909 -0.189555 7.27109 0.568665L11.4699 5.10367C12.6023 6.32678 11.8003 8.41808 10.1988 8.41808H7.7976V13.0292C7.7976 13.5654 7.3952 14 6.8988 14H4.6518C4.1554 14 3.75301 13.5654 3.75301 13.0292V8.41808H1.8012C0.199712 8.41808 -0.602316 6.32678 0.530109 5.10367Z"
									/>
								</svg>
							</div>
						</div>
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

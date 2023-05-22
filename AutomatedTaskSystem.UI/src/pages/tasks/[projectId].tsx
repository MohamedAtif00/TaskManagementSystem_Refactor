import Link from "next/link";
import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import { useAppSelector } from "../../app/hooks";
import PlusIcon from "../../assets/Icons/Plus";
import QueryButton from "../../components/button/queryButton";
import CreateTask from "../../components/forms/projects/addTask";
import AssignTask from "../../components/forms/tasks/assignToTask";
import Header from "../../components/header/header";
import TaskDetails from "../../components/taskDetails";
import API from "../../lib/API";
import styles from "../../styles/tasks.module.scss";

const Task = ({
	id,
	name,
	lo,
	flagged = false,
	attention,
	userName,
	from,
	isRollback,
	rollbackCount,
}: {
	id: number;
	name: string;
	lo: string;
	flagged: boolean;
	attention: boolean;
	userName: string;
	from: string;
	isRollback: boolean;
	rollbackCount: number;
}) => {
	const router = useRouter();
	const projectId = router.query.projectId;

	return (
		<Link
			href={{
				pathname: `/tasks/${projectId}`,
				query: {
					taskId: id,
				},
			}}
		>
			<div
				className={[
					styles.task,
					flagged
						? styles.flagged
						: attention
						? "border border-solid border-emerald-600"
						: "",
				].join(" ")}
			>
				{isRollback ? (
					<div className="flex px-1 items-end justify-between bg-orange-800 text-white rounded-t-md">
						<div className="text-xl font-bold pt-2">Rollback</div>
						<div>{`#${rollbackCount}`}</div>
					</div>
				) : (
					""
				)}
				<div className={styles.info}>
					<div>{name}</div>
				</div>
				{from && (
					<div className="flex justify-end">
						<div>
							<div className="text-xs opacity-60 text-orange-600 flex justify-end">
								From
							</div>
							<div className="text-sm opacity-60">{from}</div>
						</div>
					</div>
				)}
				<div className={styles.details}>
					<div>{lo}</div>
				</div>
				<div className={styles.details}>
					<div>{userName}</div>
				</div>
			</div>
		</Link>
	);
};

const Tasks = () => {
	const [tasks, setTasks] = useState<TaskInfo[]>([]);
	const [filteredTasks, setFilteredTasks] = useState<TaskInfo[]>([]);
	const [project, setProject] = useState<IProject>();
	const [loFilter, setLoFilter] = useState(0);
	const auth = useAppSelector((e) => e.authSlice);
	const router = useRouter();

	useEffect(() => {
		if (loFilter > 0) {
			setFilteredTasks(
				tasks.filter((_) => _.learningObjectiveId == loFilter)
			);
			return;
		}
		setFilteredTasks(tasks);
	}, [loFilter, tasks, setFilteredTasks]);

	useEffect(() => {
		const id = router.query.projectId;
		if (id !== undefined)
			API.PROJECTS.GET_ONE(id).then(
				(res) => res && !res.error && setProject(res.data)
			);
	}, [router.query.projectId]);

	useEffect(() => {
		project &&
			API.TASKS.GET_ALL(project.id.toString()).then(
				(res) => res && setTasks(res)
			);
	}, [project]);

	useEffect(() => {
		if (project) {
			const refreshInterval = setInterval(() => {
				API.TASKS.GET_ALL(project.id.toString()).then(
					(res) => res && setTasks(res)
				);
			}, 60000);
			return () => clearInterval(refreshInterval);
		}
	}, [project]);

	const refreshTasks = () => {
		const projectId = router.query.projectId;
		if (projectId) {
			API.TASKS.GET_ALL(projectId).then((res) => {
				if (res) {
					setTasks(res);
				}
			});
		}
	};

	const los: { id: number; name: string }[] = [];
	tasks.forEach((t) => {
		if (!los.find((_) => _.id === t.learningObjectiveId))
			los.push({ id: t.learningObjectiveId, name: t.learningObjective });
	});

	if (!project) return <div>Loading</div>;

	return (
		<div className={["w-full", styles.container].join(" ")}>
			<Header text="Task" icon="Task">
				{auth.role === 1 ? (
					<QueryButton
						icon={<PlusIcon />}
						text="New Task"
						url={{
							pathname: `/tasks/${project.id}`,
							query: {
								form: "new-task",
							},
						}}
					/>
				) : (
					<></>
				)}
				<div>
					<select
						className="text-base font-normal"
						value={loFilter}
						onChange={(e) => {
							const value = e.target.value;
							const id = parseInt(value);

							if (!isNaN(id)) {
								setLoFilter(id);
							} else {
								setLoFilter(0);
							}
						}}
					>
						<option value={0}>None</option>
						{los.map((lo) => (
							<option key={lo.id} value={lo.id}>
								{lo.name}
							</option>
						))}
					</select>
				</div>
			</Header>
			<div className={styles.tasks}>
				<div className={styles.col}>
					<h3>Backlogs</h3>
					{filteredTasks
						.filter((t) => t.status == "Backlog")
						.map((t) => {
							return (
								<Task
									attention={t.attention}
									id={t.id}
									key={t.id}
									name={t.name}
									lo={t.learningObjective}
									flagged={t.flagged}
									userName={t.user}
									from={t.from}
									isRollback={t.isRollback}
									rollbackCount={t.rollbackCount}
								/>
							);
						})}
				</div>
				<div className={styles.col}>
					<h3>To Do</h3>
					{filteredTasks
						.filter((t) => t.status == "To Do")
						.map((t) => {
							return (
								<Task
									attention={t.attention}
									id={t.id}
									key={t.id}
									name={t.name}
									lo={t.learningObjective}
									flagged={t.flagged}
									userName={t.user}
									from={t.from}
									isRollback={t.isRollback}
									rollbackCount={t.rollbackCount}
								/>
							);
						})}
				</div>
				<div className={styles.col}>
					<h3>Doing</h3>
					{filteredTasks
						.filter((t) => t.status == "Doing")
						.map((t) => {
							return (
								<Task
									attention={t.attention}
									id={t.id}
									key={t.id}
									name={t.name}
									lo={t.learningObjective}
									flagged={t.flagged}
									userName={t.user}
									from={t.from}
									isRollback={t.isRollback}
									rollbackCount={t.rollbackCount}
								/>
							);
						})}
				</div>
				<div className={styles.col}>
					<h3>Done</h3>
					{filteredTasks
						.filter(
							(t) => t.status == "Done" || t.status == "Rollback"
						)
						.map((t) => {
							return (
								<Task
									id={t.id}
									attention={t.attention}
									key={t.id}
									name={t.name}
									lo={t.learningObjective}
									flagged={t.flagged}
									userName={t.user}
									from={t.from}
									isRollback={t.isRollback}
									rollbackCount={t.rollbackCount}
								/>
							);
						})}
				</div>
			</div>
			<TaskDetails refreshTasks={refreshTasks} projectId={project.id} />
			{router.query.form === "task-assign" && (
				<AssignTask
					taskId={router.query.taskId!}
					refreshTask={refreshTasks}
				/>
			)}
			{router.query.form === "new-task" && (
				<CreateTask
					refresh={() => {
						refreshTasks();
						router.back();
					}}
					projectId={project.id}
				/>
			)}
		</div>
	);
};

export default Tasks;

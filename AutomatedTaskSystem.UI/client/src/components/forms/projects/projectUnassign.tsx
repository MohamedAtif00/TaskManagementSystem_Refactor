import { useRouter } from "next/router";
import Backdrop from "../backdrop";
import React, { useEffect, useState } from "react";
import API from "../../../lib/API";

interface Props {
	handler: (userIds: number[]) => void;
}

interface loUser {
	id: number;
	name: string;
	group: { id: number; name: string };
	role: UserRole;
}

interface GroupAccordion {
	id: number;
	name: string;
	isOpen: boolean;
}

// ── UserCard ────────────────────────────────────────────────────────────────
const UserCard = ({
	user,
	selected,
	onClick,
}: {
	user: loUser;
	selected: boolean;
	onClick: () => void;
}) => (
	<button
		type="button"
		onClick={onClick}
		style={{
			display: "flex",
			flexDirection: "column",
			alignItems: "flex-start",
			padding: "12px 14px",
			borderRadius: 10,
			border: selected ? "1.5px solid #E2E8F0" : "1.5px solid #FCA5A5",
			background: selected ? "#fff" : "#FFF5F5",
			cursor: "pointer",
			textAlign: "left",
			transition: "border-color 0.15s, box-shadow 0.15s, background 0.15s",
			boxShadow: !selected ? "0 0 0 3px rgba(239,68,68,0.08)" : "none",
			minWidth: 0,
			position: "relative",
		}}
	>
		{/* "Will be removed" indicator */}
		{!selected && (
			<span
				style={{
					position: "absolute",
					top: 6,
					right: 8,
					fontSize: 10,
					fontFamily: "'DM Sans', sans-serif",
					fontWeight: 600,
					color: "#EF4444",
					letterSpacing: 0.3,
				}}
			>
				Remove
			</span>
		)}
		<span
			style={{
				fontFamily: "'DM Sans', sans-serif",
				fontWeight: 600,
				fontSize: 14,
				color: selected ? "#1E293B" : "#EF4444",
				whiteSpace: "nowrap",
				overflow: "hidden",
				textOverflow: "ellipsis",
				maxWidth: "100%",
				textDecoration: selected ? "none" : "line-through",
			}}
		>
			{user.name}
		</span>
		<span
			style={{
				fontFamily: "'DM Sans', sans-serif",
				fontSize: 12,
				color: selected ? "#94A3B8" : "#FCA5A5",
				marginTop: 2,
			}}
		>
			{user.role}
		</span>
	</button>
);

// ── GroupRow ─────────────────────────────────────────────────────────────────
const GroupRow = ({
	group,
	users,
	selectedIds,
	onToggleGroup,
	onToggleUser,
	onToggleOpen,
}: {
	group: GroupAccordion;
	users: loUser[];
	selectedIds: number[];
	onToggleGroup: (groupId: number) => void;
	onToggleUser: (userId: number) => void;
	onToggleOpen: (groupId: number) => void;
}) => {
	const groupUserIds = users.map((u) => u.id);
	const removedCount = groupUserIds.filter((id) => !selectedIds.includes(id)).length;
	const allDeselected = groupUserIds.length > 0 && removedCount === groupUserIds.length;
	const someDeselected = removedCount > 0 && !allDeselected;

	return (
		<div
			style={{
				borderRadius: 14,
				border: "1.5px solid #E2E8F0",
				overflow: "hidden",
				background: "#fff",
				marginBottom: 10,
			}}
		>
			{/* Header */}
			<div
				style={{
					display: "flex",
					alignItems: "center",
					padding: "14px 16px",
					gap: 12,
					background: "#fff",
				}}
			>
				{/* Checkbox */}
				<button
					type="button"
					onClick={() => onToggleGroup(group.id)}
					style={{
						width: 20,
						height: 20,
						borderRadius: 5,
						border: allDeselected || someDeselected ? "none" : "1.5px solid #CBD5E1",
						background: allDeselected ? "#EF4444" : someDeselected ? "#FCA5A5" : "#fff",
						cursor: "pointer",
						display: "flex",
						alignItems: "center",
						justifyContent: "center",
						flexShrink: 0,
						transition: "background 0.15s",
						padding: 0,
					}}
					aria-label={allDeselected ? "Restore all" : "Remove all"}
				>
					{allDeselected && (
						<svg width="12" height="10" viewBox="0 0 12 10" fill="none">
							<path d="M1 5L4.5 8.5L11 1.5" stroke="#fff" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
						</svg>
					)}
					{someDeselected && !allDeselected && (
						<svg width="10" height="2" viewBox="0 0 10 2" fill="none">
							<rect y="0" width="10" height="2" rx="1" fill="#fff" />
						</svg>
					)}
				</button>

				{/* Group name */}
				<span style={{ fontFamily: "'DM Sans', sans-serif", fontWeight: 700, fontSize: 16, color: "#0F172A", flex: 1 }}>
					{group.name}
				</span>

				{/* Removed badge */}
				{removedCount > 0 ? (
					<span style={{ background: "#EF4444", color: "#fff", borderRadius: 99, padding: "2px 10px", fontSize: 12, fontFamily: "'DM Sans', sans-serif", fontWeight: 600 }}>
						Remove ({removedCount})
					</span>
				) : (
					<span style={{ color: "#94A3B8", fontSize: 13, fontFamily: "'DM Sans', sans-serif" }}>
						Remove (0)
					</span>
				)}

				{/* Expand/collapse */}
				<button
					type="button"
					onClick={() => onToggleOpen(group.id)}
					style={{ width: 28, height: 28, borderRadius: 6, border: "1.5px solid #E2E8F0", background: "#F8FAFC", cursor: "pointer", display: "flex", alignItems: "center", justifyContent: "center", flexShrink: 0, padding: 0, marginLeft: 4 }}
					aria-label={group.isOpen ? "Collapse" : "Expand"}
				>
					{group.isOpen ? (
						<svg width="12" height="3" viewBox="0 0 12 3" fill="none"><rect y="0.5" width="12" height="2" rx="1" fill="#64748B" /></svg>
					) : (
						<svg width="12" height="12" viewBox="0 0 12 12" fill="none"><rect y="5" width="12" height="2" rx="1" fill="#64748B" /><rect x="5" width="2" height="12" rx="1" fill="#64748B" /></svg>
					)}
				</button>
			</div>

			{/* User grid */}
			{group.isOpen && (
				<div style={{ display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: 10, padding: "0 16px 16px" }}>
					{users.map((user) => (
						<UserCard key={user.id} user={user} selected={selectedIds.includes(user.id)} onClick={() => onToggleUser(user.id)} />
					))}
				</div>
			)}
		</div>
	);
};

// ── ProjectUnassign ───────────────────────────────────────────────────────────
const ProjectUnassign = ({ handler }: Props) => {
	const router = useRouter();
	const [active, setActive] = useState(false);
	const [selectedIds, setSelectedIds] = useState<number[]>([]);
	const [users, setUsers] = useState<loUser[]>([]);
	const [groups, setGroups] = useState<GroupAccordion[]>([]);

	useEffect(() => {
		const _active = router.query.form === "unassign" && router.query.projectId !== undefined;
		setActive(_active);
		if (!_active) setSelectedIds([]);
	}, [router]);

	useEffect(() => {
		const id = router.query.projectId;
		if (!id) return;

		API.PROJECTS.USERS_ASSIGNED(id).then((res) => {
			if (res && !res.error) {
				const formattedUsers: loUser[] = res.data.map((user: IUser): loUser => ({
					...user,
					group: user.group ?? { id: 0, name: "No Group" },
				}));
				setUsers(formattedUsers);

				// Pre-select ALL — admin deselects the ones they want to remove
				setSelectedIds(formattedUsers.map((u) => u.id));

				const uniqueGroupsMap = new Map<number, { id: number; name: string }>();
				formattedUsers.forEach((u) => {
					if (!uniqueGroupsMap.has(u.group.id)) {
						uniqueGroupsMap.set(u.group.id, { id: u.group.id, name: u.group.name });
					}
				});

				setGroups(Array.from(uniqueGroupsMap.values()).map((g) => ({ ...g, isOpen: false })));
			}
		});
	}, [router]);

	const toggleUser = (userId: number) => {
		setSelectedIds((prev) =>
			prev.includes(userId) ? prev.filter((id) => id !== userId) : [...prev, userId]
		);
	};

	const toggleGroup = (groupId: number) => {
		const groupUserIds = users.filter((u) => u.group.id === groupId).map((u) => u.id);
		const allDeselected = groupUserIds.every((id) => !selectedIds.includes(id));
		if (allDeselected) {
			// Restore all in group
			setSelectedIds((prev) => [...new Set([...prev, ...groupUserIds])]);
		} else {
			// Remove all in group
			setSelectedIds((prev) => prev.filter((id) => !groupUserIds.includes(id)));
		}
	};

	const toggleOpen = (groupId: number) => {
		setGroups((prev) => prev.map((g) => (g.id === groupId ? { ...g, isOpen: !g.isOpen } : g)));
	};

	const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
		e.preventDefault();
		const removedIds = users.map((u) => u.id).filter((id) => !selectedIds.includes(id));
		handler(removedIds);
	};

	const totalRemoved = users.filter((u) => !selectedIds.includes(u.id)).length;

	if (!active) return <></>;

	return (
		<Backdrop mainRoute={`/projects/${router.query.projectId}`}>
			<div
				style={{
					background: "#F1F5F9",
					borderRadius: 18,
					padding: 24,
					width: 560,
					maxHeight: "80vh",
					overflowY: "auto",
					boxShadow: "0 8px 40px rgba(15,23,42,0.10)",
					fontFamily: "'DM Sans', sans-serif",
				}}
			>
				<style>{`@import url('https://fonts.googleapis.com/css2?family=DM+Sans:wght@400;500;600;700&display=swap');`}</style>

				<h2 style={{ fontWeight: 700, fontSize: 20, color: "#0F172A", marginBottom: 6, marginTop: 0 }}>
					Unassign Users from Project
				</h2>

				<p style={{ fontFamily: "'DM Sans', sans-serif", fontSize: 13, color: "#64748B", marginBottom: 16, marginTop: 0 }}>
					Click a user to mark them for removal. Selected users stay on the project.
				</p>

				<form onSubmit={handleSubmit}>
					{groups.map((group) => (
						<GroupRow
							key={group.id}
							group={group}
							users={users.filter((u) => u.group.id === group.id)}
							selectedIds={selectedIds}
							onToggleGroup={toggleGroup}
							onToggleUser={toggleUser}
							onToggleOpen={toggleOpen}
						/>
					))}

					<button
						type="submit"
						disabled={totalRemoved === 0}
						style={{
							marginTop: 8,
							width: "100%",
							padding: "12px 0",
							background: totalRemoved === 0 ? "#CBD5E1" : "#EF4444",
							color: "#fff",
							border: "none",
							borderRadius: 10,
							fontFamily: "'DM Sans', sans-serif",
							fontWeight: 700,
							fontSize: 15,
							cursor: totalRemoved === 0 ? "not-allowed" : "pointer",
							letterSpacing: 0.3,
							transition: "background 0.15s",
						}}
						onMouseEnter={(e) => { if (totalRemoved > 0) e.currentTarget.style.background = "#DC2626"; }}
						onMouseLeave={(e) => { if (totalRemoved > 0) e.currentTarget.style.background = "#EF4444"; }}
					>
						{totalRemoved === 0
							? "No users selected for removal"
							: `Remove ${totalRemoved} user${totalRemoved > 1 ? "s" : ""}`}
					</button>
				</form>
			</div>
		</Backdrop>
	);
};

export default ProjectUnassign;
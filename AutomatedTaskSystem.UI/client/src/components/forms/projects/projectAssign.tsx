import React, { useEffect, useState } from "react";
import { useRouter } from "next/router";
import Backdrop from "../backdrop";
import API from "../../../lib/API";

interface Props {
  handler: (userIds: number[]) => void;
}

interface loUser {
  id: number;
  name: string;
  group: { id: number; name: string };
  role: string;
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
      border: selected ? "1.5px solid #3B82F6" : "1.5px solid #E2E8F0",
      background: "#fff",
      cursor: "pointer",
      textAlign: "left",
      transition: "border-color 0.15s, box-shadow 0.15s",
      boxShadow: selected ? "0 0 0 3px rgba(59,130,246,0.10)" : "none",
      minWidth: 0,
    }}
  >
    <span
      style={{
        fontFamily: "'DM Sans', sans-serif",
        fontWeight: 600,
        fontSize: 14,
        color: selected ? "#2563EB" : "#1E293B",
        whiteSpace: "nowrap",
        overflow: "hidden",
        textOverflow: "ellipsis",
        maxWidth: "100%",
      }}
    >
      {user.name}
    </span>
    <span
      style={{
        fontFamily: "'DM Sans', sans-serif",
        fontSize: 12,
        color: selected ? "#60A5FA" : "#94A3B8",
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
  const selectedCount = groupUserIds.filter((id) => selectedIds.includes(id)).length;
  const allSelected = groupUserIds.length > 0 && selectedCount === groupUserIds.length;
  const someSelected = selectedCount > 0 && !allSelected;

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
            border: allSelected
              ? "none"
              : someSelected
              ? "none"
              : "1.5px solid #CBD5E1",
            background: allSelected
              ? "#3B82F6"
              : someSelected
              ? "#3B82F6"
              : "#fff",
            cursor: "pointer",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            flexShrink: 0,
            transition: "background 0.15s",
            padding: 0,
          }}
          aria-label={allSelected ? "Deselect all" : "Select all"}
        >
          {allSelected && (
            <svg width="12" height="10" viewBox="0 0 12 10" fill="none">
              <path d="M1 5L4.5 8.5L11 1.5" stroke="#fff" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
            </svg>
          )}
          {someSelected && !allSelected && (
            <svg width="10" height="2" viewBox="0 0 10 2" fill="none">
              <rect y="0" width="10" height="2" rx="1" fill="#fff" />
            </svg>
          )}
        </button>

        {/* Group name */}
        <span
          style={{
            fontFamily: "'DM Sans', sans-serif",
            fontWeight: 700,
            fontSize: 16,
            color: "#0F172A",
            flex: 1,
          }}
        >
          {group.name}
        </span>

        {/* Selected badge */}
        {selectedCount > 0 && (
          <span
            style={{
              background: "#3B82F6",
              color: "#fff",
              borderRadius: 99,
              padding: "2px 10px",
              fontSize: 12,
              fontFamily: "'DM Sans', sans-serif",
              fontWeight: 600,
            }}
          >
            Selected ({selectedCount})
          </span>
        )}
        {selectedCount === 0 && (
          <span
            style={{
              color: "#94A3B8",
              fontSize: 13,
              fontFamily: "'DM Sans', sans-serif",
            }}
          >
            Selected (0)
          </span>
        )}

        {/* Expand/collapse */}
        <button
          type="button"
          onClick={() => onToggleOpen(group.id)}
          style={{
            width: 28,
            height: 28,
            borderRadius: 6,
            border: "1.5px solid #E2E8F0",
            background: "#F8FAFC",
            cursor: "pointer",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            flexShrink: 0,
            padding: 0,
            marginLeft: 4,
          }}
          aria-label={group.isOpen ? "Collapse" : "Expand"}
        >
          {group.isOpen ? (
            <svg width="12" height="3" viewBox="0 0 12 3" fill="none">
              <rect y="0.5" width="12" height="2" rx="1" fill="#64748B" />
            </svg>
          ) : (
            <svg width="12" height="12" viewBox="0 0 12 12" fill="none">
              <rect y="5" width="12" height="2" rx="1" fill="#64748B" />
              <rect x="5" width="2" height="12" rx="1" fill="#64748B" />
            </svg>
          )}
        </button>
      </div>

      {/* User grid (accordion body) */}
      {group.isOpen && (
        <div
          style={{
            display: "grid",
            gridTemplateColumns: "repeat(3, 1fr)",
            gap: 10,
            padding: "0 16px 16px",
          }}
        >
          {users.map((user) => (
            <UserCard
              key={user.id}
              user={user}
              selected={selectedIds.includes(user.id)}
              onClick={() => onToggleUser(user.id)}
            />
          ))}
        </div>
      )}
    </div>
  );
};

// ── ProjectAssign ────────────────────────────────────────────────────────────
const ProjectAssign = ({ handler }: Props) => {
  const router = useRouter();
  const [active, setActive] = useState(false);
  const [selectedIds, setSelectedIds] = useState<number[]>([]);
  const [users, setUsers] = useState<loUser[]>([]);
  const [groups, setGroups] = useState<GroupAccordion[]>([]);

  useEffect(() => {
    const _active =
      router.query.form === "assign" && router.query.projectId !== undefined;
    setActive(_active);
    if (!_active) setSelectedIds([]);
  }, [router]);

  useEffect(() => {
    const id = router.query.projectId;
    if (!id) return;

    API.PROJECTS.USERS_UNASSIGNED(id).then((res) => {
      if (res && !res.error) {
        const formattedUsers: loUser[] = res.data.map((user: any) => ({
          ...user,
          group: user.group ?? { id: 0, name: "No Group" },
        }));
        setUsers(formattedUsers);

        const uniqueGroupsMap = new Map<number, { id: number; name: string }>();
        formattedUsers.forEach((u) => {
          if (!uniqueGroupsMap.has(u.group.id)) {
            uniqueGroupsMap.set(u.group.id, { id: u.group.id, name: u.group.name });
          }
        });

        setGroups(
          Array.from(uniqueGroupsMap.values()).map((g) => ({
            ...g,
            isOpen: false,
          }))
        );
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
    const allSelected = groupUserIds.every((id) => selectedIds.includes(id));
    if (allSelected) {
      setSelectedIds((prev) => prev.filter((id) => !groupUserIds.includes(id)));
    } else {
      setSelectedIds((prev) => [...new Set([...prev, ...groupUserIds])]);
    }
  };

  const toggleOpen = (groupId: number) => {
    setGroups((prev) =>
      prev.map((g) => (g.id === groupId ? { ...g, isOpen: !g.isOpen } : g))
    );
  };

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    handler(selectedIds);
  };

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
        <h2
          style={{
            fontWeight: 700,
            fontSize: 20,
            color: "#0F172A",
            marginBottom: 18,
            marginTop: 0,
          }}
        >
          Assign Users to Project
        </h2>
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
            style={{
              marginTop: 8,
              width: "100%",
              padding: "12px 0",
              background: "#2563EB",
              color: "#fff",
              border: "none",
              borderRadius: 10,
              fontFamily: "'DM Sans', sans-serif",
              fontWeight: 700,
              fontSize: 15,
              cursor: "pointer",
              letterSpacing: 0.3,
              transition: "background 0.15s",
            }}
            onMouseEnter={(e) => (e.currentTarget.style.background = "#1D4ED8")}
            onMouseLeave={(e) => (e.currentTarget.style.background = "#2563EB")}
          >
            Save
          </button>
        </form>
      </div>
    </Backdrop>
  );
};

export default ProjectAssign;
import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import Header from "../../../components/header/header";
import styles from "../../../styles/resources.module.scss";
import API from "../../../lib/API";
import Head from "next/head";
import Loader from "../../../components/loader";
import EditSection from "../../../components/sectionComponent/editSection";
import RemoveSection from "../../../components/sectionComponent/removeSection";

const Section = () => {
	const router = useRouter();
	const [section, setSection] = useState<ISection>();
	const [isEditOpen, setIsEditOpen] = useState(false);
	const [isDeleteOpen, setIsDeleteOpen] = useState(false);
	const [allUsers, setAllUsers] = useState<{ id: number; name: string }[]>([]);
	const [allGroups, setAllGroups] = useState<{ id: number; name: string }[]>([]);

	useEffect(() => {
		const id = router.query.sectionId;
		if (id !== undefined) {
			API.RESOURCES.SECTIONS.GET_ONE(id).then((res) => {
				if (res && !res.error) setSection(res.data);
			});
			API.RESOURCES.USERS.GET_ALL().then((res) => {
				if (res && !res.error) setAllUsers(res.data);
			});
			API.RESOURCES.GROUPS.GET_ALL().then((res) => {
				if (res && !res.error) setAllGroups(res.data);
			});
		}
	}, [router.query.sectionId]);

	const updateSection = (data: {
		id: number;
		name: string;
		headId: number;
		groups: number[];
	}) => {
		return API.RESOURCES.SECTIONS.EDIT({
			id: data.id,
			name: data.name,
			headId: data.headId,
			groups: data.groups,
		}).then((res) => {
			if (res && !res.error && res.data) {
				setSection(res.data);
				return true;
			}
			return false;
		});
	};

	const handleEditSubmit = (data: {
		id: number;
		name: string;
		headId: number;
		groups: number[];
	}) => {
		updateSection(data).then((success) => {
			if (success) setIsEditOpen(false);
		});
	};

	const handleRemoveGroup = (groupId: number) => {
		if (!section) return;
		const remainingGroups = section.groups
			.filter((g) => g.id !== groupId)
			.map((g) => g.id);
		if (remainingGroups.length === 0) return;

		updateSection({
			id: section.id,
			name: section.name,
			headId: section.head.id,
			groups: remainingGroups,
		});
	};

	const handleAddGroup = (groupId: number) => {
		if (!section) return;
		if (section.groups.some((g) => g.id === groupId)) return;

		updateSection({
			id: section.id,
			name: section.name,
			headId: section.head.id,
			groups: [...section.groups.map((g) => g.id), groupId],
		});
	};

	const availableGroups = allGroups.filter(
		(g) => !section?.groups.some((sg) => sg.id === g.id)
	);

	if (section === undefined)
		return (
			<div className="flex items-center justify-center mx-auto h-full">
				<Head>
					<title>ATS - Loading</title>
				</Head>
				<Loader />
			</div>
		);

	return (
		<>
			<Head>
				<title>ATS - {section.name} Section</title>
			</Head>
			<div className="mainContainer">
				<Header text={section.name} icon="Resources" />

				<div className={styles.sectionDetail}>
					<h2>Head: {section.head.name}</h2>

					<div className="mt-4">
						<h3 className="font-semibold mb-2">Groups</h3>
						<div className="flex flex-col gap-2">
							{section.groups.map((g) => (
								<div
									key={g.id}
									className="flex items-center justify-between bg-gray-50 px-3 py-2 rounded border border-gray-200 max-w-md"
								>
									<span>{g.name}</span>
									<button
										onClick={() => handleRemoveGroup(g.id)}
										disabled={section.groups.length <= 1}
										className="text-red-600 text-sm hover:text-red-800 disabled:opacity-40 disabled:cursor-not-allowed"
										title={
											section.groups.length <= 1
												? "Section must have at least one group"
												: "Remove group from section"
										}
									>
										Remove
									</button>
								</div>
							))}
						</div>

						{availableGroups.length > 0 && (
							<div className="mt-3 flex items-center gap-2 max-w-md">
								<select
									className="border border-gray-300 rounded px-3 py-2 flex-1"
									defaultValue=""
									onChange={(e) => {
										const groupId = Number(e.target.value);
										if (groupId) {
											handleAddGroup(groupId);
											e.target.value = "";
										}
									}}
								>
									<option value="" disabled>
										Add a group...
									</option>
									{availableGroups.map((g) => (
										<option key={g.id} value={g.id}>
											{g.name}
										</option>
									))}
								</select>
							</div>
						)}
					</div>

					<div className={styles.sectionActions}>
						<button
							type="button"
							onClick={() => setIsEditOpen(true)}
							className={styles.editButton}
						>
							Edit Section
						</button>
						<button
							type="button"
							onClick={() => setIsDeleteOpen(true)}
							className={styles.deleteButton}
						>
							Delete Section
						</button>
					</div>
				</div>
			</div>

			{isEditOpen && (
				<EditSection
					section={section}
					users={allUsers}
					groups={allGroups}
					handleSubmit={handleEditSubmit}
					onClose={() => setIsEditOpen(false)}
				/>
			)}

			{isDeleteOpen && (
				<RemoveSection
					section={section}
					onClose={() => setIsDeleteOpen(false)}
				/>
			)}
		</>
	);
};

export default Section;

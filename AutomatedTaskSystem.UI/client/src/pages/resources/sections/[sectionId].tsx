import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import Header from "../../../components/header/header";
import styles from "../../../styles/resources.module.scss";
import API from "../../../lib/API";
import Head from "next/head";
import Loader from "../../../components/loader";
import EditSection from "../../../components/sectionComponent/editSection";


const Section = () => {
	const router = useRouter();
	const [section, setSection] = useState<ISection>();
	const [isModalOpen, setIsModalOpen] = useState(false);
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

	const handleEditSubmit = (data: {
		id: number;
		name: string;
		headId: number;
		groups: number[];
	}) => {
		API.RESOURCES.SECTIONS.EDIT({id:data.id, name:data.name,headId:data.headId,groups:data.groups}).then((res) => {
			if (!res.error) {
				setSection((prev) =>
					prev ? { ...prev, name: data.name, head: allUsers.find(u => u.id === data.headId)!, groups: allGroups.filter(g => data.groups.includes(g.id)) } : prev
				);
				setIsModalOpen(false);
			}
		});
	};

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

				<div className={styles.container}>
					<h2>Head: {section.head.name}</h2>
					<div>
						Groups:
						<div>
							{section.groups.map((g) => (
								<div key={g.id}>{g.name}</div>
							))}
						</div>
					</div>

					{/* <div className="mt-4">
						<button
							onClick={() => setIsModalOpen(true)}
							className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
						>
							Edit Section
						</button>
					</div> */}
				</div>
			</div>

			{isModalOpen && (
				<EditSection
					section={section}
					users={allUsers}
					groups={allGroups}
					handleSubmit={handleEditSubmit}
					onClose={() => setIsModalOpen(false)}
				/>
			)}
		</>
	);
};

export default Section;

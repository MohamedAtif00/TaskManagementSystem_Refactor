import React, { useEffect, useState } from "react";
import { motion } from "framer-motion";
import styles from "../styles.module.scss";

import FormField from "../forms/field";
import Dropdown from "../forms/dropdown";
import SelectList from "../forms/selectList";
import { useRouter } from "next/router";
import { useAppSelector } from "../../app/hooks";

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

const EditSection = ({ section, users, groups, handleSubmit, onClose }: Props) => {
     const { query, pathname, push } = useRouter();
     const { role } = useAppSelector((s) => s.authSlice);
	const [name, setName] = useState(section.name);
	const [head, setHead] = useState(section.head.id);
	const [selectedGroups, setSelectedGroups] = useState<number[]>(
		section.groups.map((g) => g.id)
	);
	const [submittable, setSubmittable] = useState(false);
    const [active, setActive] = useState(false);
	useEffect(() => {
		setSubmittable(name.trim() !== "" && head > 0 && selectedGroups.length > 0);
	}, [name, head, selectedGroups]);

    //  useEffect(() => {
    //         if (query.form === "create-sprint") return setActive(true);
    //         setActive(false);
    //     }, [query]);

    
    // if (!active || (role !== 2 && role !== 0)) return null;
	return (
		<div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 overflow-scroll">
			<motion.div
				initial={{ opacity: 0, y: -50 }}
				animate={{ opacity: 1, y: 0 }}
				exit={{ opacity: 0, y: -50 }}
				transition={{ duration: 0.3 }}
				className="bg-white rounded-lg shadow-lg p-6 w-full max-w-xl "
			>
				<h2 className="text-xl font-bold mb-4">Edit Section</h2>
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
					{/* <div className={styles.inputs}>
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
							updateList={(id: number) =>
								setSelectedGroups((prev) =>
									prev.includes(id)
										? prev.filter((g) => g !== id)
										: [...prev, id]
								)
							}
						/>
					</div> */}
                    <div className={styles.inputs}>
                        <FormField
                            label="Name"
                            onChange={setName}
                            value={name}
                        />
                        <Dropdown
                            handleChange={setHead}
                            id={head}
                            label="Section Head"
                            options={users}
                        />
                        {/* <SelectList
                            label="Groups"
                            list={groups}
                            selected={groups}
                            updateList={(n: number) =>
                                setGroups((ps) => {
                                    const newState: number[] = [];
                                    const foundIndex = ps.findIndex(
                                        (g) => g == n
                                    );
                                    if (foundIndex >= 0) {
                                        ps.forEach((v, i) => {
                                            if (i !== foundIndex) {
                                                newState.push(v);
                                            }
                                        });
                                        return newState;
                                    }
                                    ps.forEach((g) => {
                                        newState.push(g);
                                    });
                                    newState.push(n);
                                    return newState;
                                })
                            }
                        /> */}
                    </div>
					<div className="flex justify-between items-center mt-6">
						<button
							type="button"
							onClick={onClose}
							className="bg-gray-300 text-gray-800 px-4 py-2 rounded hover:bg-gray-400"
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
			</motion.div>
		</div>
	);
};

export default EditSection;

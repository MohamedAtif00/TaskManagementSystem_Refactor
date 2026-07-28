import { useRouter } from "next/router";
import InputTextField from "../../formComponents/InputTextField";
import { useEffect, useState } from "react";
import FormConclusion from "../../formComponents/FormConclusion";
import API from "../../../lib/API";
import { dismissFormModal } from "../../../lib/routerHelpers";
import { motion } from "framer-motion";
import { useAppDispatch } from "../../../app/hooks";
import { edit } from "../../../slices/projectSlice";
import Dropdown from "../../formComponents/DropDown";
import {
    mapSubjectGroupNodesToOptions,
    subjectLocationLabel,
    type SubjectGroupPickerOption,
} from "../../../lib/curriculumHierarchy";

const SUBJECT_GROUP_HINT = "Year › Project › Term › Subject group";

const subjectIdFromQuery = (query: ReturnType<typeof useRouter>["query"]) => {
    const s = query.subjectId ?? query.projectId;
    if (Array.isArray(s)) return s[0];
    return s;
};

const EditProject = () => {
    const dispatch = useAppDispatch();
    const router = useRouter();
    const { query, pathname } = router;
    const [active, setActive] = useState<boolean>(false);
    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [folder, setFolder] = useState<SubjectGroupPickerOption | null>(null);
    const [folders, setFolders] = useState<SubjectGroupPickerOption[]>([]);
    const [error, setError] = useState("");

    useEffect(() => {
        const sid = subjectIdFromQuery(query);
        if (query.form === "edit-project" && sid) {
            let cancelled = false;
            (async () => {
                const one = await API.PROJECTS.GET_ONE(sid);
                if (!one || one.error || cancelled) return;
                const s = one.data;
                setName(s.name);
                setDescription(s.description);
                const foldersRes = await API.PROJECTS.ROOT.WITH_SUBJECTS();
                if (!cancelled && foldersRes && typeof foldersRes === "object" && "error" in foldersRes && !foldersRes.error && "data" in foldersRes) {
                    setFolders(mapSubjectGroupNodesToOptions((foldersRes as { data: unknown[] }).data));
                }
                if (!cancelled) {
                    const folderId = Number((s as any).folderId);
                    if (!Number.isNaN(folderId)) {
                        const location =
                            subjectLocationLabel((s as any).folderPath) || `Subject group #${folderId}`;
                        setFolder({ id: folderId, name: location });
                    }
                }
            })();
            return setActive(true);
        }
        setActive(false);
    }, [query]);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setError("");

        const sid = subjectIdFromQuery(query);
        if (!sid) return setError("Missing subject");

        if (name === "") return setError("Please enter name");
        if (folder === null) return setError("Please select a subject group location");

        API.PROJECTS.EDIT({
            id: sid,
            name,
            description,
            folderId: folder.id,
        }).then((res) => {
            if (res && !res.error) {
                dispatch(edit(res.data));
                dismissFormModal(router);
            }
        });
    };

    if (active)
        return (
            <motion.div
                initial={{ backgroundColor: "#00000000" }}
                animate={{ backgroundColor: "#00000055", height: "auto" }}
                className="z-50 flex items-center justify-center fixed top-0 left-0 right-0 min-h-screen"
            >
                <motion.div
                    initial={{ opacity: 0.1 }}
                    animate={{ opacity: 1 }}
                    className="bg-white px-5 py-4 basis-80 rounded-lg max-w-lg w-full"
                >
                    <h2 className="text-lg mb-5">Edit subject</h2>
                    <form
                        onSubmit={handleSubmit}
                        className="flex flex-col gap-8"
                    >
                        <div className="flex flex-col gap-2">
                            <div className="text-red-600">{error}</div>
                            <InputTextField
                                label="Name"
                                value={name}
                                handleChange={setName}
                            />
                            <InputTextField
                                label="Description"
                                value={description}
                                handleChange={setDescription}
                            />
                            <div className="flex flex-col gap-3">
                                <Dropdown
                                    value={folder}
                                    handleChange={setFolder}
                                    label="Subject group location"
                                    hint={SUBJECT_GROUP_HINT}
                                    options={folders}
                                />
                            </div>
                        </div>
                        <FormConclusion
                            pathname={pathname}
                            submittable={true}
                        />
                    </form>
                </motion.div>
            </motion.div>
        );

    return <></>;
};

export default EditProject;

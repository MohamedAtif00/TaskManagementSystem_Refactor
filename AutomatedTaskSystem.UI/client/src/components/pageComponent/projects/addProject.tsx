import { useRouter } from "next/router";
import InputTextField from "../../formComponents/InputTextField";
import { useEffect, useState } from "react";
import FormConclusion from "../../formComponents/FormConclusion";
import API from "../../../lib/API";
import { dismissFormModal } from "../../../lib/routerHelpers";
import { motion } from "framer-motion";
import { useAppDispatch } from "../../../app/hooks";
import { add } from "../../../slices/projectSlice";
import Dropdown from "../../formComponents/DropDown";
import {
    mapSubjectGroupNodesToOptions,
    type SubjectGroupPickerOption,
} from "../../../lib/curriculumHierarchy";

const SUBJECT_GROUP_HINT = "Year › Project › Term › Subject group";

const presetFolderId = (query: ReturnType<typeof useRouter>["query"]) => {
    const raw = query.folderId ?? query.termId;
    if (raw === undefined) return null;
    const n = Number(Array.isArray(raw) ? raw[0] : raw);
    return Number.isNaN(n) ? null : n;
};

const AddProject = () => {
    const dispatch = useAppDispatch();
    const router = useRouter();
    const { query, pathname } = router;
    const lockedFolderId = presetFolderId(query);
    const [active, setActive] = useState<boolean>(false);
    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [folder, setFolder] = useState<SubjectGroupPickerOption | null>(null);
    const [folders, setFolders] = useState<SubjectGroupPickerOption[]>([]);
    const [error, setError] = useState("");

    useEffect(() => {
        if (query.form === "add-project") {
            if (lockedFolderId === null) {
                API.PROJECTS.ROOT.WITH_SUBJECTS().then((res) => {
                    if (res && typeof res === "object" && "error" in res && !res.error && "data" in res) {
                        setFolders(mapSubjectGroupNodesToOptions((res as { data: unknown[] }).data));
                    }
                });
            }
            setName("");
            setDescription("");
            setFolder(null);
            return setActive(true);
        }
        setActive(false);
    }, [query, lockedFolderId]);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setError("");

        if (name === "") return setError("Please enter name");

        const folderId =
            lockedFolderId !== null ? lockedFolderId : folder === null ? null : folder.id;

        if (folderId === null) {
            return setError("Please select a subject group location");
        }

        API.PROJECTS.CREATE({
            name,
            description,
            folderId,
        }).then((res) => {
            if (res && !res.error) {
                dispatch(add(res.data));
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
                    <h2 className="text-lg mb-5">Add new subject</h2>
                    <form onSubmit={handleSubmit} className="flex flex-col gap-8">
                        <div className="flex flex-col gap-2">
                            <div className="text-red-600">{error}</div>
                            <InputTextField label="Name" value={name} handleChange={setName} />
                            <InputTextField
                                label="Description"
                                value={description}
                                handleChange={setDescription}
                            />
                            {lockedFolderId === null && (
                                <div className="flex flex-col gap-3">
                                    <Dropdown
                                        value={folder}
                                        handleChange={setFolder}
                                        label="Subject group location"
                                        hint={SUBJECT_GROUP_HINT}
                                        options={folders}
                                    />
                                </div>
                            )}
                        </div>
                        <FormConclusion pathname={pathname} submittable={true} />
                    </form>
                </motion.div>
            </motion.div>
        );

    return <></>;
};

export default AddProject;

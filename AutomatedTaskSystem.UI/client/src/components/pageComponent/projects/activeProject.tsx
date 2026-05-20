import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import FormConclusion from "../../formComponents/FormConclusion";
import API from "../../../lib/API";
import { dismissFormModal } from "../../../lib/routerHelpers";
import { motion } from "framer-motion";
import { useAppDispatch } from "../../../app/hooks";
import { edit } from "../../../slices/projectSlice";

const ActivateProject = () => {
    const dispatch = useAppDispatch();
    const router = useRouter();
    const { query, pathname } = router;
    const [active, setActive] = useState<boolean>(false);
    const [project, setProject] = useState<IProject>();

    useEffect(() => {
        const sid = query.subjectId ?? query.projectId;
        const idStr = Array.isArray(sid) ? sid[0] : sid;
        if (query.form === "activate-project" && idStr) {
            API.PROJECTS.GET_ONE(idStr).then((res) => {
                if (res && !res.error) setProject(res.data);
            });
            return setActive(true);
        }
        setActive(false);
    }, [query]);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();

        project &&
            API.PROJECTS.UPDATE_STATUS({
                id: project.id,
                status: 0,
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
                animate={{ backgroundColor: "#00000055" }}
                className="z-50 flex items-center justify-center fixed top-0 left-0 right-0 min-h-screen"
            >
                <motion.div
                    initial={{ opacity: 0.1, height: "10rem" }}
                    animate={{ opacity: 1, height: "12rem" }}
                    className="bg-white px-5 py-4 basis-80 rounded-lg flex flex-col justify-between"
                >
                    {project ? (
                        <>
                            <h2 className="text-lg">Open Project</h2>
                            <div>
                                About to Open{" "}
                                <span className="font-bold text-emerald-600">
                                    {project.name}
                                </span>
                            </div>
                            <form onSubmit={handleSubmit}>
                                <FormConclusion
                                    pathname={pathname}
                                    submittable={true}
                                    type="emerald"
                                    text={{
                                        save: "Open",
                                    }}
                                />
                            </form>
                        </>
                    ) : (
                        <></>
                    )}
                </motion.div>
            </motion.div>
        );

    return <></>;
};

export default ActivateProject;

import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import FormConclusion from "../../formComponents/FormConclusion";
import API from "../../../lib/API";
import { motion } from "framer-motion";
import { useAppDispatch } from "../../../app/hooks";
import { edit } from "../../../slices/projectSlice";

const HoldProject = () => {
    const dispatch = useAppDispatch();
    const { query, pathname, push: routerPush } = useRouter();
    const [active, setActive] = useState<boolean>(false);
    const [project, setProject] = useState<IProject>();

    useEffect(() => {
        if (query.form === "hold-project" && query.projectId) {
            API.PROJECTS.GET_ONE(query.projectId.toString()).then((res) => {
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
                status: 2,
            }).then((res) => {
                if (res && !res.error) {
                    dispatch(edit(res.data));
                    routerPush(pathname);
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
                            <h2 className="text-lg">Hold Project</h2>
                            <div>
                                Put{" "}
                                <span className="font-bold text-teal-600">
                                    {project.name}
                                </span>{" "}
                                on Hold?
                            </div>
                            <form onSubmit={handleSubmit}>
                                <FormConclusion
                                    pathname="/projects"
                                    submittable={true}
                                    type="teal"
                                    text={{
                                        save: "Hold",
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

export default HoldProject;

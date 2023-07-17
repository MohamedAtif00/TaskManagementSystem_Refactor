import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import FormConclusion from "../../formComponents/FormConclusion";
import API from "../../../lib/API";
import { motion } from "framer-motion";
import { ExclamationTriangleIcon } from "@heroicons/react/24/outline";

interface Props {
    OnSubmit: () => void;
}

const RemoveNode: React.FC<Props> = ({ OnSubmit }) => {
    const { query, push: routerPush } = useRouter();
    const [active, setActive] = useState<boolean>(false);
    const [step, setStep] = useState<{
        id: number;
        name: string;
        isSafeToDelete: boolean;
    }>();

    useEffect(() => {
        if (query.form === "delete-node" && query.id) {
            API.SCHEMAS.NODES.DELETE_CHECK(query.id).then((res) => {
                if (res && !res.error) setStep(res.data);
            });
            return setActive(true);
        }
        setActive(false);
        setStep(undefined);
    }, [query]);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();

        step &&
            API.SCHEMAS.NODES.DELETE(step.id).then((res) => {
                if (res && !res.error) {
                    OnSubmit;
                    routerPush(`/schemas/${query.schemaId}`);
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
                    {step ? (
                        <>
                            <h2 className="text-lg">Delete Node</h2>
                            <div>
                                About to Delete{" "}
                                <span className="font-bold text-red-600">
                                    {step.name}
                                </span>
                            </div>
                            {!step.isSafeToDelete && (
                                <div className="flex items-center gap-2 text-red-600">
                                    <ExclamationTriangleIcon
                                        className="h-6 w-6"
                                        aria-hidden="true"
                                    />
                                    <div>Node may have running tasks</div>
                                </div>
                            )}
                            <form onSubmit={handleSubmit}>
                                <FormConclusion
                                    pathname={`/schemas/${query.schemaId}`}
                                    submittable={true}
                                    type="danger"
                                    text={{
                                        save: "DELETE",
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

export default RemoveNode;

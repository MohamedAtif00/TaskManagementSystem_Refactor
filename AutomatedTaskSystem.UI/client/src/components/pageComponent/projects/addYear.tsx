import { useRouter } from "next/router";
import InputTextField from "../../formComponents/InputTextField";
import { useEffect, useState } from "react";
import FormConclusion from "../../formComponents/FormConclusion";
import API from "../../../lib/API";
import { dismissFormModal } from "../../../lib/routerHelpers";
import { motion } from "framer-motion";

const AddYear = () => {
    const router = useRouter();
    const { query, pathname } = router;
    const [active, setActive] = useState(false);
    const [label, setLabel] = useState("");
    const [error, setError] = useState("");

    const rootId = query.rootProjectId
        ? Number(Array.isArray(query.rootProjectId) ? query.rootProjectId[0] : query.rootProjectId)
        : NaN;

    useEffect(() => {
        if (query.form === "add-year" && !Number.isNaN(rootId)) {
            setLabel("");
            setError("");
            setActive(true);
            return;
        }
        setActive(false);
    }, [query, rootId]);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setError("");
        if (!label.trim()) return setError("Please enter a year (e.g. 2026/2027)");

        API.PROJECTS.ROOT.CREATE_YEAR(rootId, label.trim()).then((res) => {
            if (res && typeof res === "object" && "error" in res && !res.error) {
                dismissFormModal(router);
            }
        });
    };

    if (!active) return null;

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
                <h2 className="text-lg mb-5">Add year</h2>
                <form onSubmit={handleSubmit} className="flex flex-col gap-8">
                    <div className="flex flex-col gap-2">
                        <div className="text-red-600">{error}</div>
                        <InputTextField
                            label="Year"
                            value={label}
                            handleChange={setLabel}
                        />
                        <p className="text-xs text-slate-500">Enter as plain text, e.g. 2026/2027</p>
                    </div>
                    <FormConclusion pathname={pathname} submittable={true} />
                </form>
            </motion.div>
        </motion.div>
    );
};

export default AddYear;

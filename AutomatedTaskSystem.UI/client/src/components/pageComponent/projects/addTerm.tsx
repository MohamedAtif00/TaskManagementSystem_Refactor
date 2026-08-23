import { useRouter } from "next/router";
import InputTextField from "../../formComponents/InputTextField";
import { useEffect, useState } from "react";
import FormConclusion from "../../formComponents/FormConclusion";
import API from "../../../lib/API";
import { dismissFormModal } from "../../../lib/routerHelpers";
import { motion } from "framer-motion";

const AddTerm = () => {
    const router = useRouter();
    const { query, pathname } = router;
    const [active, setActive] = useState(false);
    const [name, setName] = useState("");
    const [startDate, setStartDate] = useState("");
    const [endDate, setEndDate] = useState("");
    const [error, setError] = useState("");

    const yearId = query.yearId
        ? Number(Array.isArray(query.yearId) ? query.yearId[0] : query.yearId)
        : NaN;

    useEffect(() => {
        if (query.form === "add-term" && !Number.isNaN(yearId)) {
            setName("");
            setStartDate("");
            setEndDate("");
            setError("");
            setActive(true);
            return;
        }
        setActive(false);
    }, [query, yearId]);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setError("");
        if (!name.trim()) return setError("Please enter a term name");
        if (startDate && endDate && endDate < startDate)
            return setError("End date must be on or after start date");

        API.PROJECTS.ROOT.CREATE_TERM(yearId, {
            name: name.trim(),
            order: 0,
            startDate: startDate || null,
            endDate: endDate || null,
        }).then((res) => {
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
                <h2 className="text-lg mb-5">Add term</h2>
                <form onSubmit={handleSubmit} className="flex flex-col gap-8">
                    <div className="flex flex-col gap-3">
                        <div className="text-red-600">{error}</div>
                        <InputTextField label="Term name" value={name} handleChange={setName} />
                        <label className="flex flex-col gap-1 text-sm">
                            <span className="font-medium">Start date</span>
                            <input
                                type="date"
                                value={startDate}
                                onChange={(e) => setStartDate(e.target.value)}
                                className="border border-solid border-gray-300 rounded px-2 py-1"
                            />
                        </label>
                        <label className="flex flex-col gap-1 text-sm">
                            <span className="font-medium">End date</span>
                            <input
                                type="date"
                                value={endDate}
                                onChange={(e) => setEndDate(e.target.value)}
                                className="border border-solid border-gray-300 rounded px-2 py-1"
                            />
                        </label>
                    </div>
                    <FormConclusion pathname={pathname} submittable={true} />
                </form>
            </motion.div>
        </motion.div>
    );
};

export default AddTerm;

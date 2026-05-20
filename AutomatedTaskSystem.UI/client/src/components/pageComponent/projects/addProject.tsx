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

type IdName = { id: number; name: string };

const presetTermId = (query: ReturnType<typeof useRouter>["query"]) => {
    const t = query.termId;
    if (t === undefined) return null;
    const n = Number(Array.isArray(t) ? t[0] : t);
    return Number.isNaN(n) ? null : n;
};

const AddProject = () => {
    const dispatch = useAppDispatch();
    const router = useRouter();
    const { query, pathname } = router;
    const lockedTermId = presetTermId(query);
    const [active, setActive] = useState<boolean>(false);
    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [root, setRoot] = useState<IdName | null>(null);
    const [year, setYear] = useState<IdName | null>(null);
    const [term, setTerm] = useState<IdName | null>(null);
    const [roots, setRoots] = useState<IdName[]>([]);
    const [years, setYears] = useState<IdName[]>([]);
    const [terms, setTerms] = useState<IdName[]>([]);
    const [error, setError] = useState("");

    useEffect(() => {
        if (query.form === "add-project") {
            if (lockedTermId === null) {
                API.PROJECTS.ROOT.LIST().then((res) => {
                    if (res && typeof res === "object" && "error" in res && !res.error && "data" in res) {
                        setRoots((res as { data: IdName[] }).data);
                    }
                });
            }
            setName("");
            setDescription("");
            setRoot(null);
            setYear(null);
            setTerm(null);
            setYears([]);
            setTerms([]);
            return setActive(true);
        }
        setActive(false);
    }, [query, lockedTermId]);

    useEffect(() => {
        if (!active || lockedTermId !== null) return;
        if (root === null) {
            setYears([]);
            setYear(null);
            setTerms([]);
            setTerm(null);
            return;
        }
        API.PROJECTS.ROOT.YEARS(root.id).then((res) => {
            if (res && typeof res === "object" && "error" in res && !res.error && "data" in res) {
                setYears((res as { data: IdName[] }).data);
            } else setYears([]);
        });
    }, [root, active, lockedTermId]);

    useEffect(() => {
        if (!active || lockedTermId !== null) return;
        if (year === null) {
            setTerms([]);
            setTerm(null);
            return;
        }
        API.PROJECTS.ROOT.TERMS(year.id).then((res) => {
            if (res && typeof res === "object" && "error" in res && !res.error && "data" in res) {
                setTerms((res as { data: IdName[] }).data);
            } else setTerms([]);
        });
    }, [year, active, lockedTermId]);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setError("");

        if (name === "") return setError("Please enter name");

        const termId =
            lockedTermId !== null ? lockedTermId : term === null ? null : term.id;

        if (termId === null) {
            if (root === null) return setError("Please select a project");
            if (year === null) return setError("Please select a year");
            return setError("Please select a term");
        }

        API.PROJECTS.CREATE({
            name,
            description,
            termId,
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
                            {lockedTermId === null && (
                                <div className="flex flex-col gap-3">
                                    <Dropdown
                                        value={root}
                                        handleChange={(v) => {
                                            setRoot(v);
                                            setYear(null);
                                            setTerm(null);
                                        }}
                                        label="Project"
                                        options={roots}
                                    />
                                    <Dropdown
                                        value={year}
                                        handleChange={(v) => {
                                            setYear(v);
                                            setTerm(null);
                                        }}
                                        label="Year"
                                        options={years}
                                    />
                                    <Dropdown
                                        value={term}
                                        handleChange={setTerm}
                                        label="Term"
                                        options={terms}
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

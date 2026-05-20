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

type IdName = { id: number; name: string };

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
    const [root, setRoot] = useState<IdName | null>(null);
    const [year, setYear] = useState<IdName | null>(null);
    const [term, setTerm] = useState<IdName | null>(null);
    const [roots, setRoots] = useState<IdName[]>([]);
    const [years, setYears] = useState<IdName[]>([]);
    const [terms, setTerms] = useState<IdName[]>([]);
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
                const rootsRes = await API.PROJECTS.ROOT.LIST();
                if (!cancelled && rootsRes && typeof rootsRes === "object" && "error" in rootsRes && !rootsRes.error && "data" in rootsRes) {
                    setRoots((rootsRes as { data: IdName[] }).data);
                }
                const yearsRes = await API.PROJECTS.ROOT.YEARS(s.rootProjectId);
                if (!cancelled && yearsRes && typeof yearsRes === "object" && "error" in yearsRes && !yearsRes.error && "data" in yearsRes) {
                    setYears((yearsRes as { data: IdName[] }).data);
                }
                const termsRes = await API.PROJECTS.ROOT.TERMS(s.projectYearId);
                if (!cancelled && termsRes && typeof termsRes === "object" && "error" in termsRes && !termsRes.error && "data" in termsRes) {
                    setTerms((termsRes as { data: IdName[] }).data);
                }
                if (!cancelled) {
                    setRoot(s.rootProject);
                    setYear(s.projectYear);
                    setTerm(s.term);
                }
            })();
            return setActive(true);
        }
        setActive(false);
    }, [query]);

    useEffect(() => {
        if (!active || root === null) return;
        API.PROJECTS.ROOT.YEARS(root.id).then((res) => {
            if (res && typeof res === "object" && "error" in res && !res.error && "data" in res) {
                setYears((res as { data: IdName[] }).data);
            }
        });
    }, [root?.id, active]);

    useEffect(() => {
        if (!active || year === null) return;
        API.PROJECTS.ROOT.TERMS(year.id).then((res) => {
            if (res && typeof res === "object" && "error" in res && !res.error && "data" in res) {
                setTerms((res as { data: IdName[] }).data);
            }
        });
    }, [year?.id, active]);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setError("");

        const sid = subjectIdFromQuery(query);
        if (!sid) return setError("Missing subject");

        if (name === "") return setError("Please enter name");
        if (root === null) return setError("Please select a curriculum project");
        if (year === null) return setError("Please select an academic year");
        if (term === null) return setError("Please select a term");

        API.PROJECTS.EDIT({
            id: sid,
            name,
            description,
            termId: term.id,
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

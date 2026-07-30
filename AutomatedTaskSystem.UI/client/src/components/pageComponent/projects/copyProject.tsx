import { useRouter } from "next/router";
import InputTextField from "../../formComponents/InputTextField";
import { useEffect, useMemo, useState } from "react";
import FormConclusion from "../../formComponents/FormConclusion";
import API, { BasicInfo } from "../../../lib/API";
import { dismissFormModal } from "../../../lib/routerHelpers";
import { motion } from "framer-motion";
import { useAppDispatch } from "../../../app/hooks";
import { add } from "../../../slices/projectSlice";
import CustomizedCombobox from "../../formComponents/Combobox";

type LoRow = {
    id: number;
    name: string;
    unitName: string;
    lessonName: string;
    sourceSchemaId: number;
    sourceSchemaName: string;
};

const subjectIdFromQuery = (query: ReturnType<typeof useRouter>["query"]) => {
    const s = query.subjectId ?? query.projectId;
    if (Array.isArray(s)) return s[0];
    return s;
};

const flattenLearningObjectives = (details: ProjectDetails): LoRow[] =>
    details.units.flatMap((unit) =>
        unit.lessons.flatMap((lesson) =>
            lesson.learningObjectives.map((lo) => ({
                id: lo.id,
                name: lo.name,
                unitName: unit.name,
                lessonName: lesson.name,
                sourceSchemaId: lo.schema.id,
                sourceSchemaName: lo.schema.name,
            }))
        )
    );

const buildSchemaOverrides = (
    rows: LoRow[],
    overrides: Record<number, number>
) => {
    const groups = new Map<number, number[]>();

    for (const row of rows) {
        const effectiveSchemaId = overrides[row.id] ?? row.sourceSchemaId;
        if (effectiveSchemaId === row.sourceSchemaId) continue;

        const existing = groups.get(effectiveSchemaId) ?? [];
        existing.push(row.id);
        groups.set(effectiveSchemaId, existing);
    }

    return Array.from(groups.entries()).map(([schemaId, sourceLearningObjectiveIds]) => ({
        schemaId,
        sourceLearningObjectiveIds,
    }));
};

const CopyProject = ({ onSuccess }: { onSuccess?: () => void }) => {
    const dispatch = useAppDispatch();
    const router = useRouter();
    const { query, pathname } = router;
    const [active, setActive] = useState(false);
    const [sourceName, setSourceName] = useState("");
    const [name, setName] = useState("");
    const [loRows, setLoRows] = useState<LoRow[]>([]);
    const [schemaOverrides, setSchemaOverrides] = useState<Record<number, number>>({});
    const [selectedLoIds, setSelectedLoIds] = useState<Set<number>>(new Set());
    const [bulkSchema, setBulkSchema] = useState<BasicInfo>();
    const [schemas, setSchemas] = useState<BasicInfo[]>([]);
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        const sid = subjectIdFromQuery(query);
        if (query.form === "copy-project" && sid) {
            let cancelled = false;
            setLoading(true);
            setError("");
            setSchemaOverrides({});
            setSelectedLoIds(new Set());
            setBulkSchema(undefined);

            (async () => {
                const [detailsRes, schemasRes] = await Promise.all([
                    API.PROJECTS.GET_ONE_DETAILED(sid),
                    API.SCHEMAS.GET_ALL(),
                ]);

                if (cancelled) return;

                if (!detailsRes || detailsRes.error) {
                    setError("Could not load subject details");
                    setLoading(false);
                    return;
                }

                const rows = flattenLearningObjectives(detailsRes.data);
                setSourceName(detailsRes.data.name);
                setName(`${detailsRes.data.name} - Copy`);
                setLoRows(rows);

                if (Array.isArray(schemasRes)) {
                    setSchemas(
                        schemasRes.map((s: { id: number; name: string }) => ({
                            id: s.id,
                            name: s.name,
                        }))
                    );
                }

                setLoading(false);
            })();

            return setActive(true);
        }

        setActive(false);
    }, [query]);

    const schemaNameById = useMemo(() => {
        const map = new Map<number, string>();
        for (const row of loRows) {
            map.set(row.sourceSchemaId, row.sourceSchemaName);
        }
        for (const schema of schemas) {
            map.set(schema.id, schema.name);
        }
        return map;
    }, [loRows, schemas]);

    const toggleLoSelection = (loId: number) => {
        setSelectedLoIds((prev) => {
            const next = new Set(prev);
            if (next.has(loId)) next.delete(loId);
            else next.add(loId);
            return next;
        });
    };

    const toggleSelectAll = () => {
        if (selectedLoIds.size === loRows.length) {
            setSelectedLoIds(new Set());
            return;
        }
        setSelectedLoIds(new Set(loRows.map((row) => row.id)));
    };

    const applySchemaToSelected = () => {
        if (!bulkSchema || selectedLoIds.size === 0) {
            return setError("Select learning objectives and a schema to apply");
        }

        setError("");
        setSchemaOverrides((prev) => {
            const next = { ...prev };
            selectedLoIds.forEach((loId) => {
                next[loId] = bulkSchema.id;
            });
            return next;
        });
    };

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setError("");

        const sid = subjectIdFromQuery(query);
        if (!sid) return setError("Missing subject");
        if (name.trim() === "") return setError("Please enter a name");
        if (loRows.length === 0) return setError("Subject has no learning objectives to copy");

        API.PROJECTS.COPY({
            id: sid,
            name: name.trim(),
            schemaOverrides: buildSchemaOverrides(loRows, schemaOverrides),
        }).then((res) => {
            if (res && !res.error) {
                dispatch(add(res.data));
                onSuccess?.();
                dismissFormModal(router);
            } else if (res && res.error) {
                setError(res.message || "Copy failed");
            }
        });
    };

    if (!active) return <></>;

    return (
        <motion.div
            initial={{ backgroundColor: "#00000000" }}
            animate={{ backgroundColor: "#00000055", height: "auto" }}
            className="z-50 flex items-center justify-center fixed top-0 left-0 right-0 min-h-screen p-4"
        >
            <motion.div
                initial={{ opacity: 0.1 }}
                animate={{ opacity: 1 }}
                className="bg-white px-5 py-4 rounded-lg w-full max-w-4xl max-h-[90vh] overflow-y-auto"
            >
                <h2 className="text-lg mb-2">Copy subject</h2>
                <p className="text-slate-600 text-sm mb-4">
                    Copying <span className="font-semibold text-cyan-700">{sourceName}</span> into
                    the same subject group with a new name.
                </p>

                {loading ? (
                    <div className="py-8 text-center text-slate-500">Loading...</div>
                ) : (
                    <form onSubmit={handleSubmit} className="flex flex-col gap-6">
                        <div className="text-red-600">{error}</div>
                        <InputTextField label="New name" value={name} handleChange={setName} />

                        <div className="flex flex-col gap-3">
                            <div className="font-medium">Learning objectives</div>
                            <div className="flex flex-wrap items-end gap-3 p-3 bg-slate-50 rounded-lg border border-slate-200">
                                <div className="flex-1 min-w-[200px]">
                                    <label className="text-sm text-slate-600 block mb-1">
                                        Schema to apply
                                    </label>
                                    <CustomizedCombobox
                                        value={bulkSchema}
                                        onChange={setBulkSchema}
                                        options={schemas}
                                    />
                                </div>
                                <button
                                    type="button"
                                    className="px-4 py-2 bg-cyan-600 text-white rounded-lg hover:bg-cyan-700 disabled:opacity-50"
                                    onClick={applySchemaToSelected}
                                    disabled={!bulkSchema || selectedLoIds.size === 0}
                                >
                                    Apply to selected ({selectedLoIds.size})
                                </button>
                            </div>

                            <div className="border border-slate-200 rounded-lg overflow-hidden">
                                <div className="grid grid-cols-[auto_1fr_1fr_1fr_1fr] gap-2 px-3 py-2 bg-slate-100 text-sm font-medium">
                                    <div>
                                        <input
                                            type="checkbox"
                                            checked={
                                                loRows.length > 0 &&
                                                selectedLoIds.size === loRows.length
                                            }
                                            onChange={toggleSelectAll}
                                            aria-label="Select all learning objectives"
                                        />
                                    </div>
                                    <div>Location</div>
                                    <div>LO name</div>
                                    <div>Source schema</div>
                                    <div>Effective schema</div>
                                </div>
                                {loRows.map((row) => {
                                    const effectiveSchemaId =
                                        schemaOverrides[row.id] ?? row.sourceSchemaId;
                                    const effectiveSchemaName =
                                        schemaNameById.get(effectiveSchemaId) ?? "Unknown";
                                    const changed = effectiveSchemaId !== row.sourceSchemaId;

                                    return (
                                        <div
                                            key={row.id}
                                            className={`grid grid-cols-[auto_1fr_1fr_1fr_1fr] gap-2 px-3 py-2 text-sm border-t border-slate-100 ${
                                                changed ? "bg-cyan-50" : ""
                                            }`}
                                        >
                                            <div>
                                                <input
                                                    type="checkbox"
                                                    checked={selectedLoIds.has(row.id)}
                                                    onChange={() => toggleLoSelection(row.id)}
                                                    aria-label={`Select ${row.name}`}
                                                />
                                            </div>
                                            <div className="text-slate-600">
                                                {row.unitName} › {row.lessonName}
                                            </div>
                                            <div>{row.name}</div>
                                            <div>{row.sourceSchemaName}</div>
                                            <div className={changed ? "font-medium text-cyan-700" : ""}>
                                                {effectiveSchemaName}
                                            </div>
                                        </div>
                                    );
                                })}
                            </div>
                        </div>

                        <FormConclusion
                            pathname={pathname}
                            submittable={!loading && loRows.length > 0}
                            type="chill"
                            text={{ save: "Copy subject" }}
                        />
                    </form>
                )}
            </motion.div>
        </motion.div>
    );
};

export default CopyProject;

import { useEffect, useMemo, useState } from "react";
import API from "../../lib/API";
import { IDName } from "../../lib/API/workFromHome";
import ImportLosFromExcel from "./importLosFromExcel";

interface SprintLoPickerProps {
    selectedLos: IDName[];
    selectedProjectId: string;
    onSelectLos: (los: IDName[]) => void;
    onDeselectLo: (lo: IDName) => void;
    onImported: (matched: IDName[]) => void;
}

const ALL = "";

type HierarchyLesson = {
    id: number;
    name: string;
    loIds: number[];
};

type HierarchyUnit = {
    id: number;
    name: string;
    lessons: HierarchyLesson[];
    loIds: number[];
};

const SprintLoPicker = ({
    selectedLos,
    selectedProjectId,
    onSelectLos,
    onDeselectLo,
    onImported,
}: SprintLoPickerProps) => {
    const [eligibleLos, setEligibleLos] = useState<IDName[]>([]);
    const [units, setUnits] = useState<HierarchyUnit[]>([]);
    const [isLoadingLOs, setIsLoadingLOs] = useState(false);
    const [loFetchError, setLoFetchError] = useState("");
    const [unitFilter, setUnitFilter] = useState(ALL);
    const [lessonFilter, setLessonFilter] = useState(ALL);

    useEffect(() => {
        setUnitFilter(ALL);
        setLessonFilter(ALL);
        setEligibleLos([]);
        setUnits([]);
        setLoFetchError("");

        if (!selectedProjectId) return;

        let cancelled = false;

        const fetchHierarchy = async () => {
            setIsLoadingLOs(true);
            try {
                const [eligibleRes, detailsRes] = await Promise.all([
                    API.PROJECTS.GET_ALL_LOS(Number(selectedProjectId)),
                    API.PROJECTS.GET_ONE_DETAILED(selectedProjectId),
                ]);

                if (cancelled) return;

                if (!eligibleRes || eligibleRes.error || !eligibleRes.data) {
                    setEligibleLos([]);
                    setUnits([]);
                    setLoFetchError(eligibleRes?.message || "Failed to fetch learning outcomes.");
                    return;
                }

                if (!detailsRes || detailsRes === false || detailsRes.error || !detailsRes.data) {
                    setEligibleLos([]);
                    setUnits([]);
                    setLoFetchError(
                        (detailsRes && detailsRes !== false && detailsRes.message) ||
                            "Failed to fetch subject units and lessons."
                    );
                    return;
                }

                const eligible = eligibleRes.data as IDName[];
                const eligibleIds = new Set(eligible.map((lo) => lo.id));

                const hierarchyUnits: HierarchyUnit[] = (detailsRes.data.units || []).map((unit) => {
                    const lessons: HierarchyLesson[] = (unit.lessons || []).map((lesson) => {
                        const loIds = (lesson.learningObjectives || [])
                            .map((lo) => lo.id)
                            .filter((id) => eligibleIds.has(id));
                        return { id: lesson.id, name: lesson.name, loIds };
                    });
                    const loIds = lessons.flatMap((lesson) => lesson.loIds);
                    return { id: unit.id, name: unit.name, lessons, loIds };
                });

                setEligibleLos(eligible);
                setUnits(hierarchyUnits);
                setLoFetchError("");
            } catch (err) {
                console.error("Error fetching learning outcomes hierarchy:", err);
                if (!cancelled) {
                    setEligibleLos([]);
                    setUnits([]);
                    setLoFetchError("An error occurred while fetching learning outcomes.");
                }
            } finally {
                if (!cancelled) setIsLoadingLOs(false);
            }
        };

        fetchHierarchy();
        return () => {
            cancelled = true;
        };
    }, [selectedProjectId]);

    const selectedIds = useMemo(() => new Set(selectedLos.map((lo) => lo.id)), [selectedLos]);

    const eligibleById = useMemo(() => {
        const map = new Map<number, IDName>();
        eligibleLos.forEach((lo) => map.set(lo.id, lo));
        return map;
    }, [eligibleLos]);

    const unselectedEligible = useMemo(
        () => eligibleLos.filter((lo) => !selectedIds.has(lo.id)),
        [eligibleLos, selectedIds]
    );

    const unitOptions = useMemo(
        () =>
            units.filter((unit) =>
                unit.loIds.some((id) => !selectedIds.has(id) && eligibleById.has(id))
            ),
        [units, selectedIds, eligibleById]
    );

    const selectedUnit = useMemo(
        () => (unitFilter === ALL ? null : units.find((unit) => String(unit.id) === unitFilter) ?? null),
        [units, unitFilter]
    );

    const lessonOptions = useMemo(() => {
        if (!selectedUnit) return [];
        return selectedUnit.lessons.filter((lesson) =>
            lesson.loIds.some((id) => !selectedIds.has(id) && eligibleById.has(id))
        );
    }, [selectedUnit, selectedIds, eligibleById]);

    useEffect(() => {
        if (unitFilter !== ALL && !unitOptions.some((unit) => String(unit.id) === unitFilter)) {
            setUnitFilter(ALL);
            setLessonFilter(ALL);
        }
    }, [unitOptions, unitFilter]);

    useEffect(() => {
        if (lessonFilter !== ALL && !lessonOptions.some((lesson) => String(lesson.id) === lessonFilter)) {
            setLessonFilter(ALL);
        }
    }, [lessonOptions, lessonFilter]);

    const filteredAvailable = useMemo(() => {
        if (unitFilter === ALL) {
            return unselectedEligible;
        }

        const unit = selectedUnit;
        if (!unit) return [];

        const loIds =
            lessonFilter === ALL
                ? unit.loIds
                : unit.lessons.find((lesson) => String(lesson.id) === lessonFilter)?.loIds ?? [];

        return loIds
            .filter((id) => !selectedIds.has(id))
            .map((id) => eligibleById.get(id))
            .filter((lo): lo is IDName => Boolean(lo));
    }, [unitFilter, lessonFilter, selectedUnit, unselectedEligible, selectedIds, eligibleById]);

    const hasActiveFilter = unitFilter !== ALL || lessonFilter !== ALL;

    const clearFilters = () => {
        setUnitFilter(ALL);
        setLessonFilter(ALL);
    };

    const selectClassName =
        "w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring focus:border-blue-500 text-sm bg-white";

    return (
        <div>
            <label className="block text-gray-700 font-medium mb-1">Learning Outcomes</label>
            <ImportLosFromExcel selectedLos={selectedLos} onImported={onImported} />
            {isLoadingLOs && selectedProjectId && (
                <p className="text-gray-500 mt-2">Loading learning outcomes...</p>
            )}
            {loFetchError && <p className="text-red-500 mt-2">{loFetchError}</p>}

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 mt-3">
                <div>
                    <label htmlFor="lo-filter-unit" className="block text-gray-600 text-sm mb-1">
                        Unit
                    </label>
                    <select
                        id="lo-filter-unit"
                        value={unitFilter}
                        onChange={(e) => {
                            setUnitFilter(e.target.value);
                            setLessonFilter(ALL);
                        }}
                        disabled={!selectedProjectId || isLoadingLOs}
                        className={selectClassName}
                    >
                        <option value={ALL}>All</option>
                        {unitOptions.map((unit) => (
                            <option key={unit.id} value={String(unit.id)}>
                                {unit.name}
                            </option>
                        ))}
                    </select>
                </div>
                <div>
                    <label htmlFor="lo-filter-lesson" className="block text-gray-600 text-sm mb-1">
                        Lesson
                    </label>
                    <select
                        id="lo-filter-lesson"
                        value={lessonFilter}
                        onChange={(e) => setLessonFilter(e.target.value)}
                        disabled={!selectedProjectId || isLoadingLOs || unitFilter === ALL}
                        className={selectClassName}
                    >
                        <option value={ALL}>All</option>
                        {lessonOptions.map((lesson) => (
                            <option key={lesson.id} value={String(lesson.id)}>
                                {lesson.name}
                            </option>
                        ))}
                    </select>
                </div>
            </div>

            <div className="flex flex-wrap items-center gap-2 mt-3">
                <button
                    type="button"
                    onClick={() => onSelectLos(filteredAvailable)}
                    disabled={filteredAvailable.length === 0}
                    className="bg-blue-600 text-white px-3 py-1.5 rounded text-sm hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed"
                >
                    Add all filtered ({filteredAvailable.length})
                </button>
                <button
                    type="button"
                    onClick={clearFilters}
                    disabled={!hasActiveFilter}
                    className="bg-gray-200 text-gray-700 px-3 py-1.5 rounded text-sm hover:bg-gray-300 disabled:bg-gray-100 disabled:text-gray-400 disabled:cursor-not-allowed"
                >
                    Clear filters
                </button>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 border p-4 rounded-lg mt-3">
                <div>
                    <h4 className="font-semibold mb-2 text-gray-800">Available</h4>
                    <div className="h-48 overflow-y-auto border rounded p-2 space-y-1 bg-gray-50">
                        {!selectedProjectId && (
                            <p className="text-gray-500 text-sm p-2">
                                Select a project to see available LOs, or import from Excel.
                            </p>
                        )}
                        {selectedProjectId &&
                            filteredAvailable.map((lo) => (
                                <div
                                    key={lo.id}
                                    onClick={() => onSelectLos([lo])}
                                    className="p-2 border rounded cursor-pointer hover:bg-blue-100 transition-colors"
                                >
                                    {lo.name}
                                </div>
                            ))}
                        {selectedProjectId && !isLoadingLOs && filteredAvailable.length === 0 && (
                            <p className="text-gray-500 text-sm p-2">
                                {eligibleLos.length === 0
                                    ? "No learning outcomes available for this project."
                                    : unselectedEligible.length === 0
                                      ? "All available LOs selected."
                                      : "No learning outcomes match these filters."}
                            </p>
                        )}
                    </div>
                </div>
                <div>
                    <h4 className="font-semibold mb-2 text-gray-800">Selected ({selectedLos.length})</h4>
                    <div className="h-48 overflow-y-auto border rounded p-2 space-y-1 bg-blue-50">
                        {selectedLos.map((lo) => (
                            <div
                                key={lo.id}
                                onClick={() => onDeselectLo(lo)}
                                className="p-2 border rounded cursor-pointer hover:bg-red-100 transition-colors flex justify-between items-center bg-white"
                            >
                                <span>{lo.name}</span>
                                <span className="text-red-500 font-bold text-lg leading-none">&times;</span>
                            </div>
                        ))}
                        {selectedLos.length === 0 && (
                            <p className="text-gray-500 text-sm p-2">
                                Import from Excel or click an available LO to select it.
                            </p>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default SprintLoPicker;

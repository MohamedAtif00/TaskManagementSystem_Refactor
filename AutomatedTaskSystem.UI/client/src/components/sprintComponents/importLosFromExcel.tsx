import { useRef, useState } from "react";
import * as XLSX from "xlsx";
import API from "../../lib/API";
import { IDName } from "../../lib/API/workFromHome";
import { LoNameError } from "../../lib/API/Sprints.d";

const MAX_FILE_BYTES = 2 * 1024 * 1024;
const MAX_DATA_ROWS = 500;
const ACCEPTED_HEADERS = ["name", "lo name", "learning objective", "learning outcome"];
const ACCEPTED_EXTENSIONS = [".xlsx", ".xls"];

interface ImportLosFromExcelProps {
    selectedLos: IDName[];
    onImported: (matched: IDName[]) => void;
}

const cellToString = (value: unknown): string => {
    if (value === null || value === undefined) return "";
    return String(value).trim();
};

const findNameHeader = (headers: string[]): string | undefined =>
    headers.find((header) => ACCEPTED_HEADERS.includes(header.trim().toLowerCase()));

const downloadTemplate = () => {
    const worksheet = XLSX.utils.aoa_to_sheet([
        ["Name"],
        ["Example learning objective 1"],
        ["Example learning objective 2"],
    ]);
    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, "Learning Objectives");
    XLSX.writeFile(workbook, "sprint-lo-import-template.xlsx");
};

const ImportLosFromExcel = ({ selectedLos, onImported }: ImportLosFromExcelProps) => {
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [isImporting, setIsImporting] = useState(false);
    const [fileError, setFileError] = useState("");
    const [addedCount, setAddedCount] = useState<number | null>(null);
    const [errors, setErrors] = useState<LoNameError[]>([]);

    const resetFeedback = () => {
        setFileError("");
        setAddedCount(null);
        setErrors([]);
    };

    const handleFileChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0];
        event.target.value = "";
        if (!file) return;

        resetFeedback();

        const extension = file.name.slice(file.name.lastIndexOf(".")).toLowerCase();
        if (!ACCEPTED_EXTENSIONS.includes(extension)) {
            setFileError("Please upload an Excel file (.xlsx or .xls).");
            return;
        }
        if (file.size > MAX_FILE_BYTES) {
            setFileError("File is too large. Maximum size is 2 MB.");
            return;
        }

        try {
            const buffer = await file.arrayBuffer();
            const workbook = XLSX.read(buffer, { type: "array" });
            const sheetName = workbook.SheetNames[0];
            if (!sheetName) {
                setFileError("The Excel file has no sheets.");
                return;
            }

            const sheet = workbook.Sheets[sheetName];
            const rows = XLSX.utils.sheet_to_json<Record<string, unknown>>(sheet, { defval: "" });
            if (rows.length === 0) {
                setFileError("The Excel file has no learning objective names.");
                return;
            }
            if (rows.length > MAX_DATA_ROWS) {
                setFileError(`A maximum of ${MAX_DATA_ROWS} rows can be imported at once.`);
                return;
            }

            const headers = Object.keys(rows[0] ?? {});
            const nameHeader = findNameHeader(headers);
            if (!nameHeader) {
                setFileError('The first sheet must have a "Name" column.');
                return;
            }

            const uniqueNames: string[] = [];
            const seen = new Set<string>();
            const duplicateErrors: LoNameError[] = [];

            for (const row of rows) {
                const name = cellToString(row[nameHeader]);
                if (!name) continue;
                const key = name.toLowerCase();
                if (seen.has(key)) {
                    duplicateErrors.push({
                        name,
                        message: "Duplicate name in the file",
                    });
                    continue;
                }
                seen.add(key);
                uniqueNames.push(name);
            }

            if (uniqueNames.length === 0) {
                setFileError("The Excel file has no learning objective names.");
                setErrors(duplicateErrors);
                return;
            }

            setIsImporting(true);
            const response = await API.SPRINTS.RESOLVE_LOS_BY_NAME(uniqueNames);
            if (!response || response.error || !response.data) {
                setFileError(response?.message || "Failed to validate learning objectives.");
                setErrors(duplicateErrors);
                return;
            }

            const alreadySelected = new Set(selectedLos.map((lo) => lo.id));
            const toAdd = (response.data.matched || []).filter((lo) => !alreadySelected.has(lo.id));
            if (toAdd.length > 0) {
                onImported(toAdd);
            }

            setAddedCount(toAdd.length);
            setErrors([...(response.data.errors || []), ...duplicateErrors]);
        } catch (err) {
            console.error(err);
            setFileError("Could not read the Excel file. Please use the template and try again.");
        } finally {
            setIsImporting(false);
        }
    };

    return (
        <div className="border border-dashed border-gray-300 rounded-lg p-4 bg-gray-50 space-y-3">
            <div className="flex flex-wrap items-center justify-between gap-3">
                <div>
                    <p className="text-sm font-medium text-gray-800">Import from Excel</p>
                    <p className="text-xs text-gray-500">Use a Name column to add learning outcomes quickly.</p>
                </div>
                <div className="flex items-center gap-2">
                    <button
                        type="button"
                        onClick={downloadTemplate}
                        className="px-3 py-1.5 text-sm rounded border border-gray-300 bg-white text-gray-700 hover:bg-gray-100"
                    >
                        Download template
                    </button>
                    <button
                        type="button"
                        onClick={() => fileInputRef.current?.click()}
                        disabled={isImporting}
                        className="px-3 py-1.5 text-sm rounded bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-60"
                    >
                        {isImporting ? "Importing..." : "Upload Excel"}
                    </button>
                    <input
                        ref={fileInputRef}
                        type="file"
                        accept=".xlsx,.xls,application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                        className="hidden"
                        onChange={handleFileChange}
                    />
                </div>
            </div>
            {fileError && <p className="text-sm text-red-600">{fileError}</p>}
            {addedCount !== null && !fileError && (
                <p className="text-sm text-green-700">
                    {addedCount} learning outcome{addedCount === 1 ? "" : "s"} added.
                </p>
            )}
            {errors.length > 0 && (
                <div className="max-h-32 overflow-y-auto rounded border border-red-200 bg-red-50 p-2 space-y-1">
                    {errors.map((error, index) => (
                        <p key={`${error.name}-${index}`} className="text-xs text-red-700">
                            {error.name}: {error.message}
                        </p>
                    ))}
                </div>
            )}
        </div>
    );
};

export default ImportLosFromExcel;

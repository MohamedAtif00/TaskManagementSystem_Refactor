import { useEffect, useState } from "react";
import API from "../../lib/API";
import Loader from "../../components/loader";
import { GridColDef } from "@mui/x-data-grid";
import HierarchyListPage from "../../components/curriculum/HierarchyListPage";

type IdName = { id: number; name: string };

const projectColumns: GridColDef[] = [
    { field: "col0", headerName: "ID", width: 90 },
    { field: "col1", headerName: "Name", width: 280 },
    { field: "col2", headerName: "Description", width: 360 },
];

const TasksIndex = () => {
    const [roots, setRoots] = useState<IdName[]>();
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        API.PROJECTS.ROOT.LIST().then((res) => {
            if (res && typeof res === "object" && "error" in res && !res.error && "data" in res) {
                setRoots((res as { data: IdName[] }).data);
            } else {
                setRoots([]);
            }
            setLoading(false);
        });
    }, []);

    if (loading || roots === undefined) {
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Loader />
            </div>
        );
    }

    return (
        <HierarchyListPage
            title="Tasks"
            pageTitle="Tasks"
            icon="task"
            showAdd={false}
            rows={roots.map((r) => ({
                id: r.id,
                col0: r.id,
                col1: r.name,
                col2: "",
            }))}
            columns={projectColumns}
            rowHref={(id) => `/tasks/browse/${id}`}
        />
    );
};

export default TasksIndex;

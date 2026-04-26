import { useState, useEffect, CSSProperties } from "react";
import { useRouter } from "next/router";
import Head from "next/head";
import API from "../../../../lib/API";
import { DataGrid, GridColDef, GridRenderCellParams } from "@mui/x-data-grid";
import { PieChart } from "@mui/x-charts/PieChart";
import { Box } from "@mui/material";
import { LightDropdown } from "../../../../components/formComponents/LightDropdown";
import { ProjectStatusData } from "../../../../components/charts/ProjectStatusChart";
import ProjectOverview from "./overview";

type ChartView = "overview" | "progress";

const Tab = ({
  label,
  active,
  onClick,
}: {
  label: string;
  active: boolean;
  onClick: () => void;
}) => {
  return (
    <button
      onClick={onClick}
      className={`
        relative px-4 py-2 text-sm font-medium transition-colors
        ${active ? "text-blue-600" : "text-gray-400 hover:text-gray-600"}
      `}
    >
      {label}
      {active && (
        <div className="absolute bottom-0 left-0 right-0 h-0.5 bg-blue-600" />
      )}
    </button>
  );
};

const StatusBadge = ({ status }: { status: string }) => {
  const getStatusStyles = (s: string) => {
    switch (s) {
      case "On Track":
        return "bg-green-500 text-white";
      case "At Risk":
        return "bg-yellow-400 text-white";
      case "Delayed":
        return "bg-red-400 text-white";
      default:
        return "bg-gray-400 text-white";
    }
  };
  return (
    <span
      className={`px-3 py-1 rounded-md text-sm font-medium ${getStatusStyles(
        status
      )}`}
    >
      {status}
    </span>
  );
};

const ProgressDonut = ({ percentage }: { percentage: number }) => {
  const getStatusColor = (perc: number) => {
    if (perc >= 75) return "#22c55e";
    if (perc >= 50) return "#fbbf24";
    return "#ef4444";
  };

  const progressData = [
    { label: "Done", value: percentage, color: getStatusColor(percentage) },
    { label: "Pending", value: 100 - percentage, color: "#e5e7eb" },
  ];

  return (
    <Box
      className="relative flex items-center justify-center"
      sx={{ width: 50, height: 50 }}
    >
      <PieChart
        series={[
          {
            innerRadius: 10,
            outerRadius: 18,
            data: progressData,
            cx: "50%",
            cy: "50%",
            startAngle: -130,
            endAngle: 230,
            paddingAngle: 0,
          },
        ]}
        width={50}
        height={50}
        margin={{ top: 0, bottom: 0, left: 0, right: 0 }}
        hideLegend={true}
      />
      <Box
        sx={{
          position: "absolute",
          top: "50%",
          left: "50%",
          transform: "translate(-50%, -50%)",
          fontSize: "10px",
          fontWeight: "bold",
        }}
      >
        {percentage}%
      </Box>
    </Box>
  );
};

const CurrentPhaseDisplay = ({
  phases,
}: {
  phases: { groupName: string; colorCode: string }[];
}) => {
  if (!phases || phases.length === 0)
    return <span className="text-gray-400">-</span>;
  return (
    <div className="flex flex-col gap-1">
      {phases.map((phase, index) => (
        <div key={index} className="flex items-center gap-1">
          <span
            className="w-2 h-2 rounded-full"
            style={{ backgroundColor: phase.colorCode }}
          />
          <span className="text-sm">{phase.groupName}</span>
        </div>
      ))}
    </div>
  );
};

const ProjectChartsPage = () => {
  const router = useRouter();
  const { projectChartId } = router.query;
  const chartId =
    router.isReady && projectChartId
      ? Array.isArray(projectChartId)
        ? String(projectChartId[0])
        : String(projectChartId)
      : "";
  const [activeTab, setActiveTab] = useState<ChartView>("overview");
  const [loTableData, setLoTableData] = useState<any[]>([]);
  const [isLoadingTable, setIsLoadingTable] = useState(false);
  const [projectName, setProjectName] = useState("");

  useEffect(() => {
    if (!router.isReady) return;
    const fetchProjectName = async () => {
      if (!chartId) return;
      try {
        const response = await API.PROJECTS.GET_PROJECT_LO_TABLE(chartId);
        if (response && !response.error && response.data?.projectName) {
          setProjectName(response.data.projectName);
        }
      } catch (error) {
        console.error("Error fetching project name:", error);
      }
    };
    fetchProjectName();
  }, [router.isReady, chartId]);

  useEffect(() => {
    if (!router.isReady) return;
    const fetchLOTable = async () => {
      if (activeTab === "progress" && chartId) {
        try {
          setIsLoadingTable(true);
          const response = await API.PROJECTS.GET_PROJECT_LO_TABLE(chartId);
          if (response && !response.error && response.data) {
            setLoTableData(response.data.data ?? []);
            if (response.data.projectName) setProjectName(response.data.projectName);
          } else {
            setLoTableData([]);
          }
        } catch (error) {
          console.error("Error fetching learning objectives table:", error);
          setLoTableData([]);
        } finally {
          setIsLoadingTable(false);
        }
      } else {
        setLoTableData([]);
        setIsLoadingTable(false);
      }
    };
    fetchLOTable();
  }, [router.isReady, activeTab, chartId]);

  const buttonStyle: CSSProperties = {
    backgroundColor: "",
    height: "32px",
    fontSize: "14px",
  };

  const [phaseFilter, setPhaseFilter] = useState<{ id: number; name: string }>({
    id: 0,
    name: "All",
  });
  const [statusFilter, setStatusFilter] = useState<{ id: number; name: string }>({
    id: 0,
    name: "All",
  });

  const statusOptions = [
    { id: 0, name: "All" },
    { id: 1, name: "On Track" },
    { id: 2, name: "At Risk" },
    { id: 3, name: "Delayed" },
  ];

  const phaseOptions = (() => {
    const phases = new Set<string>();
    loTableData.forEach((row) => {
      row.currentPhases?.forEach((phase: any) => {
        const name = phase.groupName ?? phase.GroupName;
        if (name) phases.add(name);
      });
    });
    const options = [{ id: 0, name: "All" }];
    Array.from(phases).forEach((phaseName, index) => {
      options.push({ id: index + 1, name: phaseName });
    });
    return options;
  })();

  const filteredLoTableData = loTableData.filter((row) => {
    if (statusFilter.name !== "All" && row.status !== statusFilter.name) return false;
    if (phaseFilter.name !== "All") {
      const hasMatchingPhase = row.currentPhases?.some((phase: any) => {
        const name = phase.groupName ?? phase.GroupName;
        return name === phaseFilter.name;
      });
      if (!hasMatchingPhase) return false;
    }
    return true;
  });

  const handleLOClick = (item: ProjectStatusData) => {
    if (item.id && chartId) {
      router.push({
        pathname: `/tasks/${chartId}/board`,
        query: { loid: item.id, loName: item.name },
      });
    }
  };

  /** Parses backend LO table dates (d/M/yyyy). Returns null for "In Process", "-", etc. */
  const parseLoTableDate = (value: unknown): number | null => {
    if (value == null) return null;
    const s = String(value).trim();
    const parts = s.split("/").map((p) => p.trim());
    if (parts.length !== 3) return null;
    const day = Number(parts[0]);
    const month = Number(parts[1]);
    const year = Number(parts[2]);
    if (![day, month, year].every((n) => Number.isFinite(n))) return null;
    const t = new Date(year, month - 1, day).getTime();
    if (Number.isNaN(t)) return null;
    const check = new Date(year, month - 1, day);
    if (
      check.getFullYear() !== year ||
      check.getMonth() !== month - 1 ||
      check.getDate() !== day
    ) {
      return null;
    }
    return t;
  };

  const NON_DATE_SORT_RANK = Number.POSITIVE_INFINITY;

  const DateComparator = (v1: string, v2: string) => {
    const t1 = parseLoTableDate(v1);
    const t2 = parseLoTableDate(v2);
    const r1 = t1 === null ? NON_DATE_SORT_RANK : t1;
    const r2 = t2 === null ? NON_DATE_SORT_RANK : t2;
    if (r1 === NON_DATE_SORT_RANK && r2 === NON_DATE_SORT_RANK) {
      return String(v1).localeCompare(String(v2));
    }
    return r1 - r2;
  };

  const loTableColumns: GridColDef[] = [
    { field: "id", headerName: "ID", width: 80 },
    {
      field: "name",
      headerName: "Learning Objectives",
      flex: 1,
      minWidth: 180,
      renderCell: (params: GridRenderCellParams) => (
        <div
          onClick={(e) => {
            e.stopPropagation();
            handleLOClick(params.row);
          }}
          style={{
            cursor: "pointer",
            fontWeight: 500,
            textDecoration: "none",
          }}
          onMouseEnter={(e) => (e.currentTarget.style.color = "#2563eb")}
          onMouseLeave={(e) => (e.currentTarget.style.color = "")}
        >
          {params.value}
        </div>
      ),
    },
    { field: "subject", headerName: "Subject", width: 120 },
    { field: "startDate", headerName: "Start Date", width: 110,sortComparator:DateComparator },
    { field: "endDate", headerName: "End Date", width: 110,sortComparator:DateComparator },
    {
      field: "activeTasks",
      headerName: "Active Tasks",
      width: 100,
      align: "center",
      headerAlign: "center",
    },
    {
      field: "currentPhases",
      headerName: "Current Phase",
      width: 150,
      renderCell: (params: GridRenderCellParams) => (
        <CurrentPhaseDisplay phases={params.value} />
      ),
    },
    {
      field: "status",
      headerName: "Status",
      width: 110,
      renderCell: (params: GridRenderCellParams) => (
        <StatusBadge status={params.value} />
      ),
    },
    {
      field: "progress",
      headerName: "Progress",
      width: 90,
      align: "center",
      headerAlign: "center",
      renderCell: (params: GridRenderCellParams) => (
        <ProgressDonut percentage={params.value} />
      ),
    },
  ];

  return (
    <>
      <Head>
        <title>ATS - Project analytics</title>
      </Head>
      <div className="mx-10 w-full relative max-h-screen overflow-y-auto pr-4">
        <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
          <div className="flex gap-2 items-center">
            <button onClick={() => router.push("/tasks")}>
              <svg
                width="10"
                height="17"
                viewBox="0 0 10 17"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
              >
                <path
                  fillRule="evenodd"
                  clipRule="evenodd"
                  d="M2.41379 8.485L9.48479 15.556L8.07079 16.97L0.292786 9.192C0.105315 9.00447 0 8.75016 0 8.485C0 8.21984 0.105315 7.96553 0.292786 7.778L8.07079 0L9.48479 1.414L2.41379 8.485Z"
                  fill="black"
                />
              </svg>
            </button>
            <h1 className="font-bold text-2xl">{projectName}</h1>
          </div>
        </div>

        <div className="flex justify-between items-end h-20 ">
          <div className="flex gap-2 items-end h-11 mt-4 px-1">
            <Tab
              label="Overview"
              active={activeTab === "overview"}
              onClick={() => setActiveTab("overview")}
            />
            <Tab
              label="Learning Objectives"
              active={activeTab === "progress"}
              onClick={() => setActiveTab("progress")}
            />
          </div>
          {activeTab === "progress" && (
            <div className="flex items-end justify-end h-full gap-5">
              <LightDropdown
                value={phaseFilter}
                options={phaseOptions}
                onChange={setPhaseFilter}
                buttonStyle={buttonStyle}
              />
              <LightDropdown
                value={statusFilter}
                options={statusOptions}
                onChange={setStatusFilter}
                buttonStyle={buttonStyle}
              />
            </div>
          )}
        </div>

        <div className="mt-4">
          {activeTab === "overview" && <ProjectOverview />}
          {activeTab === "progress" && (
            <div className="bg-white p-6 rounded-lg shadow-sm">
              <h2 className="text-xl font-bold mb-4">Learning Objectives</h2>
              {isLoadingTable ? (
                <div className="flex items-center justify-center h-64">
                  <div className="text-gray-500">Loading data...</div>
                </div>
              ) : filteredLoTableData.length > 0 ? (
                <DataGrid
                  rows={filteredLoTableData}
                  columns={loTableColumns}
                  pageSizeOptions={[10, 25, 50]}
                  initialState={{
                    pagination: { paginationModel: { pageSize: 10 } },
                  }}
                  autoHeight
                  disableRowSelectionOnClick
                  getRowHeight={() => "auto"}
                  sx={{
                    "& .MuiDataGrid-cell": {
                      display: "flex",
                      alignItems: "center",
                    },
                  }}
                />
              ) : (
                <div className="flex items-center justify-center h-64">
                  <div className="text-gray-500">
                    {loTableData.length > 0
                      ? "No learning objectives match the selected filters"
                      : "No learning objectives data available"}
                  </div>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </>
  );
};

export default ProjectChartsPage;


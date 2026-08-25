import { useEffect, useState } from "react";
import { useRouter } from "next/router";
import API from "../../../../lib/API";
import Loader from "../../../../components/loader";
import { DataGrid, GridColDef, GridRenderCellParams } from "@mui/x-data-grid";
import ProjectStatusChart, {
  ProjectStatusData,
} from "../../../../components/charts/ProjectStatusChart";
import { LightDropdown } from "../../../../components/formComponents/LightDropdown";

const timePeriodOptions = [
  { id: 1, name: "Today" },
  { id: 2, name: "Last Week" },
  { id: 3, name: "Last Month" },
  { id: 4, name: "All Time" },
];

const EmptyState = ({ message }: { message: string }) => (
  <div className="flex flex-col items-center justify-center py-12 text-gray-400">
    <svg
      className="w-16 h-16 mb-4"
      fill="none"
      stroke="currentColor"
      viewBox="0 0 24 24"
      xmlns="http://www.w3.org/2000/svg"
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth={1.5}
        d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
      />
    </svg>
    <p className="text-sm">{message}</p>
  </div>
);

const ProjectTag = ({
  label,
  value,
  color,
  groupId,
  onClick,
  isFilled = false,
}: any & { onClick?: (tag: any) => void }) => {
  const [isHover, setIsHover] = useState(false);
  const effectiveFilled = isFilled || isHover;
  const pillStyle = {
    display: "inline-flex",
    alignItems: "center",
    padding: "6px 14px",
    borderRadius: "20px",
    border: `1px solid ${color}`,
    backgroundColor: effectiveFilled ? color : "#ffffff",
    color: effectiveFilled ? "#ffffff" : "#333333",
    fontFamily: "Segoe UI, Tahoma, Geneva, Verdana, sans-serif",
    fontSize: "14px",
    fontWeight: "500",
    minWidth: "fit-content",
    cursor: "pointer",
    opacity: effectiveFilled ? 1 : 0.9,
  } as const;

  const dotStyle = {
    width: "10px",
    height: "10px",
    borderRadius: "50%",
    backgroundColor: effectiveFilled ? "#ffffff" : color,
    marginRight: "8px",
  } as const;

  const valueStyle = {
    marginLeft: "10px",
    color: effectiveFilled ? "#ffffff" : color,
    opacity: effectiveFilled ? 1 : 0.6,
    fontWeight: "600",
  } as const;

  return (
    <div
      style={pillStyle}
      onMouseEnter={() => setIsHover(true)}
      onMouseLeave={() => setIsHover(false)}
      onClick={() => onClick && onClick({ label, value, color, groupId, isFilled })}
    >
      <div style={dotStyle} />
      <span>{label}</span>
      <span style={valueStyle}>{value}</span>
    </div>
  );
};

const TagsSection = ({
  tags,
  selectedGroup,
  selectedTimePeriod,
  onTimePeriodChange,
  onClick,
}: {
  tags: any[];
  selectedGroup?: number;
  selectedTimePeriod: { id: number; name: string };
  onTimePeriodChange: (value: { id: number; name: string }) => void;
  onClick?: (tag: any) => void;
}) => {
  const containerStyle = {
    display: "flex",
    width: "83%",
    flexWrap: "wrap" as const,
    gap: "10px",
    borderRadius: "8px",
  };

  return (
    <div className="">
      <div style={{ display: "flex", alignItems: "flex-start", gap: "20px" }}>
        {tags.length === 0 ? (
          <div style={containerStyle}>
            <EmptyState message="No in-progress tasks available for this time period" />
          </div>
        ) : (
          <div style={containerStyle}>
            {tags.map((tag, index) => (
              <ProjectTag
                key={index}
                label={tag.label}
                groupId={tag.groupId}
                value={tag.value}
                color={tag.color}
                isFilled={
                  selectedGroup !== undefined
                    ? tag.groupId === selectedGroup
                    : !!tag.isFilled
                }
                onClick={() => onClick && onClick(tag)}
              />
            ))}
          </div>
        )}
        <LightDropdown
          value={selectedTimePeriod}
          options={timePeriodOptions}
          onChange={onTimePeriodChange}
        />
      </div>
    </div>
  );
};

const TaskStatusBadge = ({ status, label }: { status: number; label: string }) => {
  const styles =
    status === 0
      ? "bg-white text-gray-800 border-gray-800"
      : status === 1
        ? "bg-blue-500 text-white border-blue-500"
        : "bg-amber-500 text-white border-amber-500";

  return (
    <span className={`px-3 py-1 rounded-full text-xs font-medium border ${styles}`}>
      {label}
    </span>
  );
};

type InProgressTask = {
  id: number;
  name: string;
  status: number;
  statusName: string;
  learningObjectiveId: number;
  learningObjectiveName: string;
  groupId: number;
  groupName: string;
  groupColor: string;
  assigneeName: string;
};

const ProjectLoProgress = () => {
  const router = useRouter();
  const { projectChartId } = router.query;
  const chartId =
    router.isReady && projectChartId
      ? Array.isArray(projectChartId)
        ? String(projectChartId[0])
        : String(projectChartId)
      : "";

  const [loading, setLoading] = useState(true);
  const [tags, setTags] = useState<any[]>([]);
  const [loProgressData, setLoProgressData] = useState<ProjectStatusData[]>([]);
  const [tasks, setTasks] = useState<InProgressTask[]>([]);
  const [selectedGroup, setSelectGroup] = useState<number>();
  const [isLoadingProgress, setIsLoadingProgress] = useState(false);
  const [isLoadingTasks, setIsLoadingTasks] = useState(false);
  const [selectedTimePeriod, setSelectedTimePeriod] = useState<{
    id: number;
    name: string;
  }>(timePeriodOptions[3]);

  useEffect(() => {
    if (!router.isReady || !chartId) {
      if (router.isReady) setLoading(false);
      return;
    }

    const fetchTags = async () => {
      try {
        setLoading(true);
        const overviewResponse = await API.PROJECTS.GET_ANALYTICS_OVERVIEW(
          chartId,
          selectedTimePeriod.id
        );
        if (overviewResponse && !overviewResponse.error && overviewResponse.data) {
          setTags(overviewResponse.data.learningActivitiesSummary?.tags || []);
        } else {
          setTags([]);
        }
      } catch (error) {
        console.error("Error fetching LO progress tags:", error);
        setTags([]);
      } finally {
        setLoading(false);
      }
    };

    fetchTags();
  }, [router.isReady, chartId, selectedTimePeriod]);

  useEffect(() => {
    if (!router.isReady || !chartId) return;

    const fetchLOProgress = async () => {
      try {
        setIsLoadingProgress(true);
        const response = await API.PROJECTS.GET_PROJECT_LO_PROGRESS(
          chartId,
          selectedTimePeriod.id,
          selectedGroup,
          true
        );
        if (response && !response.error && response.data) {
          setLoProgressData(response.data.data);
        } else {
          setLoProgressData([]);
        }
      } catch (error) {
        console.error("Error fetching learning objectives progress:", error);
        setLoProgressData([]);
      } finally {
        setIsLoadingProgress(false);
      }
    };

    fetchLOProgress();
  }, [router.isReady, chartId, selectedTimePeriod, selectedGroup]);

  useEffect(() => {
    if (!router.isReady || !chartId) return;

    const fetchTasks = async () => {
      try {
        setIsLoadingTasks(true);
        const response = await API.PROJECTS.GET_PROJECT_IN_PROGRESS_TASKS(
          chartId,
          selectedTimePeriod.id,
          selectedGroup
        );
        if (response && !response.error && response.data) {
          setTasks(response.data.data);
        } else {
          setTasks([]);
        }
      } catch (error) {
        console.error("Error fetching in-progress tasks:", error);
        setTasks([]);
      } finally {
        setIsLoadingTasks(false);
      }
    };

    fetchTasks();
  }, [router.isReady, chartId, selectedTimePeriod, selectedGroup]);

  const handleLOClick = (item: ProjectStatusData) => {
    if (item.id && chartId) {
      router.push({
        pathname: `/tasks/${chartId}/board`,
        query: { loid: item.id, loName: item.name },
      });
    }
  };

  const handleTagClick = (tag: any) => {
    if (selectedGroup === tag.groupId) setSelectGroup(undefined);
    else setSelectGroup(tag.groupId);
  };

  const openLoOnBoard = (loId: number, loName: string) => {
    if (!chartId || !loId) return;
    router.push({
      pathname: `/tasks/${chartId}/board`,
      query: { loid: loId, loName },
    });
  };

  const taskColumns: GridColDef[] = [
    { field: "id", headerName: "ID", width: 80 },
    {
      field: "name",
      headerName: "Task",
      flex: 1,
      minWidth: 180,
    },
    {
      field: "learningObjectiveName",
      headerName: "Learning Objective",
      flex: 1,
      minWidth: 180,
      renderCell: (params: GridRenderCellParams) => (
        <div
          onClick={(e) => {
            e.stopPropagation();
            openLoOnBoard(params.row.learningObjectiveId, params.value);
          }}
          style={{ cursor: "pointer", fontWeight: 500 }}
          onMouseEnter={(e) => (e.currentTarget.style.color = "#2563eb")}
          onMouseLeave={(e) => (e.currentTarget.style.color = "")}
        >
          {params.value}
        </div>
      ),
    },
    {
      field: "statusName",
      headerName: "Status",
      width: 120,
      renderCell: (params: GridRenderCellParams) => (
        <TaskStatusBadge status={params.row.status} label={params.value} />
      ),
    },
    {
      field: "groupName",
      headerName: "Group",
      width: 150,
      renderCell: (params: GridRenderCellParams) => (
        <div className="flex items-center gap-2">
          <span
            className="w-2.5 h-2.5 rounded-full"
            style={{ backgroundColor: params.row.groupColor }}
          />
          <span>{params.value}</span>
        </div>
      ),
    },
    {
      field: "assigneeName",
      headerName: "Assignee",
      width: 150,
      renderCell: (params: GridRenderCellParams) => params.value || "-",
    },
  ];

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <Loader />
      </div>
    );
  }

  return (
    <div>
      <div className="mb-6">
        <TagsSection
          tags={tags}
          selectedGroup={selectedGroup}
          selectedTimePeriod={selectedTimePeriod}
          onTimePeriodChange={setSelectedTimePeriod}
          onClick={handleTagClick}
        />
      </div>

      <div className="mb-6">
        <div className="bg-white p-6 rounded-lg shadow-sm">
          <h2 className="text-xl font-bold mb-4">Learning Objectives Progress</h2>
          <p className="text-sm text-gray-500 mb-4">
            Learning objectives with tasks still in Backlog, To Do, or Doing
            {selectedGroup !== undefined ? " for the selected group" : ""}.
          </p>
          {isLoadingProgress ? (
            <div className="flex items-center justify-center h-64">
              <div className="text-gray-500">Loading progress data...</div>
            </div>
          ) : loProgressData.length > 0 ? (
            <ProjectStatusChart data={loProgressData} onBarClick={handleLOClick} />
          ) : (
            <div className="flex items-center justify-center h-64">
              <div className="text-gray-500">
                No in-progress learning objectives for the selected filters
              </div>
            </div>
          )}
        </div>
      </div>

      <div className="mb-6">
        <div className="bg-white p-6 rounded-lg shadow-sm">
          <h2 className="text-xl font-bold mb-4">In-progress tasks</h2>
          <p className="text-sm text-gray-500 mb-4">
            Tasks in Backlog, To Do, or Doing
            {selectedGroup !== undefined ? " for the selected group" : ""}.
          </p>
          {isLoadingTasks ? (
            <div className="flex items-center justify-center h-64">
              <div className="text-gray-500">Loading tasks...</div>
            </div>
          ) : tasks.length > 0 ? (
            <DataGrid
              rows={tasks}
              columns={taskColumns}
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
                No in-progress tasks for the selected filters
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ProjectLoProgress;

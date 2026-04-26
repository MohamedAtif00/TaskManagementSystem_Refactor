import { useEffect, useState } from "react";
import { useRouter } from "next/router";
import API from "../../../../lib/API";
import Loader from "../../../../components/loader";
import { PieChart } from "@mui/x-charts/PieChart";
import { Box, Typography } from "@mui/material";
import ProjectStatusChart, {
  ProjectStatusData,
} from "../../../../components/charts/ProjectStatusChart";
import { LightDropdown } from "../../../../components/formComponents/LightDropdown";

// Reuse the same time period filter UX as sprint
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

const DonutChart = ({
  data,
  total,
  title,
  subTitle,
  percientage,
  showLegends = true,
  showEmptyState = false,
  emptyStateMessage = "No data available",
  topPosition,
}: {
  data: { label: string; value: number; color: string }[];
  total: number;
  title: string;
  subTitle?: string;
  percientage?: boolean;
  showLegends?: boolean;
  showEmptyState?: boolean;
  emptyStateMessage?: string;
  topPosition?: number | string;
}) => {
  const verticalCenter = topPosition || "50%";
  const dataWithValues = data.map((item) => ({
    ...item,
    label: ` ${item.value} ${item.label}`,
  }));

  if (showEmptyState) {
    return (
      <div className="bg-white p-6 rounded-lg shadow-sm">
        <h2 className="text-xl font-bold mb-4">{title}</h2>
        <EmptyState message={emptyStateMessage} />
      </div>
    );
  }

  return (
    <div className="bg-white  p-10 pb-2 rounded-lg shadow-sm my-5">
      <h2 className="text-l font-bold mb-4">{title}</h2>
      <Box className="relative flex items-center justify-center">
        <PieChart
          series={[
            {
              innerRadius: 70,
              outerRadius: 90,
              data: dataWithValues,
              cx: "50%",
              cy: "50%",
              startAngle: -180,
              endAngle: 180,
            },
          ]}
          hideLegend={!showLegends}
          width={300}
          height={300}
          slotProps={{
            tooltip: { trigger: "item" },
            legend: {
              direction: "horizontal",
              position: { vertical: "bottom", horizontal: "center" },
            },
          }}
          margin={{ top: 20, bottom: 80, left: 20, right: 20 }}
        />
        <Box
          className="absolute inset-0 flex flex-col items-center justify-center"
          sx={{
            pointerEvents: "none",
            left: "50%",
            top: verticalCenter,
            transform: "translate(-50%, -50%)",
          }}
        >
          <Typography
            sx={{ fontSize: "26px", fontWeight: "bold", lineHeight: 1 }}
          >
            {total}
            {percientage ? "%" : ""}
          </Typography>
          <Typography sx={{ fontSize: "14px", color: "#666", marginTop: "1px" }}>
            {subTitle}
          </Typography>
        </Box>
      </Box>
    </div>
  );
};

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
            <EmptyState message="No learning objectives available for this time period" />
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

const ProjectOverview = () => {
  const router = useRouter();
  const { projectChartId } = router.query;
  const [loading, setLoading] = useState(true);
  const [overviewData, setOverviewData] = useState<any | null>(null);
  const [loProgressData, setLoProgressData] = useState<ProjectStatusData[]>([]);
  const [selectedGroup, setSelectGroup] = useState<number>();
  const [isLoadingProgress, setIsLoadingProgress] = useState(false);
  const [selectedTimePeriod, setSelectedTimePeriod] = useState<{
    id: number;
    name: string;
  }>(timePeriodOptions[3]);

  useEffect(() => {
    if (!router.isReady) return;
    if (!projectChartId) {
      setLoading(false);
      return;
    }

    const fetchProjectData = async () => {
      try {
        setLoading(true);
        const overviewResponse = await API.PROJECTS.GET_ANALYTICS_OVERVIEW(
          projectChartId as string | string[],
          selectedTimePeriod.id
        );

        if (overviewResponse && !overviewResponse.error && overviewResponse.data) {
          setOverviewData(overviewResponse.data);
        } else {
          setOverviewData(null);
        }
      } catch (error) {
        console.error("Error fetching project overview data:", error);
        setOverviewData(null);
      } finally {
        setLoading(false);
      }
    };

    fetchProjectData();
  }, [router.isReady, projectChartId, selectedTimePeriod]);

  useEffect(() => {
    if (!router.isReady || !projectChartId) return;

    const fetchLOProgress = async () => {
      try {
        setIsLoadingProgress(true);
        const response = await API.PROJECTS.GET_PROJECT_LO_PROGRESS(
          projectChartId,
          selectedTimePeriod.id,
          selectedGroup
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
  }, [router.isReady, projectChartId, selectedTimePeriod, selectedGroup]);

  if (loading) {
    return (
      <div className="flex items-center justify-center h-screen">
        <Loader />
      </div>
    );
  }

  if (!overviewData) {
    return (
      <div className="flex items-center justify-center h-screen">
        <p className="text-gray-500">No data available</p>
      </div>
    );
  }

  const progressPercent = overviewData.projectProgress?.progressPercent ?? 0;

  const projectChartData = [
    { label: "Done", value: progressPercent, color: "#22C55E" },
    { label: "Remaining", value: 100 - progressPercent, color: "#E9FAEF" },
  ].filter((item: any) => item.value > 0);

  const tasksSummary = overviewData.tasksSummary || {};
  const taskChartData = [
    { label: "Active", value: tasksSummary.active || 0, color: "#3b82f6" },
    { label: "Completed", value: tasksSummary.completed || 0, color: "#10b981" },
    { label: "Rollback", value: tasksSummary.rollback || 0, color: "#f59e0b" },
    { label: "Flagged", value: tasksSummary.flagged || 0, color: "#ef4444" },
    { label: "Not Started", value: tasksSummary.notStarted || 0, color: "#9ca3af" },
  ].filter((item: any) => item.value > 0);

  const loSummary = overviewData.learningActivitiesSummary?.loSummary || {};
  const loChartData = [
    { label: "Completed", value: loSummary.completed || 0, color: "#10b981" },
    { label: "In Process", value: loSummary.inProcess || 0, color: "#f59e0b" },
    { label: "Not Started", value: loSummary.notStarted || 0, color: "#9ca3af" },
  ].filter((item: any) => item.value > 0);

  const chartId =
    router.isReady && projectChartId
      ? Array.isArray(projectChartId)
        ? String(projectChartId[0])
        : String(projectChartId)
      : "";

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

  const unitsCount = overviewData.numberOfUnits ?? 0;
  const lessonsCount = overviewData.numberOfLessons ?? 0;
  const loCount = overviewData.numberOfLearningObjectives ?? 0;
  const statNumberClass = "text-4xl font-semibold mt-2 text-sky-400";

  return (
    <div>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <div className="bg-white rounded-lg shadow-sm border border-gray-100 p-5">
          <div className="text-sm font-bold text-gray-900">Number of Units</div>
          <div className={statNumberClass}>{unitsCount}</div>
        </div>
        <div className="bg-white rounded-lg shadow-sm border border-gray-100 p-5">
          <div className="text-sm font-bold text-gray-900">Number of Lessons</div>
          <div className={statNumberClass}>{lessonsCount}</div>
        </div>
        <div className="bg-white rounded-lg shadow-sm border border-gray-100 p-5">
          <div className="text-sm font-bold text-gray-900">
            Number of Learning Objectives
          </div>
          <div className={statNumberClass}>{loCount}</div>
        </div>
      </div>

      <div className="grid grid-cols-2 md:grid-cols-3 gap-6">
        <DonutChart
          data={projectChartData}
          total={progressPercent}
          title="Project progress"
          percientage={true}
          showLegends={false}
          topPosition={"40%"}
        />

        <DonutChart
          data={
            loChartData.length > 0
              ? loChartData
              : [
                  {
                    label: "Total",
                    value: loSummary.total || 0,
                    color: "#3b82f6",
                  },
                ]
          }
          total={loSummary.total || 0}
          title="Learning Objectives Summary"
          subTitle="Total LOs"
          showEmptyState={(loSummary.total || 0) === 0}
          emptyStateMessage="No learning objectives in this project"
          topPosition={"35%"}
        />

        <DonutChart
          data={taskChartData}
          total={tasksSummary.total || 0}
          title="Tasks Summary"
          subTitle="Total Tasks"
          topPosition={"31%"}
        />
      </div>

      <div className="mb-6">
        <TagsSection
          tags={overviewData.learningActivitiesSummary?.tags || []}
          selectedGroup={selectedGroup}
          selectedTimePeriod={selectedTimePeriod}
          onTimePeriodChange={setSelectedTimePeriod}
          onClick={handleTagClick}
        />
      </div>

      <div className="mb-6">
        <div className="bg-white p-6 rounded-lg shadow-sm">
          <h2 className="text-xl font-bold mb-4">Learning Objectives Progress</h2>
          {isLoadingProgress ? (
            <div className="flex items-center justify-center h-64">
              <div className="text-gray-500">Loading progress data...</div>
            </div>
          ) : loProgressData.length > 0 ? (
            <ProjectStatusChart data={loProgressData} onBarClick={handleLOClick} />
          ) : (
            <div className="flex items-center justify-center h-64">
              <div className="text-gray-500">
                No learning objectives progress data available
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ProjectOverview;


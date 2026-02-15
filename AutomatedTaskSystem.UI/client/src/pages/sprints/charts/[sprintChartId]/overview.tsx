import { useEffect, useRef, useState } from "react";
import { useRouter } from "next/router";
import API from "../../../../lib/API";
import Loader from "../../../../components/loader";
import { PieChart } from '@mui/x-charts/PieChart';
import { Box, Typography } from "@mui/material";
import { TagData, SprintOverviewData } from "../../../../lib/API/Sprints.d";
import ProjectStatusChart, { ProjectStatusData } from "../../../../components/charts/ProjectStatusChart";
import { LightDropdown } from "../../../../components/formComponents/LightDropdown";


// Individual Tag/Pill Component
const ProjectTag = ({ label, value, color, isFilled = false }: TagData) => {
  const pillStyle = {
    display: 'inline-flex',
    alignItems: 'center',
    padding: '6px 14px',
    borderRadius: '20px',
    border: `1px solid ${color}`,
    backgroundColor: isFilled ? color : '#ffffff',
    color: isFilled ? '#ffffff' : '#333333',
    fontFamily: 'Segoe UI, Tahoma, Geneva, Verdana, sans-serif',
    fontSize: '14px',
    fontWeight: '500',
    minWidth: 'fit-content'
  };

  const dotStyle = {
    width: '10px',
    height: '10px',
    borderRadius: '50%',
    backgroundColor: isFilled ? '#ffffff' : color,
    marginRight: '8px'
  };

  const valueStyle = {
    marginLeft: '10px',
    color: isFilled ? '#ffffff' : color,
    opacity: isFilled ? 1 : 0.6,
    fontWeight: '600'
  };

  return (
    <div style={pillStyle}>
      <div style={dotStyle} />
      <span>{label}</span>
      <span style={valueStyle}>{value}</span>
    </div>
  );
};

// Empty State Component
const EmptyState = ({ message }: { message: string }) => {
  return (
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
};

// Time period options for filtering
const timePeriodOptions = [
  { id: 1, name: 'Today' },
  { id: 2, name: 'Last Week' },
  { id: 3, name: 'Last Month' },
  { id: 4, name: 'All Time' }
];

// Tags Section Component
interface TagsSectionProps {
  tags: TagData[];
  selectedTimePeriod: { id: number; name: string };
  onTimePeriodChange: (value: { id: number; name: string }) => void;
}

const TagsSection = ({ tags, selectedTimePeriod, onTimePeriodChange }: TagsSectionProps) => {
  const containerStyle = {
    display: 'flex',
    width:'83%',
    flexWrap: 'wrap' as const,
    gap: '10px',
    padding: '',
    // backgroundColor: '#f0f2f5',
    borderRadius: '8px'
  };

  return (
    <div className="">
      {tags.length === 0 ? (
        <EmptyState message="No learning objectives available" />
      ) : (
        <div style={{ display: 'flex', alignItems: 'flex-start', gap: '20px' }}>
          <div style={containerStyle}>
            {tags.map((tag, index) => (
              <ProjectTag
                key={index}
                label={tag.label}
                value={tag.value}
                color={tag.color}
                isFilled={tag.isFilled}
              />
            ))}
          </div>
          <LightDropdown
            value={selectedTimePeriod}
            options={timePeriodOptions}
            onChange={onTimePeriodChange}
          />
        </div>
      )}
    </div>
  );
};

// Donut Chart Component
const DonutChart = ({
  data,
  total,
  title,
  subTitle,
  percientage,
  showLegends = true,
  showEmptyState = false,
  emptyStateMessage = "No data available",
  topPosition
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
  const verticalCenter = topPosition || '50%';

  const dataWithValues = data.map(item => ({
    ...item,
    label: ` ${item.value} ${item.label}` // Format: "Completed: 10"
  }));
  // Show empty state if requested
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
      <Box
        className="relative flex items-center justify-center"
        // sx={{ width: 300, height: 300, margin: 'auto' }}
      >
        <PieChart
          series={[
            {
              innerRadius: 70,
              outerRadius: 90,
              data: dataWithValues,
              cx: '50%',
              cy: '50%',
              // paddingAngle: 0,
              startAngle: -180,
              endAngle: 180,
            },
          ]}
          hideLegend={!showLegends}
          width={300}
          height={300}
          slotProps={{
            tooltip: { trigger: 'item' },
            legend: {
              direction: 'horizontal',
              position: { vertical: 'bottom', horizontal: 'center' },
            }
          }}
          margin={{ top: 20, bottom: 80, left: 20, right: 20 }}
        />
        <Box
          className="absolute inset-0 flex flex-col items-center justify-center"
          sx={{
            pointerEvents: 'none',
            left: '50%',
            top: verticalCenter,
            transform: 'translate(-50%, -50%)',
          }}
        >
          <Typography sx={{ fontSize: '26px', fontWeight: 'bold', lineHeight: 1 }}>
            {total}{percientage ? '%' : ''}
          </Typography>
          <Typography sx={{ fontSize: '14px', color: '#666', marginTop: '1px' }}>
            {subTitle}
          </Typography>
        </Box>
      </Box>
    </div>
  );
};

// Main Overview Component
// All data is fetched from the backend API - no frontend calculations
const SprintOverview = () => {
  const router = useRouter();
  const { sprintChartId } = router.query;
  const [loading, setLoading] = useState(true);
  const [overviewData, setOverviewData] = useState<SprintOverviewData | null>(null);
  const [loProgressData, setLoProgressData] = useState<ProjectStatusData[]>([]);
  const [isLoadingProgress, setIsLoadingProgress] = useState(false);
  const [selectedTimePeriod, setSelectedTimePeriod] = useState<{ id: number; name: string }>(timePeriodOptions[3]);

  useEffect(() => {

    if (!router.isReady) return;

    if (!sprintChartId) {
      setLoading(false);
      return;
    }

    const fetchSprintData = async () => {
      try {
        setLoading(true);

        // Fetch pre-calculated analytics from the backend
        // All data analysis and calculations are performed by the backend API
        // Pass the selected time period ID to filter the data
        const overviewResponse = await API.SPRINTS.GET_SPRINT_OVERVIEW(sprintChartId, selectedTimePeriod.id);

        if (overviewResponse && !overviewResponse.error && overviewResponse.data) {
          setOverviewData(overviewResponse.data);
        } else {
          // Handle case where there is an API error - set empty data structure
          const emptyTaskSummary = {
            active: 0,
            completed: 0,
            rollback: 0,
            flagged: 0,
            notStarted: 0,
            total: 0
          };
          setOverviewData({
            tags: [],
            taskSummary: emptyTaskSummary,
            loSummary: {
              completed: 0,
              notStarted: 0,
              inProcess: 0,
              total: 0
            },
            sprintSummary: emptyTaskSummary
          });
        }
      } catch (error) {
        console.error("Error fetching sprint overview data:", error);
        // Set empty data on error so UI doesn't stay in loading state
        const emptyTaskSummary = {
          active: 0,
          completed: 0,
          rollback: 0,
          flagged: 0,
          notStarted: 0,
          total: 0
        };
        setOverviewData({
          tags: [],
          taskSummary: emptyTaskSummary,
          loSummary: {
            completed: 0,
            notStarted: 0,
            inProcess:0,
            total: 0
          },
          sprintSummary: emptyTaskSummary
        });
      } finally {
        setLoading(false);
      }
    };

    fetchSprintData();
  }, [router.isReady, sprintChartId, selectedTimePeriod]);

  // Fetch learning objectives progress data for the progress chart
  useEffect(() => {
    if (!router.isReady || !sprintChartId) return;

    const fetchLOProgress = async () => {
      try {
        setIsLoadingProgress(true);
        // Fetch pre-calculated LO progress analytics from the backend
        const response = await API.SPRINTS.GET_SPRINT_LO_PROGRESS(sprintChartId);

        if (response && !response.error && response.data) {
          // Backend returns LearningObjectivesProgressData with 'data' property containing the chart data
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
  }, [router.isReady, sprintChartId]);

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

  // Calculate sprint progress percentage: (completed / total) * 100
  const sprintProgressPercentage = overviewData.sprintSummary.total > 0
    ? Math.round((overviewData.sprintSummary.completed / overviewData.sprintSummary.total) * 100)
    : 0;

  // Format backend data for chart display (adding colors for visualization)
  // Note: This is display formatting only, not data analysis or calculations
  // Sprint progress chart uses percentage-based values for the donut visualization
  const sprintChartData = [
    { label: 'Done', value: sprintProgressPercentage, color: '#22C55E' },
    { label: 'Remaining', value: 100 - sprintProgressPercentage, color: '#E9FAEF' },
  ].filter(item => item.value > 0);

  const taskChartData = [
    { label: 'Active', value: overviewData.taskSummary.active, color: '#3b82f6' },
    { label: 'Completed', value: overviewData.taskSummary.completed, color: '#10b981' },
    { label: 'Rollback', value: overviewData.taskSummary.rollback, color: '#f59e0b' },
    { label: 'Flagged', value: overviewData.taskSummary.flagged, color: '#ef4444' },
    { label: 'Not Started', value: overviewData.taskSummary.notStarted, color: '#9ca3af' },
  ].filter(item => item.value > 0);

  const loChartData = [
    { label: 'Completed', value: overviewData.loSummary.completed, color: '#10b981' },
    { label: 'In Process', value: overviewData.loSummary.inProcess, color: '#f59e0b' },
    { label: 'Not Started', value: overviewData.loSummary.notStarted, color: '#9ca3af' },
  ].filter(item => item.value > 0);

  return (
    <div>
      {/* Charts Section - All data comes from backend API */}
      <div className="grid grid-cols-2 md:grid-cols-3 gap-6">
        {/* Sprint Chart - displays completion percentage */}
        <DonutChart
          data={sprintChartData}
          total={sprintProgressPercentage}
          title="Sprint progress"
          percientage={true}
          showLegends={false}
          topPosition={'40%'}
        />

        {/* Learning Objectives Summary Chart */}
        <DonutChart
          data={loChartData.length > 0 ? loChartData : [{ label: 'Total', value: overviewData.loSummary.total, color: '#3b82f6' }]}
          total={overviewData.loSummary.total}
          title="Learning Objectives Summary"
          subTitle="Total LOs"
          showEmptyState={overviewData.loSummary.total === 0}
          emptyStateMessage="No learning objectives in this sprint"
          topPosition={'35%'}

        />

        {/* Task Summary Chart */}
        <DonutChart
          data={taskChartData}
          total={overviewData.taskSummary.total}
          title="Tasks Summary"
          subTitle="Total Tasks"
          topPosition={'31%'}
        />
      </div>

      {/* Tags Section - Data comes from backend API */}
      <div className="mb-6">
        <TagsSection
          tags={overviewData.tags}
          selectedTimePeriod={selectedTimePeriod}
          onTimePeriodChange={setSelectedTimePeriod}
        />
      </div>

      {/* Learning Objectives Progress Chart */}
      <div className="mb-6">
        <div className="bg-white p-6 rounded-lg shadow-sm">
          <h2 className="text-xl font-bold mb-4">Learning Objectives Progress</h2>
          {isLoadingProgress ? (
            <div className="flex items-center justify-center h-64">
              <div className="text-gray-500">Loading progress data...</div>
            </div>
          ) : loProgressData.length > 0 ? (
            <ProjectStatusChart data={loProgressData} />
          ) : (
            <div className="flex items-center justify-center h-64">
              <div className="text-gray-500">No learning objectives progress data available</div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default SprintOverview;


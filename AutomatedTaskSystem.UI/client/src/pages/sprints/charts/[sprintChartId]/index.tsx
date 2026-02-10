import { useState, useEffect } from "react";
import { useRouter } from "next/router";
import Head from "next/head";
import SprintOverview from './overview';
import Tab from '../../../../components/Tab/Tab';
import API from '../../../../lib/API';
import { LearningObjectiveTableRow } from '../../../../lib/API/sprints';
import { DataGrid, GridColDef, GridRenderCellParams } from "@mui/x-data-grid";
import { PieChart } from "@mui/x-charts/PieChart";
import { Box } from "@mui/material";

type ChartView = 'overview' | 'progress';

// Status badge component for the table
const StatusBadge = ({ status }: { status: string }) => {
  const getStatusStyles = (s: string) => {
    switch (s) {
      case 'On Track': return 'bg-green-500 text-white';
      case 'At Risk': return 'bg-yellow-400 text-white';
      case 'Delayed': return 'bg-red-400 text-white';
      default: return 'bg-gray-400 text-white';
    }
  };
  return (
    <span className={`px-3 py-1 rounded-md text-sm font-medium ${getStatusStyles(status)}`}>
      {status}
    </span>
  );
};

// Progress donut chart component for the table
const ProgressDonut = ({ percentage }: { percentage: number }) => {
  const getStatusColor = (perc: number) => {
    if (perc >= 75) return '#22c55e';
    if (perc >= 50) return '#fbbf24';
    return '#ef4444';
  };
  const progressData = [
    { label: 'Done', value: percentage, color: getStatusColor(percentage) },
    { label: 'Pending', value: 100 - percentage, color: '#e5e7eb' },
  ];
  return (
    <Box className="relative flex items-center justify-center" sx={{ width: 50, height: 50 }}>
      <PieChart
        series={[{ innerRadius: 10, outerRadius: 18, data: progressData, cx: '50%', cy: '50%', startAngle: -130, endAngle: 230, paddingAngle: 0 }]}
        width={50} height={50}
        // slotProps={}
        margin={{ top: 0, bottom: 0, left: 0, right: 0 }}
      />
      <Box sx={{ position: 'absolute', top: '50%', left: '50%', transform: 'translate(-50%, -50%)', fontSize: '10px', fontWeight: 'bold', color: getStatusColor(percentage) }}>
        {percentage}%
      </Box>
    </Box>
  );
};

// Current phase display component
const CurrentPhaseDisplay = ({ phases }: { phases: { groupName: string; colorCode: string }[] }) => {
  if (!phases || phases.length === 0) return <span className="text-gray-400">-</span>;
  return (
    <div className="flex flex-col gap-1">
      {phases.map((phase, index) => (
        <div key={index} className="flex items-center gap-1">
          <span className="w-2 h-2 rounded-full" style={{ backgroundColor: phase.colorCode }} />
          <span className="text-sm">{phase.groupName}</span>
        </div>
      ))}
    </div>
  );
};

const SprintChartsPage = () => {
  const router = useRouter();
  const { sprintChartId } = router.query;
  const [activeTab, setActiveTab] = useState<ChartView>('overview');
  const [loTableData, setLoTableData] = useState<LearningObjectiveTableRow[]>([]);
  const [isLoadingTable, setIsLoadingTable] = useState(false);

  useEffect(() => {
    const fetchLOTable = async () => {
      if (activeTab === 'progress' && sprintChartId) {
        try {
          setIsLoadingTable(true);
          const response = await API.SPRINTS.GET_SPRINT_LO_TABLE(sprintChartId);
          if (response && !response.error && response.data) {
            setLoTableData(response.data.data);
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
  }, [activeTab, sprintChartId]);

  const loTableColumns: GridColDef[] = [
    { field: 'id', headerName: 'ID', width: 80 },
    { field: 'name', headerName: 'Learning Objectives', flex: 1, minWidth: 180 },
    { field: 'subject', headerName: 'Subject', width: 120 },
    { field: 'startDate', headerName: 'Start Date', width: 110 },
    { field: 'activeTasks', headerName: 'Active Tasks', width: 100, align: 'center', headerAlign: 'center' },
    { field: 'currentPhases', headerName: 'Current Phase', width: 150, renderCell: (params: GridRenderCellParams) => <CurrentPhaseDisplay phases={params.value} /> },
    { field: 'status', headerName: 'Status', width: 110, renderCell: (params: GridRenderCellParams) => <StatusBadge status={params.value} /> },
    { field: 'progress', headerName: 'Progress', width: 90, align: 'center', headerAlign: 'center', renderCell: (params: GridRenderCellParams) => <ProgressDonut percentage={params.value} /> },
  ];

  return (
    <>
      <Head><title>ATS - Sprint Charts</title></Head>
      <div className="mx-10 w-full relative max-h-screen overflow-y-auto pr-4">
        <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
          <div className="flex gap-2 items-center">
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
            </svg>
            <h1 className="font-bold text-2xl">Sprint Charts</h1>
          </div>
          <button onClick={() => router.push('/sprints')} className="px-4 py-1 rounded bg-gray-600 text-white hover:bg-gray-700 transition-colors">Back to Sprints</button>
        </div>
        <div className="flex gap-2 items-end h-9 mt-4 px-1">
          <Tab label="Overview" active={activeTab === 'overview'} onClick={() => setActiveTab('overview')} />
          <Tab label="Learning Objectives" active={activeTab === 'progress'} onClick={() => setActiveTab('progress')} />
        </div>
        <div className="mt-4">
          {activeTab === 'overview' && <SprintOverview />}
          {activeTab === 'progress' && (
            <div className="bg-white p-6 rounded-lg shadow-sm">
              <h2 className="text-xl font-bold mb-4">Learning Objectives</h2>
              {isLoadingTable ? (
                <div className="flex items-center justify-center h-64"><div className="text-gray-500">Loading data...</div></div>
              ) : loTableData.length > 0 ? (
                <DataGrid rows={loTableData} columns={loTableColumns} pageSizeOptions={[10, 25, 50]} initialState={{ pagination: { paginationModel: { pageSize: 10 } } }} autoHeight disableRowSelectionOnClick getRowHeight={() => 'auto'} sx={{ '& .MuiDataGrid-cell': { display: 'flex', alignItems: 'center' } }} />
              ) : (
                <div className="flex items-center justify-center h-64"><div className="text-gray-500">No learning objectives data available</div></div>
              )}
            </div>
          )}
        </div>
      </div>
    </>
  );
};

export default SprintChartsPage;


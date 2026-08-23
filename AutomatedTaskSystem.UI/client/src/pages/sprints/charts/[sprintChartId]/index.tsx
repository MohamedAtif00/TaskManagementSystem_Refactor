import { useState, useEffect, CSSProperties } from "react";
import { useRouter } from "next/router";
import Head from "next/head";
import SprintOverview from './overview';
import API from '../../../../lib/API';
import { LearningObjectiveTableRow } from '../../../../lib/API/sprints';
import { DataGrid, GridColDef, GridRenderCellParams } from "@mui/x-data-grid";
import { PieChart } from "@mui/x-charts/PieChart";
import { Box } from "@mui/material";
import { LightDropdown } from "../../../../components/formComponents/LightDropdown";
import { ProjectStatusData } from "../../../../components/charts/ProjectStatusChart";

type ChartView = 'overview' | 'progress';

// Local Tab component with the same design
const Tab = ({ label, active, onClick }: { label: string; active: boolean; onClick: () => void }) => {
  return (
    <button
      onClick={onClick}
      className={`
        relative px-4 py-2 text-sm font-medium transition-colors
        ${active ? 'text-blue-600' : 'text-gray-400 hover:text-gray-600'}
      `}
    >
      {label}
      {active && (
        <div className="absolute bottom-0 left-0 right-0 h-0.5 bg-blue-600" />
      )}
    </button>
  );
};

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
        margin={{ top: 0, bottom: 0, left: 0, right: 0 }}
        hideLegend= {true}
      />
      <Box sx={{ position: 'absolute', top: '50%', left: '50%', transform: 'translate(-50%, -50%)', fontSize: '10px', fontWeight: 'bold' }}>
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
  const [sprintName, setSprintName] = useState('');

  // Fetch sprint name on initial load
  useEffect(() => {
    const fetchSprintName = async () => {
      if (sprintChartId && !sprintName) {
        try {
          const response = await API.SPRINTS.GET_SPRINT_LO_TABLE(sprintChartId);
          if (response && !response.error && response.data) {
            setSprintName(response.data.sprintName || '');
          }
        } catch (error) {
          console.error("Error fetching sprint name:", error);
        }
      }
    };
    fetchSprintName();
  }, [sprintChartId]);

  useEffect(() => {
    const fetchLOTable = async () => {
      if (activeTab === 'progress' && sprintChartId) {
        try {
          setIsLoadingTable(true);
          const response = await API.SPRINTS.GET_SPRINT_LO_TABLE(sprintChartId);
          if (response && !response.error && response.data) {
            setLoTableData(response.data.data);
            setSprintName(response.data.sprintName || '');
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

  const buttonStyle:CSSProperties = {
    backgroundColor: '',
    height:'32px',
    fontSize:'14px'
  };

  const [phaseFilter, setPhaseFilter] = useState<{ id: number; name: string }>({ id: 0, name: 'All' });
  const [statusFilter, setStatusFilter] = useState<{ id: number; name: string }>({ id: 0, name: 'All' });

  // Status options for the dropdown
  const statusOptions = [
    { id: 0, name: 'All' },
    { id: 1, name: 'On Track' },
    { id: 2, name: 'At Risk' },
    { id: 3, name: 'Delayed' }
  ];

  // Derive unique phase options from the loaded data
  const phaseOptions = (() => {
    const phases = new Set<string>();
    loTableData.forEach(row => {
      row.currentPhases?.forEach(phase => {
        if (phase.groupName) {
          phases.add(phase.groupName);
        }
      });
    });
    const options = [{ id: 0, name: 'All' }];
    Array.from(phases).forEach((phaseName, index) => {
      options.push({ id: index + 1, name: phaseName });
    });
    return options;
  })();

  const handlePhaseFilterChange = (value: { id: number; name: string }) => {
    setPhaseFilter(value);
  };

  const handleStatusFilterChange = (value: { id: number; name: string }) => {
    setStatusFilter(value);
  };

  // Apply filters to the table data
  const filteredLoTableData = loTableData.filter(row => {
    // Filter by status
    if (statusFilter.name !== 'All' && row.status !== statusFilter.name) {
      return false;
    }
    // Filter by phase (check if any of the row's currentPhases match the selected phase)
    if (phaseFilter.name !== 'All') {
      const hasMatchingPhase = row.currentPhases?.some(phase => phase.groupName === phaseFilter.name);
      if (!hasMatchingPhase) {
        return false;
      }
    }
    return true;
  });

  const handleLOClick = (item: ProjectStatusData) => {
      if (item.id && sprintChartId) {
        router.push({
          pathname: `/tasks/sprint/${sprintChartId}/board`,
          query: { loid: item.id, loName: item.name }
        });
      }
    };



  const loTableColumns: GridColDef[] = [
    { field: 'id', headerName: 'ID', width: 80 },
    { 
  field: 'name', 
  headerName: 'Learning Objectives', 
  flex: 1, 
  minWidth: 180,
  renderCell: (params: GridRenderCellParams) => (
    <div
      onClick={(e) => {
        e.stopPropagation(); // Prevents triggering row-level clicks
        console.log("Clicked ID:", params.row.id);
        handleLOClick(params.row);
        // Add your navigation or modal logic here:
        // navigate(`/lo/${params.row.id}`);
      }}
      style={{ 
        // color: '#2563eb', 
        cursor: 'pointer',
        fontWeight: 500,
        textDecoration: 'none',
      }}
      onMouseEnter={(e) => (e.currentTarget.style.color = '#2563eb')}
      onMouseLeave={(e) => (e.currentTarget.style.color = '')}
    >
      {params.value}
    </div>
  )
},
    { field: 'subject', headerName: 'Subject', width: 120 },
    { field: 'startDate', headerName: 'Start Date', width: 110 },
    { field: 'endDate', headerName: 'End Date', width: 110 },
    { field: 'activeTasks', headerName: 'Active Tasks', width: 100, align: 'center', headerAlign: 'center' },
    { field: 'currentPhases', headerName: 'Current Phase', width: 150, renderCell: (params: GridRenderCellParams) => <CurrentPhaseDisplay phases={params.value} /> },
    { field: 'status', headerName: 'Status', width: 110, renderCell: (params: GridRenderCellParams) => <StatusBadge status={params.value} /> },
    { field: 'progress', headerName: 'Progress', width: 90, align: 'center', headerAlign: 'center', renderCell: (params: GridRenderCellParams) => <ProgressDonut percentage={params.value} /> },
  ];

  return (
    <>
      <Head><title>TMS - Sprint Charts</title></Head>
      <div className="mx-10 w-full relative max-h-screen overflow-y-auto pr-4">
        <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
          <div className="flex gap-2 items-center">
            {/* <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
            </svg> */}
            <button onClick={() => router.push('/sprints')}>

              <svg width="10" height="17" viewBox="0 0 10 17" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path fill-rule="evenodd" clip-rule="evenodd" d="M2.41379 8.485L9.48479 15.556L8.07079 16.97L0.292786 9.192C0.105315 9.00447 0 8.75016 0 8.485C0 8.21984 0.105315 7.96553 0.292786 7.778L8.07079 0L9.48479 1.414L2.41379 8.485Z" fill="black"/>
              </svg>
            </button>

            {/* <h1 className="font-bold text-2xl">Sprint Charts</h1> */}
            <h1 className="font-bold text-2xl">{sprintName}</h1>
          </div>
          {/* <button onClick={() => router.push('/sprints')} className="px-4 py-1 rounded bg-gray-600 text-white hover:bg-gray-700 transition-colors">Back to Sprints</button> */}
        </div>
        <div className="flex justify-between items-end h-20 ">
          <div className="flex gap-2 items-end h-11 mt-4 px-1">
            <Tab label="Overview" active={activeTab === 'overview'} onClick={() => setActiveTab('overview')} />
            <Tab label="Learning Objectives" active={activeTab === 'progress'} onClick={() => setActiveTab('progress')} />
          </div>
          {
            activeTab === 'progress' && (
              <div className="flex items-end justify-end h-full gap-5">
                <LightDropdown value={phaseFilter} options={phaseOptions} onChange={handlePhaseFilterChange} buttonStyle={buttonStyle} />
                <LightDropdown value={statusFilter} options={statusOptions} onChange={handleStatusFilterChange} buttonStyle={buttonStyle}/>
              </div>
            )
          }
        </div>
        <div className="mt-4">
          {activeTab === 'overview' && <SprintOverview />}
          {activeTab === 'progress' && (
            <div className="bg-white p-6 rounded-lg shadow-sm">
              <h2 className="text-xl font-bold mb-4">Learning Objectives</h2>
              
              {isLoadingTable ? (
                <div className="flex items-center justify-center h-64"><div className="text-gray-500">Loading data...</div></div>
              ) : filteredLoTableData.length > 0 ? (
                <>
                  <DataGrid rows={filteredLoTableData} columns={loTableColumns} pageSizeOptions={[10, 25, 50]} initialState={{ pagination: { paginationModel: { pageSize: 10 } } }} autoHeight disableRowSelectionOnClick getRowHeight={() => 'auto'} sx={{ '& .MuiDataGrid-cell': { display: 'flex', alignItems: 'center' } }} />
                </>
              ) : (
                <div className="flex items-center justify-center h-64"><div className="text-gray-500">{loTableData.length > 0 ? 'No learning objectives match the selected filters' : 'No learning objectives data available'}</div></div>
              )}
            </div>
          )}
        </div>
      </div>
    </>
  );
};

export default SprintChartsPage;
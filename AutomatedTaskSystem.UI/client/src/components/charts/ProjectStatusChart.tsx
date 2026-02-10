import React from 'react';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Cell,
  Legend,
  LabelList
} from 'recharts';

// Type definitions
export interface ProjectStatusData {
  name: string;
  value: number;
  status: 'On Track' | 'At Risk' | 'Delayed';
}

interface ProjectStatusDataWithRemaining extends ProjectStatusData {
  remaining: number;
}

interface ProjectStatusChartProps {
  data?: ProjectStatusData[];
  height?: number;
  backgroundColor?: string;
  padding?: string;
}

// Helper to get color based on status
const getBarColor = (status: string): string => {
  switch (status) {
    case 'On Track': return '#4ade80'; // Green
    case 'At Risk':  return '#fbbf24'; // Orange/Yellow
    case 'Delayed':  return '#f87171'; // Red
    default:         return '#cbd5e1'; // Gray
  }
};

// Custom tooltip component
const CustomTooltip = ({ active, payload, label }: any) => {
  if (active && payload && payload.length) {
    const dataItem = payload[0]?.payload;
    return (
      <div style={{
        backgroundColor: '#fff',
        padding: '5px',
        border: '1px solid #ccc',
        // borderRadius: '4px',
        // boxShadow: '0 2px 2px rgba(0,0,0,0.1)'
      }}>
        <p style={{ margin: 0, fontWeight: 'bold' }}>{label}</p>
        <p style={{ margin: 0, color: getBarColor(dataItem?.status) }}>
          Progress: {dataItem?.value}%
        </p>
        <p style={{ margin: 0, color: '#666' }}>
          Status: {dataItem?.status}
        </p>
        <p style={{ margin: 0, color: '#666' }}>
          Remaining: {100 - dataItem?.value}%
        </p>
      </div>
    );
  }
  return null;
};

// Default data for demonstration purposes
const defaultData: ProjectStatusData[] = [
  { name: 'Loly Adventure', value: 47, status: 'On Track' },
  { name: 'Chummy Yummy', value: 58, status: 'On Track' },
  { name: 'Alpha tics', value: 75, status: 'On Track' },
  { name: 'Colors', value: 60, status: 'Delayed' },
  { name: 'Racing', value: 65, status: 'At Risk' },
  { name: 'Habits', value: 73, status: 'Delayed' },
  { name: 'Finance', value: 82, status: 'On Track' },
  { name: 'Education', value: 45, status: 'At Risk' },
  { name: 'Health', value: 68, status: 'On Track' },
  { name: 'Travel', value: 55, status: 'Delayed' },
];

const ProjectStatusChart: React.FC<ProjectStatusChartProps> = ({
  data = defaultData,
  backgroundColor = '#f8fafc',
  padding = '2px'
}) => {
  // Calculate height: each bar needs ~15-20px for tighter spacing
  const calculatedHeight = data.length * 28;
  const chartHeight = calculatedHeight;
  
  // Prepare data with remaining percentage for background
  const dataWithRemaining: ProjectStatusDataWithRemaining[] = data.map(item => ({
    ...item,
    remaining: 100 - item.value
  }));

  return (
    <div style={{ width: '100%', height: chartHeight, backgroundColor, padding }}>
      <ResponsiveContainer>
        <BarChart
          layout="vertical"
          data={dataWithRemaining}
          barCategoryGap="1%"
          margin={{ top: 0, right: 30, left: 100, bottom: 0 }}
        >
          <CartesianGrid 
            strokeDasharray="3 3" 
            horizontal={false}
            stroke="#e5e7eb"
          />
          <XAxis 
            type="number" 
            domain={[0, 100]} 
            hide 
          />
          <Legend />
          <YAxis 
            dataKey="name" 
            type="category" 
            width={100}
            tick={{ fontSize: 12, fill: '#374151' }}
            axisLine={false}
            tickLine={false}
          />
          <Tooltip content={<CustomTooltip />} />
          
          {/* Foreground bar (progress) - Rendered SECOND so it's on top */}
          <Bar
            dataKey="value"
            stackId="a"
            name="Progress"
            barSize={5}
            // background={fill}
          >
            {dataWithRemaining.map((entry, index) => (
              <Cell
                key={`cell-${index}`}
                fill={getBarColor(entry.status)}
                stroke={getBarColor(entry.status)}
                // strokeWidth={1}
              />
            ))}
            {/* Add value labels at the end of each bar */}
            <LabelList
              dataKey="value"
              position="right"
              offset={10}
              style={{ fill: '#374151', fontSize: 12, fontWeight: 'bold' }}
              // formatter={(value: number) => `${value}%`}
            />
          </Bar>
          {/* Background bar (remaining space) - Rendered FIRST so it's behind */}
          <Bar
            dataKey="remaining"
            stackId="a"
            barSize={5}
            fill="#E7EFFF"
            stroke="#e2e8f0"
            // strokeWidth={1}
            isAnimationActive={false}
          />
          
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
};

export default ProjectStatusChart;
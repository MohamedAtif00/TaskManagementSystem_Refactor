import React, { useState } from 'react';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Cell,
  LabelList,
  Legend
} from 'recharts';

// Type definitions
export interface ProjectStatusData {
  id?: number;
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
  onBarClick?: (item: ProjectStatusData) => void;
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

// Custom legend component
const CustomLegend = () => {
  const legendItems = [
    { status: 'On Track', color: '#4ade80' },
    { status: 'At Risk', color: '#fbbf24' },
    { status: 'Delayed', color: '#f87171' }
  ];

  return (
    <div style={{
      display: 'flex',
      justifyContent: 'center',
      gap: '20px',
      marginTop: '10px',
      marginBottom: '10px'
    }}>
      {legendItems.map((item) => (
        <div key={item.status} style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
          <div style={{
            width: '12px',
            height: '12px',
            backgroundColor: item.color,
            borderRadius: '2px'
          }} />
          <span style={{ fontSize: '12px', color: '#374151' }}>{item.status}</span>
        </div>
      ))}
    </div>
  );
};

// Default data for demonstration purposes
// const defaultData: ProjectStatusData[] = [
//   { name: 'Loly Adventure', value: 47, status: 'On Track' },
//   { name: 'Chummy Yummy', value: 58, status: 'On Track' },
//   { name: 'Alpha tics', value: 75, status: 'On Track' },
//   { name: 'Colors', value: 60, status: 'Delayed' },
//   { name: 'Racing', value: 65, status: 'At Risk' },
//   { name: 'Habits', value: 73, status: 'Delayed' },
//   { name: 'Finance', value: 82, status: 'On Track' },
//   { name: 'Education', value: 45, status: 'At Risk' },
//   { name: 'Health', value: 68, status: 'On Track' },
//   { name: 'Travel', value: 55, status: 'Delayed' },
// ];

const ProjectStatusChart: React.FC<ProjectStatusChartProps> = ({
  data = [],
  backgroundColor = '#f8fafc',
  padding = '2px',
  onBarClick
}) => {
  // Calculate height: each bar needs ~18–22px for tighter spacing
  const calculatedHeight = data.length * 30;
  // const calculatedHeight =  690;
  const chartHeight = calculatedHeight;

  // Prepare data with remaining percentage for background
  const dataWithRemaining: ProjectStatusDataWithRemaining[] = data.map(item => ({
    ...item,
    remaining: 100 - item.value
  }));

  // Handle bar click
  const handleBarClick = (data: any) => {
    if (onBarClick && data) {
      onBarClick(data);
    }
  };

  const YAxisTick = ({ x, y, payload }: any) => {
    const [isHovered, setIsHovered] = useState(false);
    const label = String(payload?.value ?? '');
    const matchingItem = dataWithRemaining.find(d => d.name === label);
    const clickable = Boolean(onBarClick && matchingItem);

    return (
      <g
    transform={`translate(${x},${y})`}
    style={{ cursor: clickable ? 'pointer' : 'default' }}
    // Trigger state changes
    onMouseEnter={() => setIsHovered(true)}
    onMouseLeave={() => setIsHovered(false)}
    onClick={(e) => {
      e.stopPropagation();
      if (clickable && matchingItem) onBarClick?.(matchingItem);
    }}
  >
    <text
      x={0}
      y={0}
      dx={-4}
      dy={4}
      textAnchor="end"
      // Change fill color based on state
      fill={isHovered ? "#2563eb" : "#374151"}
      // Change font weight based on state
      fontWeight={isHovered ? "bold" : "normal"}
      fontSize={12}
      style={{ transition: 'all 0.2s ease' }}
    >
      {label}
    </text>
    
    {/* Detailed Example: Showing an extra background rect only on hover */}
    {isHovered && (
      <rect 
        x={-label.length * 8} 
        y={-10} 
        width={label.length * 8} 
        height={20} 
        fill="rgba(0,0,0,0.05)" 
        rx={4}
      />
    )}
  </g>
    );
  };

  return (
    <div style={{ width: '100%', backgroundColor, padding }}>
      
      <div style={{ width: '100%', height: chartHeight }}>
        <ResponsiveContainer>
          <BarChart
            layout="vertical"
            data={dataWithRemaining}
            barCategoryGap={'10%'}
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
            <YAxis
              dataKey="name"
              type="category"
              width={100}
              tick={<YAxisTick />}
              axisLine={false}
              tickLine={false}

            />
            <Tooltip content={<CustomTooltip />} />

            {/* Single progress bar with status-based coloring */}
            <Bar
              dataKey="value"
              barSize={8}
              background={{ fill: '#E7EFFF' }}
              onClick={handleBarClick}
              cursor={onBarClick ? 'pointer' : 'default'}
            >
              {dataWithRemaining.map((entry, index) => (
                <Cell
                  key={`cell-${index}`}
                  fill={getBarColor(entry.status)}
                />
              ))}
              {/* Add value labels at the end of each bar */}
              <LabelList
                dataKey="value"
                position="right"
                offset={10}
                style={{ fill: '#374151', fontSize: 12, fontWeight: 'bold' }}
                
              />
            </Bar>

          </BarChart>
        </ResponsiveContainer>
        <CustomLegend />
      </div>
    </div>
  );
};

export default ProjectStatusChart;
import React from 'react';
import { Box, Typography } from '@mui/material';
import { PieChart } from '@mui/x-charts/PieChart';

interface ProgressDonutProps {
  percentage: number;
  size?: number;
}

const ProgressDonut: React.FC<ProgressDonutProps> = ({ percentage = 0, size = 70 }) => {
  // Logic remains encapsulated here
  const getStatusColor = (perc: number) => {
    if (perc > 90) return '#22c55e'; // Green
    if (perc > 70) return '#f59e0b'; // Amber
    return '#ef4444';                // Red
  };

  const verticalCenter = '38%';
  const progressData = [
    { label: 'Done', value: percentage, color: getStatusColor(percentage) },
    { label: 'Pending', value: Math.max(0, 100 - percentage), color: '#e5e7eb' },
  ];

  return (
    <Box 
      className="relative flex items-center justify-center" 
      sx={{ width: size, height: size, margin: 'auto' }}
    >
      <PieChart
        series={[
          {
            innerRadius: size * 0.17, // Scaled radii
            outerRadius: size * 0.31,
            data: progressData,
            cx: '50%',
            cy: verticalCenter,
            startAngle: -130,
            endAngle: 230,
            paddingAngle: 0,
          },
        ]}
        hideLegend
        width={size}
        height={size}
        slotProps={{ tooltip: { trigger: 'none' } }}
        margin={{ top: 0, bottom: 0, left: 0, right: 0 }}
      />

      <Box
        className="absolute inset-0 flex items-center justify-center"
        sx={{ 
          pointerEvents: 'none',
          left: '50%',
          top: verticalCenter,
          transform: 'translate(-50%, -50%)',
          width: '100%',
        }}
      >
        <Typography 
          sx={{ 
            fontSize: `${size * 0.14}px`, // Scaled font
            fontWeight: 'bold',
            lineHeight: 1 
          }}
        >
          {percentage.toFixed(0)}%
        </Typography>
      </Box>
    </Box>
  );
};

export default ProgressDonut;
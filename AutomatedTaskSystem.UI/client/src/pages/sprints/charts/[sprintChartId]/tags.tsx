import React, { CSSProperties } from 'react';

// Type definitions
interface ProjectTagProps {
  label: string;
  value: number | string;
  color: string;
  isFilled?: boolean;
}

interface TagData {
  label: string;
  value: number | string;
  color: string;
  isFilled?: boolean;
}

interface TagListProps {
  data: TagData[];
}

// Individual Pill Component
const ProjectTag: React.FC<ProjectTagProps> = ({ label, value, color, isFilled = false }) => {
  // Container Style
  const pillStyle: CSSProperties = {
    display: 'inline-flex',
    alignItems: 'center',
    padding: '4px 12px',
    borderRadius: '20px',
    border: `1px solid ${color}`,
    backgroundColor: isFilled ? color : '#ffffff',
    color: isFilled ? '#ffffff' : '#333333',
    fontFamily: 'Segoe UI, Tahoma, Geneva, Verdana, sans-serif',
    fontSize: '14px',
    fontWeight: '500',
    minWidth: 'fit-content'
  };

  // Dot Style
  const dotStyle: CSSProperties = {
    width: '10px',
    height: '10px',
    borderRadius: '50%',
    backgroundColor: isFilled ? '#ffffff' : color,
    marginRight: '8px'
  };

  // Value (Number) Style
  const valueStyle: CSSProperties = {
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

// Main Wrapper Component
const TagList: React.FC<TagListProps> = ({ data }) => {
  const containerStyle: CSSProperties = {
    display: 'flex',
    flexWrap: 'wrap',
    gap: '10px',
    padding: '20px',
    backgroundColor: '#f0f2f5',
    borderRadius: '8px'
  };

  return (
    <div style={containerStyle}>
      {data.map((item, index) => (
        <ProjectTag 
          key={index}
          label={item.label}
          value={item.value}
          color={item.color}
          isFilled={item.isFilled}
        />
      ))}
    </div>
  );
};

export default TagList;
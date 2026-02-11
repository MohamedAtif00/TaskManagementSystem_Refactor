import { useEffect, useRef, useState } from "react";

const LightDropdown = ({
  value,
  options,
  onChange,
  buttonStyle = {} // Default to empty object to prevent undefined errors
}: {
  value: { id: number; name: string };
  options: { id: number; name: string }[];
  onChange: (value: { id: number; name: string }) => void;
  buttonStyle?: React.CSSProperties; // Optional prop
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const defaultButtonStyle: React.CSSProperties = {
    display: 'inline-flex',
    alignItems: 'center',
    justifyContent: 'center',
    gap: '8px',
    padding: '10px 20px',
    borderRadius: '12px',
    border: '2px solid #3B82F6',
    backgroundColor: '#EEF2FF',
    color: '#3B82F6',
    fontSize: '15px',
    fontWeight: '500',
    cursor: 'pointer',
    minWidth: 'fit-content',
    outline: 'none',
    transition: 'all 0.2s ease'
  };

  // Merge: incoming props overwrite defaults
  const mergedButtonStyle = { ...defaultButtonStyle, ...buttonStyle };

  const dropdownListStyle: React.CSSProperties = {
    position: 'absolute',
    top: '100%',
    left: '0',
    marginTop: '4px',
    backgroundColor: '#ffffff',
    border: '1px solid #E5E7EB',
    borderRadius: '12px',
    boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
    zIndex: 50,
    minWidth: '100%',
    overflow: 'hidden'
  };

  const optionStyle: React.CSSProperties = {
    padding: '10px 16px',
    cursor: 'pointer',
    color: '#374151',
    fontSize: '14px',
    transition: 'background-color 0.15s ease'
  };

  return (
    <div ref={dropdownRef} style={{ position: 'relative', display: 'inline-block' }}>
      <button
        onClick={() => setIsOpen(!isOpen)}
        style={mergedButtonStyle} // FIXED: Use the merged style here
        onMouseEnter={(e) => {
          e.currentTarget.style.backgroundColor = '#DBEAFE';
        }}
        onMouseLeave={(e) => {
          // FIXED: Returns to the merged background color, not hardcoded blue
          e.currentTarget.style.backgroundColor = (mergedButtonStyle.backgroundColor as string);
        }}
      >
        <span>{value.name}</span>
        <svg
          width="12"
          height="12"
          viewBox="0 0 12 12"
          fill="none"
          style={{
            transform: isOpen ? 'rotate(180deg)' : 'rotate(0deg)',
            transition: 'transform 0.2s ease'
          }}
        >
          <path
            d="M2.5 4.5L6 8L9.5 4.5"
            // FIXED: Stroke color should match the merged text color
            stroke={mergedButtonStyle.color as string} 
            strokeWidth="1.5"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
      </button>

      {isOpen && (
        <div style={dropdownListStyle}> {/* FIXED: Was previously mergedButtonStyle */}
          {options.map((option) => (
            <div
              key={option.id}
              onClick={() => {
                onChange(option);
                setIsOpen(false);
              }}
              style={{
                ...optionStyle,
                backgroundColor: option.id === value.id ? '#EEF2FF' : 'transparent',
                color: option.id === value.id ? '#3B82F6' : '#374151',
                fontWeight: option.id === value.id ? '500' : '400'
              }}
              onMouseEnter={(e) => {
                if (option.id !== value.id) {
                  e.currentTarget.style.backgroundColor = '#F3F4F6';
                }
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.backgroundColor = option.id === value.id ? '#EEF2FF' : 'transparent';
              }}
            >
              {option.name}
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export { LightDropdown };
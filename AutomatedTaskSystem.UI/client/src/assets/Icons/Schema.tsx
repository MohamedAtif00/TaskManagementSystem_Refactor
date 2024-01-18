const SchemaIcon = ({ color = "black" }: { color?: string }) => {
    return (
        <svg
            className="w-full h-full"
            width="24"
            height="24"
            viewBox="0 0 24 24"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
        >
            <circle cx="19.5" cy="4.5" r="4.5" fill={color} />
            <circle cx="4.5" cy="4.5" r="4" stroke={color} />
            <circle cx="19.5" cy="19.5" r="4" stroke={color} />
            <circle cx="4.5" cy="19.5" r="4" stroke={color} />
            <rect
                x="5"
                y="8"
                width="8"
                height="1"
                transform="rotate(90 5 8)"
                fill={color}
            />
            <rect x="8" y="19" width="8" height="1" fill={color} />
            <rect x="8" y="4" width="8" height="1" fill={color} />
        </svg>
    );
};

export default SchemaIcon;

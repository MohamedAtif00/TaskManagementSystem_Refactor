const ProjectIcon = ({ color = "black" }: { color?: string }) => {
    return (
        <svg
            className="w-full h-full"
            width="24"
            height="24"
            viewBox="0 0 20 24"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
        >
            <path
                d="M13.75 2.4H2.5V21.6H17.5V6H13.75V2.4ZM2.5 0H15L20 4.8V21.6C20 22.2365 19.7366 22.847 19.2678 23.2971C18.7989 23.7471 18.163 24 17.5 24H2.5C1.83696 24 1.20107 23.7471 0.732233 23.2971C0.263392 22.847 0 22.2365 0 21.6V2.4C0 1.76348 0.263392 1.15303 0.732233 0.702944C1.20107 0.252856 1.83696 0 2.5 0V0ZM5 10.8H15V13.2H5V10.8ZM5 15.6H15V18H5V15.6Z"
                fill={color}
            />
        </svg>
    );
};

export default ProjectIcon;

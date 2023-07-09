interface Props {
    className?: string;
    color?: string;
}

const TaskIcon = (props: Props) => {
    return (
        <svg
            width="24"
            height="24"
            viewBox="0 0 24 24"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
            className={[props.className, "w-full h-full"].join("")}
        >
            <path
                d="M18.1111 1H5.88889C3.18883 1 1 3.18883 1 5.88889V18.1111C1 20.8112 3.18883 23 5.88889 23H18.1111C20.8112 23 23 20.8112 23 18.1111V5.88889C23 3.18883 20.8112 1 18.1111 1Z"
                stroke={props.color}
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
            />
            <path
                d="M8.33334 12L11.0833 14.4444L15.6667 9.55554"
                stroke={props.color}
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
            />
        </svg>
    );
};

export default TaskIcon;

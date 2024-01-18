const PlusIcon = ({
	color,
	className,
}: {
	color?: string;
	className?: string;
}) => {
	return (
		<svg
			width="18"
			height="18"
			viewBox="0 0 18 18"
			fill="none"
			xmlns="http://www.w3.org/2000/svg"
			className={className ? className : ""}
		>
			<path
				d="M8 17C8 17.5523 8.44772 18 9 18C9.55229 18 10 17.5523 10 17H8ZM10 1C10 0.447715 9.55229 0 9 0C8.44772 0 8 0.447715 8 1H10ZM17 10C17.5523 10 18 9.55229 18 9C18 8.44772 17.5523 8 17 8V10ZM1 8C0.447715 8 0 8.44772 0 9C0 9.55229 0.447715 10 1 10V8ZM10 17V9H8V17H10ZM10 9V1H8V9H10ZM9 10H17V8H9V10ZM9 8H1V10H9V8Z"
				fill={color ? color : "white"}
			/>
		</svg>
	);
};

export default PlusIcon;

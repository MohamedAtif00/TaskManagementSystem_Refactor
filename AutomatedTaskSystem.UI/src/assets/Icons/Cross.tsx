interface Props {
	className?: string;
}

const CrossIcon = ({ className }: Props) => {
	return (
		<svg
			width="24"
			height="24"
			viewBox="0 0 24 24"
			fill="none"
			xmlns="http://www.w3.org/2000/svg"
			className={className}
		>
			<path
				d="M20 20L4 4M20 4L4 20"
				stroke="white"
				strokeWidth="2"
				strokeLinecap="round"
			/>
		</svg>
	);
};

export default CrossIcon;

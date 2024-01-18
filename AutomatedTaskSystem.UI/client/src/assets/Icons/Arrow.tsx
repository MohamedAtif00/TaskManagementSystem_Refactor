const ArrowIcon = ({ color }: { color: string }) => {
	return (
		<svg
			width="13"
			height="10"
			viewBox="0 0 13 10"
			fill="none"
			xmlns="http://www.w3.org/2000/svg"
		>
			<path
				d="M5.08579 8.08579L1.41421 4.41421C0.154284 3.15428 1.04662 1 2.82843 1H10.1716C11.9534 1 12.8457 3.15428 11.5858 4.41421L7.91421 8.08579C7.13316 8.86684 5.86683 8.86683 5.08579 8.08579Z"
				fill={color}
				stroke={color}
			/>
		</svg>
	);
};

export default ArrowIcon;

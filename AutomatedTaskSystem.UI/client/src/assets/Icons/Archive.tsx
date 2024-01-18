interface Props {
	className: string;
}

const ArchiveIcon = ({ className }: Props) => {
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
				d="M2.804 6.132C3.00149 5.24358 3.49605 4.44907 4.20601 3.87965C4.91597 3.31022 5.79889 2.99993 6.709 3H16.291C17.2011 2.99993 18.084 3.31022 18.794 3.87965C19.504 4.44907 19.9985 5.24358 20.196 6.132L20.343 6.794C21.105 10.2228 21.105 13.7772 20.343 17.206L20.196 17.868C19.9985 18.7564 19.504 19.5509 18.794 20.1204C18.084 20.6898 17.2011 21.0001 16.291 21H6.71C5.79989 21.0001 4.91697 20.6898 4.20701 20.1204C3.49705 19.5509 3.00249 18.7564 2.805 17.868L2.658 17.206C1.89608 13.7771 1.89608 10.2229 2.658 6.794L2.805 6.132H2.804Z"
				strokeWidth="2"
				strokeLinecap="round"
				strokeLinejoin="round"
			/>
			<path
				d="M2 13H8.338C8.338 14 9.311 16 11.743 16C14.176 16 15.149 14 15.149 13H21"
				strokeWidth="2"
				strokeLinejoin="round"
			/>
		</svg>
	);
};

export default ArchiveIcon;

interface Props {
	value: string,
	handleChange: (s: string) => void
}

const InputTextField = ({ value, handleChange }: Props) => {
	return (
		<div className="flex flex-col">
			<label htmlFor="name" className="text-sm">Name</label>
			<input
				value={value}
				onChange={e => handleChange(e.target.value)}
				type="text"
				id="name"
				className="border-2 border-solid border-black px-2 basis-10 outline outline-transparent"
			/>
		</div>
	);
}

export default InputTextField;
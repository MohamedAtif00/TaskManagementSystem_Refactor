import { useRouter } from "next/router"
import { Dispatch, SetStateAction, useEffect, useState } from "react";
import ArrowIcon from "../../../assets/Icons/Arrow";
import API, { BasicInfo } from "../../../lib/API";
import Backdrop from "../backdrop"

const FormField = ({ label, onChange, value }: {
	label: string;
	onChange: (params: string) => void;
	value: string;
}) => {
	return (
		<label className="flex flex-col gap-1">
			<div>{label}</div>
			<input className="outline-none px-2 py-1 w-full border border-solid border-slate-200 rounded-lg" value={value} onChange={e => onChange(e.target.value)} />
		</label>
	);
}

const Dropdown = ({ label, options, id, handleChange }: {
	label: string;
	options: { id: number; name: string }[];
	id: number;
	handleChange: (params: BasicInfo) => void;
}) => {
	const [active, setActive] = useState(false);

	const foundOption = options.find((opt) => opt.id === id);

	return (
		<div
			className="relative transition-all ease-in"
			onMouseLeave={() => (active ? setActive(false) : "")}
		>
			<div className="label">{label}:</div>
			<button
				className="flex justify-between items-center border rounded-lg px-2 py-2 text-center w-full outline-none"
				type="button"
				onClick={() => {
					setActive((ps) => !ps);
				}}
			>
				<div className={foundOption ? "" : "text-slate-300"}>
					{foundOption
						? foundOption.name
						: `Please select a ${label}`}
				</div>
				<div>
					<ArrowIcon color="#DBDFE5" />
				</div>
			</button>
			<div
				className={`absolute right-0 w-full rounded-lg bg-white overflow-y-scroll transition-[max-height] ease-in ${active ? "border border-solid border-slate-200 z-10 max-h-60" : "max-h-0 "}`}
			>
				{options.map((opt) => {
					return (
						<div
							key={opt.id}
							onClick={() => {
								handleChange(opt);
								setActive(false);
							}}
							className="cursor-pointer hover:bg-slate-200 p-2 text-center"
						>
							{opt.name}
						</div>
					);
				})}
			</div>
		</div>
	);
};

const CheckBox = (props: { id: number; name: string; selected: boolean; onClick: (id: number) => void }) => {
	return (
		<div
			className="flex gap-4 px-4 py-2 hover:bg-slate-100 select-none"
			onClick={() => props.onClick(props.id)}
		>
			<div className="flex flex-col justify-center items-center">
				<div className={`transition-all ease-in p-2 rounded border border-solid ${props.selected ? "bg-blue-500 border-blue-400" : "border-slate-300"}`}></div>
			</div>
			<div className={`${props.selected ? "font-bold" : "text-slate-500"} transition-all ease-in`}>{props.name}</div>
		</div>
	);
}

const CheckList = (props: {
	schema: {
		id: number;
		name: string;
	};
	selected: number[];
	updateSelected: Dispatch<SetStateAction<number[]>>;
}) => {
	const [nodes, setNodes] = useState<BasicInfo[]>([]);

	const handleAddToSelected = (id: number) => props.updateSelected(ps => {
		if (ps.includes(id)) {
			const newState: number[] = [];
			ps.forEach(_ => _ !== id && newState.push(_));
			return newState;
		}
		return [...ps, id];
	})

	useEffect(() => {
		API.SCHEMAS.NODES.GET_ALL_MINI(props.schema.id)
			.then(res => res && setNodes(res));
	}, [props.schema, setNodes]);

	return (
		<div className="flex flex-col gap-1">
			<h3><span className="font-bold">{props.schema.name}</span> Nodes:</h3>
			<div className="py-1 border border-solid border-slate-200 rounded-lg">
				{nodes.map(n =>
					<CheckBox
						id={n.id}
						key={n.id}
						name={n.name}
						selected={props.selected.includes(n.id)}
						onClick={handleAddToSelected}
					/>
				)}
			</div>
		</div>
	);
};

const RadioBox = (props: { id: number; name: string; selected: boolean; onClick: () => void }) => {
	return (
		<div
			className="flex gap-4 px-4 py-2 hover:bg-slate-100 select-none"
			onClick={props.onClick}
		>
			<div className="flex flex-col justify-center items-center">
				<div className={`transition-all ease-in p-2 rounded-xl border border-solid ${props.selected ? "bg-blue-500 border-blue-400" : "border-slate-300"}`}></div>
			</div>
			<div className={`${props.selected ? "font-bold" : "text-slate-500"} transition-all ease-in`}>{props.name}</div>
		</div>
	);
}

const RadioList = ({ nodes, updateState }: { nodes: number[]; updateState: (values: number[]) => void }) => {
	const [selectedValues, setSelectedValues] = useState<{ nodeId: number; stepId: number; }[]>([]);
	const [nodesWithSteps, setNodesWithSteps] = useState<{
		id: number;
		name: string;
		steps: BasicInfo[]
	}[]>([]);

	const addValues = (value: { nodeId: number; stepId: number; }) => {
		setSelectedValues(ps => {
			if (!ps.find(_ => _.nodeId === value.nodeId)) return [...ps, value];

			const newState: { nodeId: number; stepId: number }[] = [];
			ps.forEach(_ => _.nodeId !== value.nodeId ? newState.push(_) : newState.push(value));
			return newState;
		});
	}

	useEffect(() => {
		updateState(selectedValues.map(_ => _.stepId));
	}, [selectedValues, updateState]);

	useEffect(() => {
		API.SCHEMAS.NODES.STEPS.GET_MULTIPLE(nodes)
			.then(res => res && setNodesWithSteps(res));
	}, [nodes]);

	return (
		<div className="flex flex-col gap-4">
			{nodesWithSteps.map(n =>
				<div
					key={n.id}
					className="py-1 border border-solid border-slate-200 rounded-lg">
					<h3 className="px-2"><span className="font-bold">{n.name}</span> Steps:</h3>
					{n.steps.map(s => {
						return <RadioBox
							id={s.id}
							key={s.id}
							name={s.name}
							selected={!!selectedValues.find(_ => _.stepId === s.id)}
							onClick={() => addValues({ nodeId: n.id, stepId: s.id })}
						/>
					})}
				</div>
			)}
		</div>
	);
};

const EditLearningObjective = (props: LearningObjective & { updateLo: (params: LearningObjective) => void }) => {
	const router = useRouter();
	const [submittable, setSubmittable] = useState(false);
	const [name, setName] = useState(props.name);
	const [tag, setTag] = useState(props.tag);
	const [template, setTemplate] = useState(props.template);
	const [environment, setEnvironment] = useState(props.environment);
	const [schema, setSchema] = useState<BasicInfo>(props.schema);
	const [schemas, setSchemas] = useState<BasicInfo[]>([]);
	const [currentStep, setCurrentStep] = useState<1 | 2 | 3>(1);
	const [selectedNodes, setSelectedNodes] = useState<number[]>([]);
	const [selectedSteps, setSelectedSteps] = useState<number[]>([]);

	useEffect(() => {
		API.SCHEMAS.GET_ALL_MINI()
			.then(res => res && setSchemas(res))
			.catch(err => console.error(err));
	}, [setSchemas]);

	useEffect(() => {
		switch (currentStep) {
			case 1:
				setSelectedNodes([]);
				break;
			case 2:
				setSelectedSteps([]);
		}
	}, [currentStep]);

	useEffect(() => {
		switch (currentStep) {
			case 1:
				if (name !== "" && !submittable) {
					setSubmittable(true);
					break;
				}
				(name === "" && submittable) && setSubmittable(false);
				break;
			case 2:
				if (selectedNodes.length !== 0 && !submittable) {
					setSubmittable(true);
					break;
				}
				(selectedNodes.length === 0 && submittable) && setSubmittable(false);
				break;
			case 3:
				if (selectedSteps.length === selectedNodes.length && !submittable) {
					setSubmittable(true);
					break;
				}
				(selectedSteps.length !== selectedNodes.length && submittable) && setSubmittable(false);
		}
	}, [currentStep, name, submittable, setSubmittable, selectedNodes, selectedSteps]);

	const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
		e.preventDefault();
		if (currentStep === 2)
			return setCurrentStep(3);
		if (currentStep === 1 && schema.id !== props.schema.id)
			return setCurrentStep(2);
		if (submittable)
			API.PROJECTS.UNITS.LESSONS.LEARNING_OBJECTIVES.EDIT(props.id, {
				schemaId: schema.id,
				template,
				environment,
				name,
				tag,
				steps: selectedSteps
			})
			.then(lo => lo && props.updateLo(lo));
	};

	const handleGotoPreviousStep = () => {
		if (currentStep === 2)
			return setCurrentStep(1);
		else if (currentStep === 3)
			return setCurrentStep(2);
	};

	return (
		<Backdrop mainRoute={`/projects/${router.query.projectId}`}>
			<div className="bg-white p-8 rounded-lg max-h-[90vh] w-96 flex flex-col gap-4 overflow-y-scroll">
				<h3 className="text-xl">Editing <span className="font-bold">{props.name}</span></h3>
				<form onSubmit={handleSubmit} className="gap-4 flex flex-col">
					<div className="flex flex-col gap-2">
						{currentStep === 1 ? <>
							<FormField
								label="Name"
								onChange={setName}
								value={name}
							/>
							<Dropdown id={schema.id}
								label="Schema"
								options={schemas}
								handleChange={setSchema} />
							<FormField
								label="Tag"
								onChange={setTag}
								value={tag}
							/>
							<FormField
								label="Template"
								onChange={setTemplate}
								value={template}
							/>
							<FormField
								label="Environment"
								onChange={setEnvironment}
								value={environment}
							/>
						</> :
							currentStep === 2 ? <>
								<CheckList selected={selectedNodes} updateSelected={setSelectedNodes} schema={schema} />
							</> : <>
								<RadioList nodes={selectedNodes} updateState={setSelectedSteps} />
							</>}
					</div>
					<div className="flex flex-col gap-2">
						{currentStep !== 1 && <button
							onClick={handleGotoPreviousStep}
							type="button"
							className={`border border-slate-700 border-solid text-center bg-white rounded-lg transition-all ease-in opacity-90 hover:opacity-100 w-full py-2 text-slate-800`}>
							Previous
						</button>}
						<input
							type="submit"
							value={props.schema.id !== schema.id && currentStep !== 3 ? "Next" : "Save"}
							className={`text-center ${submittable ? "cursor-pointer bg-blue-500" : "bg-slate-300"} rounded-lg transition-all ease-in opacity-90 hover:opacity-100 w-full py-2 text-white`}
						/>
					</div>
				</form>
			</div>
		</Backdrop>
	);
}

export default EditLearningObjective;

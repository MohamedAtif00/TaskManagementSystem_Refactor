import Header from "../../components/header/header";
import styles from "../../styles/resources.module.scss";
import SchemaItem from "../../components/schemaItem/schemaItem";
import QueryButton from "../../components/button/queryButton";
import PlusIcon from "../../assets/Icons/Plus";
import { useAppDispatch, useAppSelector } from "../../app/hooks";
import { useEffect } from "react";
import API from "../../lib/API";
import { clear, load } from "../../slices/schemaSlice";
import AddSchema from "../../components/forms/schemas/addSchema";
import CopyIcon from "../../assets/Icons/Copy";
import DuplicateSchemaForm from "../../components/forms/schemas/duplicate";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import Link from "next/link";
import SchemaIcon from "../../assets/Icons/Schema";

const columns: GridColDef[] = [
	{ field: "col0", headerName: "ID", width: 100 },
	{
		field: "col1",
		headerName: "Name",
		width: 300,
	},
	{ field: "col2", headerName: "Description", width: 300 },
	{
		field: "col3",
		headerName: "Actions",
		width: 450,
		renderCell: (c) => (
			<div className="flex justify-end gap-4">
				<Link
					href={{
						pathname: `/schemas/${c.id}`,
					}}
				>
					<div className="text-black hover:underline cursor-pointer">
						View
					</div>
				</Link>
				<Link
					href={{
						pathname: "/schemas",
						query: {
							form: "edit-schema",
							schemaId: c.id,
						},
					}}
				>
					<div className="text-blue-600 hover:underline cursor-pointer">
						Edit
					</div>
				</Link>
				<Link
					href={{
						pathname: "/schemas",
						query: {
							form: "remove-schema",
							schemaId: c.id,
						},
					}}
				>
					<div className="text-red-600 hover:underline cursor-pointer">
						Delete
					</div>
				</Link>
			</div>
		),
		filterable: false,
		disableColumnMenu: true,
		sortable: false,
	},
];

const Schema = () => {
	const schemas = useAppSelector((states) => states.schemasSlice);
	const dispatch = useAppDispatch();

	useEffect(() => {
		API.SCHEMAS.GET_ALL().then((res) => {
			if (res) {
				dispatch(load(res));
			}
		});

		return () => {
			dispatch(clear());
		};
	}, [dispatch]);

	return (
		<div className="mx-auto relative max-h-screen overflow-y-auto pr-4">
			<div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
				<div className="flex gap-2 items-center">
					<SchemaIcon />
					<h1 className="font-bold text-2xl ">Schema</h1>
				</div>
				<Link
					href={{
						pathname: "/schemas",
						query: {
							form: "add-schema",
						},
					}}
				>
					<button className="px-4 py-1 rounded bg-blue-600 text-white">
						Create Schema
					</button>
				</Link>
			</div>
			<div className="pb-4 mt-4">
				<DataGrid
					className="bg-white relative h-full"
					rows={schemas.map((p) => {
						return {
							id: p.id,
							col0: p.id,
							col1: p.name,
							col2: p.description,
						};
					})}
					columns={columns}
				/>
			</div>
			{/* <AddProject />
			<EditProject />
			<RemoveProject /> */}
		</div>
	);
};

// <Header text="Schema" icon="Schema">
// 	<QueryButton
// 		text="View Task Bank"
// 		url={{
// 			pathname: "/schemas/task-bank",
// 		}}
// 	/>
// 	<QueryButton
// 		icon={<PlusIcon />}
// 		iconLeft
// 		iconRight={false}
// 		text="Add"
// 		url={{
// 			pathname: "/schemas",
// 			query: {
// 				form: "schema",
// 			},
// 		}}
// 	/>
// 	<QueryButton
// 		icon={<CopyIcon className="stroke-white" />}
// 		iconLeft
// 		iconRight={false}
// 		text="Duplicate"
// 		url={{
// 			pathname: "/schemas",
// 			query: {
// 				form: "duplicate",
// 			},
// 		}}
// 	/>
// </Header>
// <div className={styles.container}>
// 	{schemas.map((s) => (
// 		<SchemaItem
// 			name={s.name}
// 			tasks={s.tasks}
// 			key={s.id}
// 			id={s.id}
// 		/>
// 	))}
// </div>
// <AddSchema />
// <DuplicateSchemaForm />

export default Schema;

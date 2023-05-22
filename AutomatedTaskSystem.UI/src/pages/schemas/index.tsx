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

const Schema = () => {
	const schemas = useAppSelector((states) => states.schemasSlice);
	const dispatch = useAppDispatch();
	const auth = useAppSelector((s) => s.authSlice);

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
		<div className="mainContainer">
			<Header text="Schema" icon="Schema">
				{auth.role === 1 ? (
					<>
						<QueryButton
							text="View Task Bank"
							url={{
								pathname: "/schemas/task-bank",
							}}
						/>
						<QueryButton
							icon={<PlusIcon />}
							iconLeft
							iconRight={false}
							text="Add"
							url={{
								pathname: "/schemas",
								query: {
									form: "schema",
								},
							}}
						/>
						<QueryButton
							icon={<CopyIcon className="stroke-white" />}
							iconLeft
							iconRight={false}
							text="Duplicate"
							url={{
								pathname: "/schemas",
								query: {
									form: "duplicate",
								},
							}}
						/>
					</>
				) : (
					<></>
				)}
			</Header>
			<div className={styles.container}>
				{schemas.map((s) => (
					<SchemaItem
						name={s.name}
						tasks={s.tasks}
						key={s.id}
						id={s.id}
					/>
				))}
			</div>
			<AddSchema />
			<DuplicateSchemaForm />
		</div>
	);
};

export default Schema;

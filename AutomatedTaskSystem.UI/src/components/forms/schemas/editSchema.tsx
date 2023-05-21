import styles from "../styles.module.scss";
import Backdrop from "../backdrop";
import React, { useEffect, useState } from "react";
import FormField from "../field";
import { useRouter } from "next/router";
import API from "../../../lib/API";

interface ISchemaLocal {
	description: string;
	id: number;
	name: string;
}

const EditSchema = ({
	schema,
	updateSchema,
}: {
	schema: ISchemaLocal;
	updateSchema: (values: { name: string; description: string }) => void;
}) => {
	const [submittable, setSubmittable] = useState(false);
	const [name, setName] = useState(schema.name);
	const [description, setDescription] = useState(schema.description);
	const router = useRouter();

	useEffect(() => {
		if (name === "") {
			setSubmittable(false);
		} else setSubmittable(true);
	}, [name]);

	const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
		e.preventDefault();
		if (submittable)
			API.SCHEMAS.EDIT({ id: schema.id, name, description }).then(
				(res) => {
					if (res) {
						updateSchema({
							name: res.name,
							description: res.description,
						});
						router.back();
					}
				}
			);
	};

	return (
		<Backdrop mainRoute={`/schemas/${schema.id}`}>
			<div className={styles.form}>
				<form onSubmit={handleSubmit}>
					<div className={styles.inputs}>
						<FormField
							label="Name"
							onChange={setName}
							value={name}
						/>
						<FormField
							label="Description"
							onChange={setDescription}
							value={description}
						/>
					</div>
					<div>
						<input
							type="submit"
							value="Save"
							className={[
								styles.submit,
								submittable ? "" : styles.inactive,
							].join(" ")}
						/>
					</div>
				</form>
			</div>
		</Backdrop>
	);
};

export default EditSchema;

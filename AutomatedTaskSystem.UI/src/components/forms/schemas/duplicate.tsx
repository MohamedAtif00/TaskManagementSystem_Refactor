import styles from "../styles.module.scss";
import Backdrop from "../backdrop";
import React, { useEffect, useState } from "react";
import { useRouter } from "next/router";
import API from "../../../lib/API";
import { useAppDispatch } from "../../../app/hooks";
import { add } from "../../../slices/schemaSlice";
import Dropdown from "../dropdown";

const DuplicateSchemaForm = () => {
	const [active, setActive] = useState(false);
	const [id, setId] = useState(0);
	const [schemas, setSchemas] = useState<{ id: number; name: string }[]>([]);
	const router = useRouter();
	const dispatch = useAppDispatch();

	useEffect(() => {
		API.SCHEMAS.GET_ALL().then(res => {
			res && setSchemas(res);
		})
	}, []);

	useEffect(() => {
		const _active = router.query.form === "duplicate";
		setActive(_active);
		if (!_active) {
			setId(0);
		}
	}, [router, setId]);

	const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
		e.preventDefault();
		if (id !== 0)
			API.SCHEMAS.DUPLICATE(id).then((res) => {
				if (res) {
					dispatch(add(res));
					router.back();
				}
			});
	};

	if (active)
		return (
			<Backdrop mainRoute="/schemas">
				<div className={styles.form}>
					<form onSubmit={handleSubmit}>
						<div className={styles.inputs}>
							<Dropdown
								label="Schema"
								id={id}
								options={schemas}
								handleChange={setId}
							/>
						</div>
						<div>
							<input
								type="submit"
								value="Add"
								className={[
									styles.submit,
									id !== 0 ? "" : styles.inactive,
								].join(" ")}
							/>
						</div>
					</form>
				</div>
			</Backdrop>
		);

	return <></>;
};

export default DuplicateSchemaForm;

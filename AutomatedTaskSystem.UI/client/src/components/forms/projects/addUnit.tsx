import styles from "../styles.module.scss";
import Backdrop from "../backdrop";
import React, { useEffect, useState } from "react";
import FormField from "../field";
import { useRouter } from "next/router";

const AddUnit = ({
	path,
	submit,
}: {
	path: string;
	submit: (name: string) => void;
}) => {
	const [submittable, setSubmittable] = useState(false);
	const [name, setName] = useState("");
	const [active, setActive] = useState(false);
	const router = useRouter();

	useEffect(() => {
		const _active = router.query.form === "unit";
		setActive(_active);
		if (!_active) setName("");
	}, [router]);

	useEffect(() => {
		if (name === "") {
			setSubmittable(false);
		} else setSubmittable(true);
	}, [name]);

	const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
		e.preventDefault();
		if (submittable) submit(name);
	};

	if (active)
		return (
			<Backdrop mainRoute={path}>
				<div className={[styles.form, styles.center].join(" ")}>
					<form onSubmit={handleSubmit}>
						<div className={styles.inputs}>
							<FormField
								label="Name"
								onChange={setName}
								value={name}
							/>
						</div>
						<div>
							<input
								type="submit"
								value="Add"
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

	return <></>;
};

export default AddUnit;

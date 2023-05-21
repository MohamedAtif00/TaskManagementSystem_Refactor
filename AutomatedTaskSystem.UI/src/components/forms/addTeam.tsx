import styles from "./styles.module.scss";
import Backdrop from "./backdrop";
import React, { useEffect, useState } from "react";
import FormField from "./field";
import { useRouter } from "next/router";
import API from "../../lib/API";
import { useAppDispatch } from "../../app/hooks";
import { add } from "../../slices/teamSlice";

const AddTeam = () => {
	const [submittable, setSubmittable] = useState(false);
	const [name, setName] = useState("");
	const [active, setActive] = useState(false);
	const router = useRouter();
	const dispatch = useAppDispatch();

	useEffect(() => {
		if (name === "") {
			setSubmittable(false);
		} else setSubmittable(true);
	}, [name]);

	useEffect(() => {
		const _active = router.query.form === "team";
		setActive(_active);
		if (!_active) {
			setName("");
		}
	}, [router]);

	const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
		e.preventDefault();
		if (name == "") {
			return;
		}
		API.RESOURCES.TEAMS.CREATE({ name }).then((res) => {
			if (res) {
				dispatch(add(res));
				router.back();
			}
		});
	};

	if (active)
		return (
			<Backdrop mainRoute="/resources/teams">
				<div className={styles.form}>
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

export default AddTeam;

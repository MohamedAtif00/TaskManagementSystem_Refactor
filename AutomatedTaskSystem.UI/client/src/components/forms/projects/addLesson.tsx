import styles from "../styles.module.scss";
import Backdrop from "../backdrop";
import React, { useEffect, useRef, useState } from "react";
import FormField from "../field";
import { useRouter } from "next/router";

const AddLesson = ({
	path,
	submit,
}: {
	path: string;
	submit: (name: string) => void | Promise<boolean | void>;
}) => {
	const [submittable, setSubmittable] = useState(false);
	const [active, setActive] = useState(false);
	const [name, setName] = useState("");
	const [isSubmitting, setIsSubmitting] = useState(false);
	const submittingRef = useRef(false);
	const router = useRouter();

	useEffect(() => {
		const _active = router.query.form === "lesson";
		setActive(_active);
		if (!_active) {
			setName("");
			submittingRef.current = false;
			setIsSubmitting(false);
		}
	}, [router]);

	useEffect(() => {
		if (name === "") {
			setSubmittable(false);
		} else {
			setSubmittable(true);
		}
	}, [name]);

	const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
		e.preventDefault();
		if (!submittable || submittingRef.current) return;
		submittingRef.current = true;
		setIsSubmitting(true);
		try {
			const ok = await submit(name);
			if (ok === false) {
				submittingRef.current = false;
				setIsSubmitting(false);
			}
		} catch {
			submittingRef.current = false;
			setIsSubmitting(false);
		}
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
								value={isSubmitting ? "Adding..." : "Add"}
								disabled={isSubmitting || !submittable}
								className={[
									styles.submit,
									submittable && !isSubmitting ? "" : styles.inactive,
								].join(" ")}
							/>
						</div>
					</form>
				</div>
			</Backdrop>
		);

	return <></>;
};

export default AddLesson;

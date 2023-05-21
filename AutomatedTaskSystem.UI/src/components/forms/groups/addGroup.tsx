import styles from "../styles.module.scss";
import Backdrop from "../backdrop";
import React, { useEffect, useState } from "react";
import FormField from "../field";
import { useRouter } from "next/router";
import ColorSelection from "../colorSelection";
import API from "../../../lib/API";
import { useAppDispatch } from "../../../app/hooks";
import { add } from "../../../slices/groupSlice";

const colorList = [
	{ color: "orange", id: 1 },
	{ color: "blue", id: 2 },
	{ color: "green", id: 3 },
	{ color: "red", id: 4 },
	{ color: "pink", id: 5 },
	{ color: "black", id: 6 },
	{ color: "cyan", id: 7 },
];

const AddGroup = () => {
	const [submittable, setSubmittable] = useState(false);
	const [name, setName] = useState("");
	const [color, setColor] = useState(1);
	const [active, setActive] = useState(false);
	const router = useRouter();
	const dispatch = useAppDispatch();

	useEffect(() => {
		const _active = router.query.form === "group";
		setActive(_active);
		if (!_active) {
			setName("");
		}
	}, [router]);

	useEffect(() => {
		const _color = colorList.find((i) => i.id == color);
		if (name === "" || !_color) {
			setSubmittable(false);
		} else setSubmittable(true);
	}, [name, color]);

	const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
		e.preventDefault();
		const _color = colorList.find((i) => i.id == color);
		if (submittable) {
			API.RESOURCES.GROUPS.CREATE({
				name,
				colorCode: _color!.color,
			}).then((res) => {
				if (res) {
					dispatch(add(res));
					router.back();
				}
			});
		}
	};

	if (active)
		return (
			<Backdrop mainRoute="/resources/groups">
				<div className={styles.form}>
					<form onSubmit={handleSubmit}>
						<div className={styles.inputs}>
							<FormField
								label="Name"
								onChange={setName}
								value={name}
							/>
							<ColorSelection
								list={colorList}
								active={color}
								onClick={setColor}
							/>
						</div>
						<div>
							<input
								type="submit"
								value="Add"
								className={[
									styles.submit,
									!submittable ? styles.inactive : "",
								].join(" ")}
							/>
						</div>
					</form>
				</div>
			</Backdrop>
		);

	return <></>;
};

export default AddGroup;

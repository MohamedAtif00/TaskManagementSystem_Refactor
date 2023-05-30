import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import FormConclusion from "../../formComponents/FormConclusion";
import API from "../../../lib/API";
import { motion } from "framer-motion";
import { useAppDispatch } from "../../../app/hooks";
import { add } from "../../../slices/schemaSlice";

const DuplicateSchema = () => {
	const dispatch = useAppDispatch();
	const { query, pathname, push: routerPush } = useRouter();
	const [active, setActive] = useState<boolean>(false);
	const [schema, setSchema] = useState<ISchema>();

	useEffect(() => {
		if (query.form === "duplicate-schema" && query.schemaId) {
			API.SCHEMAS.GET_ONE(query.schemaId).then((res) => {
				if (res && !res.error) setSchema(res.data);
			});
			return setActive(true);
		}
		setActive(false);
	}, [query]);

	const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
		e.preventDefault();

		schema &&
			API.SCHEMAS.DUPLICATE(schema.id).then((res) => {
				if (res && !res.error) {
					dispatch(add(res.data));
					routerPush(pathname);
				}
			});
	};

	if (active)
		return (
			<motion.div
				initial={{ backgroundColor: "#00000000" }}
				animate={{ backgroundColor: "#00000055" }}
				className="z-50 flex items-center justify-center fixed top-0 left-0 right-0 min-h-screen"
			>
				<motion.div
					initial={{ opacity: 0.1, height: "10rem" }}
					animate={{ opacity: 1, height: "12rem" }}
					className="bg-white px-5 py-4 basis-80 rounded-lg flex flex-col justify-between"
				>
					{schema ? (
						<>
							<h2 className="text-lg">Duplicate Schema</h2>
							<div>
								About to duplicate{" "}
								<span className="font-bold text-cyan-700">
									{schema.name}
								</span>
							</div>
							<form onSubmit={handleSubmit}>
								<FormConclusion
									submittable={true}
									type="chill"
									text={{
										save: "Duplicate",
									}}
								/>
							</form>
						</>
					) : (
						<></>
					)}
				</motion.div>
			</motion.div>
		);

	return <></>;
};

export default DuplicateSchema;

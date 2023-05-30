import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import FormConclusion from "../../formComponents/FormConclusion";
import API from "../../../lib/API";
import { motion } from "framer-motion";
import { useAppDispatch } from "../../../app/hooks";
import { remove } from "../../../slices/schemaSlice";
import { UnarchivableSchemaResponse } from "../../../lib/API/Schemas";
import Link from "next/link";

const RemoveSchema = () => {
	const dispatch = useAppDispatch();
	const { query, pathname, push: routerPush } = useRouter();
	const [active, setActive] = useState<boolean>(false);
	const [schema, setSchema] = useState<ISchema>();
	const [unarchivableResponse, setUnarchivableRes] =
		useState<UnarchivableSchemaResponse[]>();

	useEffect(() => {
		if (query.form === "remove-schema" && query.schemaId) {
			API.SCHEMAS.GET_ONE(query.schemaId).then((res) => {
				if (res && !res.error) setSchema(res.data);
			});
			setUnarchivableRes(undefined);
			return setActive(true);
		}
		setActive(false);
	}, [query]);

	const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
		e.preventDefault();

		schema &&
			API.SCHEMAS.DELETE(schema.id).then((res) => {
				if (res && !res.error) {
					dispatch(remove(schema));
					routerPush(pathname);
				} else if (res && res.error) {
					setUnarchivableRes(res.data!);
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
					animate={{
						opacity: 1,
						height: unarchivableResponse ? "auto" : "12rem",
					}}
					className="bg-white px-5 py-4 basis-80 rounded-lg flex flex-col justify-between max-h-[75vh] overflow-y-auto"
				>
					{unarchivableResponse ? (
						<>
							<h2 className="text-lg">
								Schema has ongoing processes
							</h2>
							{unarchivableResponse.map((p) => (
								<div
									key={p.id}
									className="pl-1 border-l-2 border-solid text-sm mt-2"
								>
									<div>{p.name}</div>
									{p.units.map((u) => (
										<div
											key={u.id}
											className="pl-1 border-l-2 border-solid mt-2"
										>
											<div>{u.name}</div>
											{u.lessons.map((l) => (
												<div
													key={l.id}
													className="pl-1 border-l-2 border-solid mt-2"
												>
													<div>{l.name}</div>
													{l.learningObjectives.map(
														(lo) => (
															<div
																className="font-bold text-xs"
																key={lo.id}
															>
																{lo.name}
															</div>
														)
													)}
												</div>
											))}
										</div>
									))}
								</div>
							))}
							<div className="flex justify-center mt-8">
								<Link href={{ pathname }} className="w-1/2">
									<button className="h-10 bg-black text-white font-bold w-full">
										Okay
									</button>
								</Link>
							</div>
						</>
					) : schema ? (
						<>
							<h2 className="text-lg">Delete Schema</h2>
							<div>
								About to delete{" "}
								<span className="font-bold text-red-700">
									{schema.name}
								</span>
							</div>
							<form onSubmit={handleSubmit}>
								<FormConclusion
									submittable={true}
									type="danger"
									text={{
										save: "Archive",
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

export default RemoveSchema;

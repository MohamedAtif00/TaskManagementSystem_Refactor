import Link from "next/link";
import { useRouter } from "next/router";

type ConclusionType = "danger" | "chill";

interface Props {
	submittable: boolean;
	text?: {
		cancel?: string;
		save?: string;
	};
	type?: ConclusionType;
}

const FormConclusion = ({ submittable, text, type }: Props) => {
	const router = useRouter();

	return (
		<div className="flex justify-between gap-4 font-bold">
			<Link
				className="py-2 flex items-center justify-center border-2 border-solid border-black grow"
				href={router.pathname}
			>
				<button type="button">
					{text && text.cancel ? text.cancel : "Back"}
				</button>
			</Link>
			<button
				type="submit"
				className={`${
					type === "danger"
						? "bg-red-600"
						: type === "chill"
						? "bg-cyan-600"
						: "bg-black"
				} py-2 flex items-center justify-center grow ${
					submittable ? "text-white" : "opacity-50 text-gray-300"
				}`}
			>
				{text && text.save ? text.save : "Save"}
			</button>
		</div>
	);
};

export default FormConclusion;

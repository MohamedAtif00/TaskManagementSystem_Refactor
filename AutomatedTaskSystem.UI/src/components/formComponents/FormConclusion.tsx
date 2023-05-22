import Link from "next/link";
import { useRouter } from "next/router";

interface Props {
	submittable: boolean;
	text?: {
		cancel?: string;
		save?: string;
	};
	danger?: boolean;
}

const FormConclusion = ({ submittable, text, danger }: Props) => {
	const router = useRouter();

	return (
		<div className="flex justify-between gap-4 font-bold mt-5">
			<Link href={router.pathname}>
				<button
					type="button"
					className="py-2 flex items-center justify-center border-2 border-solid border-black grow"
				>
					{text && text.cancel ? text.cancel : "Back"}
				</button>
			</Link>
			<button
				type="submit"
				className={`${
					danger ? "bg-red-600" : "bg-black"
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

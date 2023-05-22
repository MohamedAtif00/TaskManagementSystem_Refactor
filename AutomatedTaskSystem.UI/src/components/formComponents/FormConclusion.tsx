import Link from "next/link";
import { useRouter } from "next/router";

interface Props {
	submittable: boolean;
}

const FormConclusion = ({ submittable }: Props) => {
	const router = useRouter();

	return (
		<div className="flex justify-between gap-4 font-bold mt-5">
			<Link href={router.pathname}>
				<button
					type="button"
					className="py-2 flex items-center justify-center border-2 border-solid border-black grow"
				>
					Back
				</button>
			</Link>
			<button
				type="submit"
				className={`bg-black py-2 flex items-center justify-center grow ${
					submittable ? "text-white" : "opacity-50 text-gray-300"
				}`}
			>
				Save
			</button>
		</div>
	);
};

export default FormConclusion;

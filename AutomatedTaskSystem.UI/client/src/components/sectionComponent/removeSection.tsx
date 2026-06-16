import { useRouter } from "next/router";
import API from "../../lib/API";
import { motion } from "framer-motion";

interface Props {
	section: { id: number; name: string };
	onClose: () => void;
}

const RemoveSection = ({ section, onClose }: Props) => {
	const router = useRouter();

	const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
		e.preventDefault();
		API.RESOURCES.SECTIONS.DELETE({ id: section.id }).then((res) => {
			if (res && !res.error) {
				router.push("/resources/sections");
			}
		});
	};

	return (
		<div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
			<motion.div
				initial={{ opacity: 0, y: -20 }}
				animate={{ opacity: 1, y: 0 }}
				className="bg-white px-6 py-5 rounded-lg w-full max-w-sm"
			>
				<h2 className="text-lg font-bold mb-2">Delete section</h2>
				<p className="mb-6">
					About to delete{" "}
					<span className="font-bold text-red-700">{section.name}</span>.
					This action cannot be undone.
				</p>
				<form onSubmit={handleSubmit}>
					<div className="flex justify-between gap-3">
						<button
							type="button"
							onClick={onClose}
							className="flex-1 bg-gray-300 text-gray-800 px-4 py-2 rounded hover:bg-gray-400"
						>
							Cancel
						</button>
						<button
							type="submit"
							className="flex-1 bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700"
						>
							Delete
						</button>
					</div>
				</form>
			</motion.div>
		</div>
	);
};

export default RemoveSection;

import Link from "next/link";
import { UrlObject } from "url";
import EditPen from "../../assets/Icons/EditPen";
import ArchiveIcon from "../../assets/Icons/Archive";
import EyeIcon from "../../assets/Icons/Eye";
import CopyIcon from "../../assets/Icons/Copy";

type TableActionType = "edit" | "archive" | "eye" | "duplicate";

interface Props {
	url: UrlObject;
	text: string;
	type: TableActionType;
}

const TableAction = ({ type, url, text }: Props) => {
	return (
		<Link
			className={`${
				type === "edit"
					? "hover:border-blue-600"
					: type === "archive"
					? "hover:border-red-600"
					: type === "duplicate"
					? "hover:border-cyan-600"
					: ""
			} bg-white shadow-lg transition-all ease-in flex gap-2 h-7 border-solid border rounded items-center group px-2 py-1 opacity-50 hover:opacity-100`}
			href={url}
		>
			{type === "edit" ? (
				<div>
					<EditPen className="group-hover:fill-blue-600 w-4 h-4 fill-black transition-all ease-in" />
				</div>
			) : type === "archive" ? (
				<div>
					<ArchiveIcon className="group-hover:stroke-red-600 w-4 h-4 stroke-black transition-all ease-in" />
				</div>
			) : type === "eye" ? (
				<div>
					<EyeIcon className="w-4 h-4 stroke-black transition-all ease-in" />
				</div>
			) : type === "duplicate" ? (
				<div>
					<CopyIcon className="group-hover:stroke-cyan-600 w-4 h-4 stroke-black transition-all ease-in" />
				</div>
			) : (
				<></>
			)}
			<div
				className={`${
					type === "edit"
						? "group-hover:text-blue-600"
						: type === "archive"
						? "group-hover:text-red-600"
						: type === "duplicate"
						? "group-hover:text-cyan-600"
						: ""
				}
				transition-all ease-in group-hover:underline cursor-pointer font-medium`}
			>
				{text}
			</div>
		</Link>
	);
};

export default TableAction;

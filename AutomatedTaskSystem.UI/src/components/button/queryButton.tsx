import Link from "next/link";
import { UrlObject } from 'url';

type props =
	| {
		text: string;
		iconLeft?: false;
		iconRight?: false;
		url: UrlObject
	}
	| {
		text: string;
		iconLeft: boolean;
		iconRight: boolean;
		icon: JSX.Element;
		url: UrlObject
	};

const QueryButton = (props: props) => {
	return (
		<Link href={props.url}>
			<a className="text-base gap-2 font-normal px-3 rounded bg-blue-500 text-white flex items-center justify-center py-1">
				{props.iconLeft ? props.icon : ""}
				<div>{props.text}</div>
				{props.iconRight ? props.iconRight : ""}
			</a>
		</Link>
	);
};

export default QueryButton;

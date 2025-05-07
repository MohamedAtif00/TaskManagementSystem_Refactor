import { useRouter } from "next/router"
import { useEffect, useState } from "react"

const useTaskPathHandler = ({type}:{type:string}) => {
	const router = useRouter();
	const [isBoard, setIsBoard] = useState(false);

	useEffect(() => {
		setIsBoard(router.pathname.includes("board"));
	})

	const projectId = type == "tasks"? router.query.projectId :router.query.sprintId;

	return () => isBoard ? `/${type}/${projectId}/board` : `/${type}/${projectId}/sheet`;

}

export default useTaskPathHandler;

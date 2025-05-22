import { useRouter } from "next/router"
import { useEffect, useState } from "react"

const useTaskPathHandler = () => {
	const router = useRouter();
	const [isBoard, setIsBoard] = useState(false);

	useEffect(() => {
		setIsBoard(router.pathname.includes("board"));
	},[router.pathname])

	const projectId = router.query.projectId;

	return () => isBoard ? `/tasks/${projectId}/board` : `/tasks/${projectId}/sheet`;

}

export default useTaskPathHandler;

import { useRouter } from "next/router"
import { useEffect, useState } from "react"

const useTaskPathHandler = ({type}:{type:string}) => {
    const router = useRouter();
    const [isBoard, setIsBoard] = useState(false);

    useEffect(() => {
        setIsBoard(router.pathname.includes("board"));
    },[router.pathname])

    const projectId = type === "tasks" ? router.query.projectId : router.query.sprintId;
    // Get learningObjectId from the router's query
    const learningObjectId = router.query.learningObjectId;

    return () => {
        if (type === "sprints" && learningObjectId) {
            return isBoard ? `/${type}/${projectId}/${learningObjectId}/board` : `/${type}/${projectId}/${learningObjectId}/sheet`;
        }else if(type === "task-sprint") {
             return `/tasks/sprint/${projectId}/board` 
        } else {
            return isBoard ? `/${type}/${projectId}/board` : `/${type}/${projectId}/sheet`;
        }
    };
}

export default useTaskPathHandler;


// const useTaskPathHandlerForSprint = ({type}:{type:string}) => {
// 	const router = useRouter();
// 	const [isBoard, setIsBoard] = useState(false);

// 	useEffect(() => {
// 		setIsBoard(router.pathname.includes("board"));
// 	},[router.pathname])

// 	const projectId = type == "tasks"? router.query.projectId :router.query.sprintId;

// 	return () => isBoard ? `/${type}//${projectId}/board` : `/${type}/${projectId}/sheet`;

// }

// export default useTaskPathHandlerForSprint;

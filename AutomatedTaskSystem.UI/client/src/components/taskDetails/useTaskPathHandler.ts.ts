import { useRouter } from "next/router";
import { useEffect, useState } from "react";

const useTaskPathHandler = ({ type }: { type: string }) => {
  const router = useRouter();
  const [isBoard, setIsBoard] = useState(false);

  useEffect(() => {
    setIsBoard(router.pathname.includes("board"));
  }, [router.pathname]);

  // When type is "tasks", this will be projectId.
  // When type is "sprints" or "task-sprint", this will be sprintId.
  const dynamicId = type === "tasks" ? router.query.projectId : router.query.sprintId;

  // Get learningObjectId from the router's query
  const learningObjectId = router.query.learningObjectId;

  // This returned function is how you'll get the path
  return () => {
    if (type === "sprints" && learningObjectId) {
      return isBoard ? `/${type}/${dynamicId}/${learningObjectId}/board` : `/${type}/${dynamicId}/${learningObjectId}/sheet`;
    } else if (type === "task-sprint") {
      return `/tasks/sprint/${dynamicId}/board`; 
    } else {
      return isBoard ? `/${type}/${dynamicId}/board` : `/${type}/${dynamicId}/sheet`;
    }
  };
};

export default useTaskPathHandler;
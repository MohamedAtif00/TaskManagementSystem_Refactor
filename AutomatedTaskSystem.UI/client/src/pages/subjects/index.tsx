import { useEffect } from "react";
import { useRouter } from "next/router";
import Loader from "../../components/loader";

/** Legacy entry: subjects list is reached via Project → Year → Term → Subjects. */
const SubjectsRedirect = () => {
    const router = useRouter();

    useEffect(() => {
        router.replace("/projects");
    }, [router]);

    return (
        <div className="flex items-center justify-center mx-auto h-full">
            <Loader />
        </div>
    );
};

export default SubjectsRedirect;

import { motion } from "framer-motion";
import { useRouter } from "next/router";

const JumpForm = () => {
    const router = useRouter();

    if (router.query.form !== "jump") return <></>;

    return (
        <motion.div
            initial={{
                opacity: 0,
            }}
            animate={{ opacity: 1 }}
            className="fixed z-50 top-0 bottom-0 left-0 right-0 backdrop-blur-sm bg-black/5 flex justify-center items-center"
        >
            <motion.div
                initial={{ scale: 0 }}
                animate={{ scale: 1 }}
                className="p-8 bg-white shadow border-2 border-neutral-700 border-solid rounded"
            ></motion.div>
        </motion.div>
    );
};

export default JumpForm;

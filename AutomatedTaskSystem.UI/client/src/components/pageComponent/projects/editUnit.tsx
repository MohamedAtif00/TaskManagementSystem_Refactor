import InputTextField from "../../formComponents/InputTextField";
import { useState } from "react";
import FormConclusion from "../../formComponents/FormConclusion";
import API from "../../../lib/API";
import { motion } from "framer-motion";

interface Props {
    id: number;
    name: string;
    onSubmit: (id: number, name: string) => void;
    projectId: number;
}

const EditUnit: React.FC<Props> = (props) => {
    const [name, setName] = useState(props.name);
    const [err, setErr] = useState("");

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (name === "") {
            return setErr("Name cannot be empty");
        }
        API.PROJECTS.UNITS.EDIT({
            id: props.id,
            name,
        }).then((res) => {
            if (res && !res.error) props.onSubmit(props.id, name);
        });
    };

    return (
        <motion.div
            initial={{ backgroundColor: "#00000000" }}
            animate={{ backgroundColor: "#00000055", height: "auto" }}
            className="z-50 flex items-center justify-center fixed top-0 left-0 right-0 min-h-screen"
        >
            <motion.div
                initial={{ opacity: 0.1 }}
                animate={{ opacity: 1 }}
                className="bg-white px-5 py-4 basis-80 rounded-lg"
            >
                <h2 className="text-lg mb-5">Edit Unit</h2>
                <form onSubmit={handleSubmit} className="flex flex-col gap-8">
                    <div className="flex flex-col gap-2">
                        <div className="text-red-600">{err}</div>
                        <InputTextField
                            label="Name"
                            value={name}
                            handleChange={setName}
                        />
                    </div>
                    <FormConclusion
                        pathname={`/projects/${props.projectId}`}
                        submittable={true}
                    />
                </form>
            </motion.div>
        </motion.div>
    );
};

export default EditUnit;

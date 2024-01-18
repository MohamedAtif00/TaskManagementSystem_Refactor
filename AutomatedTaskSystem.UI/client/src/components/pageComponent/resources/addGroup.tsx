import { motion } from "framer-motion";
import { useEffect, useState } from "react";
import InputTextField from "../../formComponents/InputTextField";
import FormConclusion from "../../formComponents/FormConclusion";
import { useRouter } from "next/router";
import API from "../../../lib/API";
import ColorDropdown from "../../formComponents/ColorDropdown";
import { useAppDispatch } from "../../../app/hooks";
import { add } from "../../../slices/groupSlice";

const AddGroup = () => {
    const { query, back: routerBack } = useRouter();
    const [active, setActive] = useState(false);
    const [error, setError] = useState("");
    const [name, setName] = useState("");
    const [color, setColor] = useState<{ id: number; name: string } | null>(
        null
    );
    const dispatch = useAppDispatch();

    useEffect(() => {
        if (query.form === "add-group") return setActive(true);

        setName("");
        setColor(null);

        return setActive(false);
    }, [query]);

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        setError("");

        if (!color) return setError("Please select a color");

        API.RESOURCES.GROUPS.CREATE({
            name,
            colorCode: color.name,
        }).then((res) => {
            if (res && !res.error) {
                dispatch(add(res.data));
                routerBack();
            }
        });
    };

    if (active)
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
                    <h2 className="text-lg mb-5">Add new group</h2>
                    <form
                        onSubmit={handleSubmit}
                        className="flex flex-col gap-8"
                    >
                        <div className="flex flex-col gap-2">
                            <div className="text-red-600">{error}</div>
                            <InputTextField
                                label="Name"
                                value={name}
                                handleChange={setName}
                            />
                            <ColorDropdown
                                value={color}
                                handleChange={setColor}
                                options={[
                                    { name: "orange", id: 1 },
                                    { name: "blue", id: 2 },
                                    { name: "green", id: 3 },
                                    { name: "red", id: 4 },
                                    { name: "pink", id: 5 },
                                    { name: "black", id: 6 },
                                    { name: "cyan", id: 7 },
                                ]}
                            />
                        </div>
                        <FormConclusion
                            pathname="/resources/groups"
                            submittable={true}
                        />
                    </form>
                </motion.div>
            </motion.div>
        );

    return <></>;
};

export default AddGroup;

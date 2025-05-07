import { useEffect, useState } from "react";
import { useRouter } from "next/router";
import { motion } from "framer-motion";
import { addDays, endOfWeek, startOfWeek } from "date-fns";
import { DateRangePicker } from "react-date-range";
import { useAppSelector } from "../../app/hooks";

import 'react-date-range/dist/styles.css';
import 'react-date-range/dist/theme/default.css';
import API from "../../lib/API";




const CreateSprint = () => {
    const { query, pathname, push } = useRouter();
    const { role } = useAppSelector((s) => s.authSlice);

    const [active, setActive] = useState(false);
    const [sprintName, setSprintName] = useState('');
    const [description, setDescription] = useState('');
    const [error, setError] = useState('');
    const [state, setState] = useState([
        {
            startDate: new Date(),
            endDate: addDays(new Date(), 7),
            key: 'selection'
        }
    ]);

    

    useEffect(() => {
        if (query.form === "create-sprint") return setActive(true);
        setActive(false);
    }, [query]);

    useEffect(()=>{
        console.log(state[0]);
        
    },[state])


    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setError("");

        if (sprintName === "") return setError("Please enter a name");

        try {
            const response = await API.SPRINTS.CREATE_SPRINT({
                name: sprintName,
                description,
                startDate: state[0].startDate.toISOString(),
                endDate: state[0].endDate.toISOString()
            }) ;

            console.log(response);
            
            if (response && !response.error) {
                console.log("Sprint created:", response.message);
                push(pathname); // Close modal
            }else if(response.error){
                setError(`Error : ${response.message}`);
            } else {
                setError("Failed to create sprint. Please try again.");
            }
        } catch (err) {
            console.error(err);
            setError("An error occurred. Please try again.");
        }
    };


    const selectNextWeek = () => {
        const today = new Date();
        const start = startOfWeek(addDays(today, 7), { weekStartsOn: 1 });
        const end = endOfWeek(addDays(today, 7), { weekStartsOn: 1 });

        setState([
            {
                startDate: start,
                endDate: end,
                key: 'selection'
            }
        ]);
    };

    if (!active || (role !== 2 && role !== 0)) return null;

    return (
        <motion.div
            initial={{ backgroundColor: "#00000000" }}
            animate={{ backgroundColor: "#00000055", height: "auto" }}
            className="z-50 flex items-center justify-center fixed top-0 left-0 right-0 w-screen min-h-screen"
        >
            <motion.div
                initial={{ opacity: 0.1 }}
                animate={{ opacity: 1 }}
                className="bg-white p-6 w-full max-w-5xl max-h-[90vh] overflow-y-auto rounded-lg shadow space-y-6"
>

                <h2 className="text-2xl font-semibold text-center">Create New Sprint</h2>
                {error && <div className="text-red-600">{error}</div>}

                <form onSubmit={handleSubmit} className="space-y-4">
                    <div>
                        <label className="block text-gray-700 font-medium mb-1">Sprint Name</label>
                        <input
                            type="text"
                            value={sprintName}
                            onChange={(e) => setSprintName(e.target.value)}
                            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring focus:border-blue-500"
                            placeholder="Enter sprint name"
                        />
                    </div>

                    <div>
                        <label className="block text-gray-700 font-medium mb-1">Description</label>
                        <textarea
                            value={description}
                            onChange={(e) => setDescription(e.target.value)}
                            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring focus:border-blue-500"
                            placeholder="Enter description"
                            rows={3}
                        />  
                    </div>

                    <div className="flex justify-center">
                        <DateRangePicker
                            onChange={(item) => setState([item.selection])}
                            moveRangeOnFirstSelection={false}
                            months={2}
                            ranges={state}
                            direction="horizontal"
                        />
                    </div>

                    <div className="flex justify-between pt-4">
                        <button
                            type="button"
                            onClick={selectNextWeek}
                            className="bg-gray-600 text-white px-4 py-2 rounded hover:bg-gray-700"
                        >
                            Select Next Week
                        </button>

                        <div className="flex gap-4">
                            <button
                                type="button"
                                onClick={() => push(pathname)}
                                className="bg-red-500 text-white px-4 py-2 rounded hover:bg-red-600"
                            >
                                Cancel
                            </button>
                            <button
                                type="submit"
                                className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700"
                                
                            >
                                Create Sprint
                            </button>
                        </div>
                    </div>
                </form>
            </motion.div>
        </motion.div>
    );
};

export default CreateSprint;

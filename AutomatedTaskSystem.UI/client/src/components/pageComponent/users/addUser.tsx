import { useEffect, useState } from "react";
import InputTextField from "../../formComponents/InputTextField";
import Dropdown from "../../formComponents/DropDown";
import FormConclusion from "../../formComponents/FormConclusion";
import API from "../../../lib/API";
import { useRouter } from "next/router";
import { add } from "../../../slices/userSlice";
import { useAppDispatch } from "../../../app/hooks";
import { motion } from "framer-motion";

// Define the types for the user roles and vacation structure
type UserRole = 0 | 1 | 2 | 3;
type AccountType = 0 | 1;

interface IUser {
    id: number;
    name: string;
    role: UserRole;
    group: { id: number; name: string };
}

const AddUser = () => {
    const router = useRouter();
    const dispatch = useAppDispatch();
    const { query, pathname } = useRouter();
    const [teamLeaders, setTeamLeaders] = useState<{ id: number; name: string }[]>([]);

    
    const [formState, setFormState] = useState({
        name: "",
        code: "",
        group: null as { id: number; name: string } | null,
        role: null as UserRole | null,
        accountType: null as { id: AccountType; name: string } | null,
        email:null as string | null,
        teamleader:null as {id:number;name:string} | null,
        vacation: { annual: 0, sick: 0, emergency: 0 },
        error: "",
        done: null as { code: string; user: IUser } | null,
    });

    const [groups, setGroups] = useState<{ id: number; name: string }[]>([]);
    const [active, setActive] = useState(false);

    const roleMap = new Map<UserRole, string>([
        [0, "Project Manager"],
        [1, "Section Head"],
        [2, "Team Leader"],
        [3, "Member"],
    ]);

    useEffect(() => {
        if (query.form === "add-user") setActive(true);
        else setActive(false);
    }, [query]);

    useEffect(() => {
        API.RESOURCES.GROUPS.GET_ALL_MINI().then((res) => {
            if (res && !res.error) setGroups(res.data);
        });
    }, [active]);

    useEffect(() => {
        if (!formState.group?.id) return;
        API.RESOURCES.GROUPS.Get_Tm(formState.group?.id).then((res) => {
            if (res && !res.error) {
                setTeamLeaders(res.data); // assuming res.data is an array of {id, name}
            }
        });
    }, [formState.group]);
    
    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();

        const { name, group, role, code, accountType,email, vacation,teamleader } = formState;

        if (!name || !group || !role) {
            return setFormState((prev) => ({
                ...prev,
                error: "Please fill out all required fields",
            }));
        }

        const res = await API.RESOURCES.USERS.CREATE({
            name,
            role,
            groupId: group.id,
            hrCode: code,
            email: email ?? "",
            teamleaderId: teamleader?.id ?? null,
            accountType: accountType?.id ?? 0,
            vacation,
        });
        
        if (res && !res.error) {
            dispatch(add(res.data.user));
            setFormState({
                ...formState,
                done: res.data,
                name: "",
                code: "",
                group: null,
                role: null,
                teamleader: null,
                accountType: null,
                email: null, // Add this line
                vacation: { annual: 0, sick: 0, emergency: 0 },
                error: "",
            });
            
        }
    };

    const handleChange = (field: keyof typeof formState) => (
        value: any
    ) => {
        setFormState((prev) => ({ ...prev, [field]: value }));
    };

    // Special handling for vacation updates
    const handleVacationChange = (vacationType: string, value: number) => {
        setFormState((prev) => ({
            ...prev,
            vacation: {
                ...prev.vacation,
                [vacationType]: value,
            },
        }));
    };

    const handleTeamleaderchange = (teamleader: { id: number; name: string }) => {
        setFormState((prev) => ({
            ...prev,
            teamleader: teamleader, // Corrected lowercase "l"
        }));
    };
    

    const renderRoleName = (roleId: UserRole) => roleMap.get(roleId) || "Choose";

    if (!active) return null;

    return (
        <motion.div
            initial={{ backgroundColor: "#00000000" }}
            animate={{ backgroundColor: "#00000055", height: "auto" }}
            className="z-50 flex items-center justify-center fixed top-0 left-0 right-0 min-h-screen"
        >
            <motion.div
                initial={{ opacity: 0.1 }}
                animate={{ opacity: 1 }}
                className="bg-white px-5 py-4 rounded-lg w-[700px]"
            >
                {formState.done ? (
                    <div className="flex flex-col gap-4">
                        <h3>
                            {renderRoleName(formState.done.user.role)}{" "}
                            <span className="font-bold">{formState.done.user.name}</span> is added as an{" "}
                            <span className="font-bold">{formState.done.user.group.name}</span>
                        </h3>
                        <div>
                            <div>Code:</div>
                            <div className="font-bold text-2xl text-center">
                                {formState.done.code}
                            </div>
                        </div>
                        <div className="flex justify-center">
                            <button
                                onClick={() => {
                                    setActive(false);
                                    setFormState((prev) => ({ ...prev, done: null }));
                                    router.push(pathname);
                                }}
                                className="h-10 bg-black text-white font-bold px-4"
                            >
                                Done
                            </button>
                        </div>
                    </div>
                ) : (
                    <>
                        <h2 className="text-lg mb-5">Add User</h2>
                        <form onSubmit={handleSubmit} className="flex flex-col gap-6">
                            <div className="text-red-600">{formState.error}</div>

                            <div className="grid grid-cols-2 gap-4">
                                <InputTextField label="Name" value={formState.name} handleChange={handleChange("name")} />
                                <InputTextField label="HR Code" value={formState.code} handleChange={handleChange("code")} />
                                
                                <Dropdown
                                    label="Group"
                                    value={formState.group}
                                    options={groups}
                                    handleChange={handleChange("group")}
                                />

                                <Dropdown
                                    label="Role"
                                    value={formState.role !== null ? { id: formState.role, name: renderRoleName(formState.role) } : null}
                                    options={[
                                        { id: 0, name: "Project Manager" },
                                        { id: 1, name: "Section Head" },
                                        { id: 2, name: "Team Leader" },
                                        { id: 3, name: "Member" },
                                    ]}
                                    handleChange={(v) => handleChange("role")(v.id as UserRole)}
                                />
                                <Dropdown
                                    label="Account Type"
                                    value={formState.accountType}
                                    options={[
                                        { id: 0, name: "Enternal" },
                                        { id: 1, name: "External" },
                                    ]}
                                    handleChange={handleChange("accountType")}
                                    />
                                    <InputTextField label="Emain" value={formState.email??""} handleChange={handleChange("email")} />
                                    {
    
                                        formState.role === 3 &&
                                    
                                        <Dropdown
                                        label="Team Leader"
                                        value={formState.teamleader !== null ? { id: formState.teamleader.id, name: formState.teamleader.name } : null}
                                        options={teamLeaders}
                                        handleChange={handleTeamleaderchange}
                                    />
                                    
                                    }

                            </div>

                            {formState.accountType?.id === 0 && (
                                <div className="mt-4">
                                    <h3 className="text-md font-semibold mb-2">Vacations</h3>
                                    <div className="grid grid-cols-3 gap-4">
                                        {["annual", "sick", "emergency"].map((vacType) => (
                                            <InputTextField
                                                key={vacType}
                                                label={vacType.charAt(0).toUpperCase() + vacType.slice(1)}
                                                value={formState.vacation[vacType as "annual" | "sick" | "emergency"].toString()} // Cast `vacType` to a valid key
                                                handleChange={(val) => handleVacationChange(vacType as "annual" | "sick" | "emergency", Number(val))} // Cast `vacType` here too
                                            />
                                        ))}
                                    </div>
                                </div>
                            )}


                            <FormConclusion pathname="/resources/users" submittable={true} />
                        </form>
                    </>
                )}
            </motion.div>
        </motion.div>
    );
};

export default AddUser;

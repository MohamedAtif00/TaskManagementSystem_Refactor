import { useEffect, useState } from "react";
import InputTextField from "../../formComponents/InputTextField";
import Dropdown from "../../formComponents/DropDown";
import FormConclusion from "../../formComponents/FormConclusion";
import API from "../../../lib/API";
import { useRouter } from "next/router";
import { add } from "../../../slices/userSlice";
import { useAppDispatch } from "../../../app/hooks";
import { motion } from "framer-motion";
import { ClassNames } from "@emotion/react";
// import Permission from "../../../lib/API/Permission";
// import { IGroup, IVacation } from "../../../../app";
import { string } from "prop-types";
import { Code } from "lucide-react";
import Link from "next/link";

// Define the types for the user roles and vacation structure
// type UserRole = 0 | 1 | 2 | 3;
export type AccountType = 0 | 1;

// interface IUser {
//     id: number;
//     name: string;
//     role: UserRole;
//     group: { id: number; name: string };
// }

interface SimpleInfo{id:number,name:string}


interface IUserFormState {
    name: string;
    hrCode: string;
    // onBoard: boolean;
    archived: boolean;
    group: SimpleInfo | null;
    role: UserRole | null;
    accountType: SimpleInfo| null;
    email: string | null;
    teamleader: SimpleInfo | null;
    vacation: IVacation;
    permission: number;
    permission_MAX:number,
    workFromHome:number,
    workFromHome_MAX:number,
    error: string;
    done: { code: string; user: IUser } | null;
}

const AddUser = () => {
    const router = useRouter();
    const dispatch = useAppDispatch();
    const { query, pathname } = useRouter();
    const [teamLeaders, setTeamLeaders] = useState<{ id: number; name: string }[]>([]);
    const [error, setError] = useState("");
  const [done, setDone] = useState<{ code: string; user: IUser } | null>(
        null
    );
    
    const roleOptions = [
    { id: 0, name: "Project Manager" },
    { id: 1, name: "Section Head" },
    { id: 2, name: "Team Leader" },
    { id: 3, name: "Member" },
    {id:4 ,name:"Owner"}
];
    const [formState, setFormState] = useState<IUserFormState>({
        name: "",
        hrCode: "",
        // onBoard: true,
        archived: false,
        group: null as { id: number; name: string } | null,
        role: null as UserRole | null,
        accountType: null as { id: AccountType; name: string } | null,
        email: null as string | null,
        teamleader: null as { id: number; name: string } | null,
        vacation: {
            annual: 0,
            sick: 0,
            emergency: 0,
            annual_MAX: 0,
            emergency_MAX: 0
        },
        permission:0,
        permission_MAX:0,
        workFromHome:0,
        workFromHome_MAX:0,
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
        [4,"Owner"]
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
        API.RESOURCES.GROUPS.Get_Tm_leaders(formState.group?.id).then((res) => {
            if (res && !res.error) {
                setTeamLeaders(res.data); 
            }
        });
    }, [formState.group]);
    
    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        debugger
        e.preventDefault();
         e.stopPropagation(); // Add this to prevent event bubbling
        setError("");
        
        const { 
            name, 
            group, 
            role, 
            hrCode,
            accountType, 
            email, 
            vacation, 
            teamleader,
            permission,
            permission_MAX,
            workFromHome,
            workFromHome_MAX
        } = formState;

        if (name === null || name === undefined || name === "" ||
            group === null || group === undefined ||
            role === null || role === undefined ||
            hrCode === null || hrCode === undefined || hrCode === "")  {
            return setFormState((prev) => ({
                ...prev,
                error: "Please fill out all required fields",
            }));
        }
        
        API.RESOURCES.USERS.CREATE({
            name,
            role:role??3,
            groupId: group.id,
            hrCode,
            email: email ?? "",
            teamLeaderId: teamleader?.id ?? null,
            accountType: accountType?.id ?? 0,
            vacation,
            permission,
            permission_MAX,
            workFromHome,
            workFromHome_MAX
        }).then(res=>{


            if (res && !res.error) {
                // dispatch(add(res.data.user));
                setFormState({
                    ...formState,
                    name: "",
                    hrCode: "",
                    group: null,
                    role: null,
                    teamleader: null,
                    accountType: null,
                    email: null,
                    // onBoard: true,
                    archived: false,
                    vacation: {
                        annual: 0,
                        sick: 0,
                        emergency: 0,
                        annual_MAX: 0,
                        emergency_MAX: 0
                    },
                    permission,
                    permission_MAX,
                    workFromHome,
                    workFromHome_MAX
                    ,
                    error: "",
                });
               setDone(res.data);
                dispatch(add(res.data.user));
                // router.push(pathname);
            }
        })
        
    };

    const handleChange = (field: keyof typeof formState) => (value: any) => {
        debugger
        if (field === 'role') {
            // Clear teamleader when changing to higher role
            const newTeamleader = (value === 0 || value === 1 || value === 2) ? null : formState.teamleader;
            setFormState(prev => ({ 
                ...prev, 
                [field]: value,
                teamleader: newTeamleader
            }));
            
        } else {
            setFormState(prev => ({ ...prev, [field]: value }));
        }
    };

    const handlePermissionChange = (value: typeof formState['permission']) => {
        setFormState((prev) => ({
            ...prev,
            permission: value
        }));
    };

    const handlePermissionMaxChange = (value: typeof formState['permission_MAX']) => {
        setFormState((prev) => ({
            ...prev,
            permission_MAX: value
        }));
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

      const handleWorkFromHomeChange = (field: 'workFromHome_Used' | 'workFromHome_MAX', value: number) => {
        setFormState((prev) => ({
            ...prev,
            [field]: value
        }));
    };

    const handleTeamleaderchange = (teamleader: { id: number; name: string }) => {
        setFormState((prev) => ({
            ...prev,
            teamleader: teamleader, // Corrected lowercase "l"
        }));
        
    };

    const shouldShowTeamLeader = (role: UserRole | null) => {
    // Show only for Member role (3) or if role is not selected yet (null)
        return role === 3 || role === null;
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
            className="bg-white px-5 py-4 rounded-lg w-[700px] max-h-[90vh] flex flex-col" // Added max-h and flex-col
        >
            {done ? (
                        <div className="flex flex-col gap-4">
                            <h3>
                                {done.user.role === 0
                                    ? "Project Manager"
                                    : done.user.role === 1
                                    ? "Section Head"
                                    : done.user.role === 2
                                    ? "Team Leader"
                                    : "Member"}{" "}
                                <span className="font-bold">
                                    {done.user.name}
                                </span>{" "}
                                is added as an{" "}
                                <span className="font-bold">
                                    {done.user.group?.name}
                                </span>
                            </h3>
                            <div>
                                <div>Code:</div>
                                <div className="font-bold text-2xl text-center">
                                    {done.code}
                                </div>
                            </div>
                            <div className="flex justify-center">
                                <Link href={{ pathname }}>
                                    <button onClick={()=>setDone(null)} className="h-10 bg-black text-white font-bold px-4">
                                        Done
                                    </button>
                                </Link>
                            </div>
                        </div>
            ) : (
                <>
                    <h2 className="text-lg mb-4">Add User</h2> {/* Added mb-4 for spacing */}
                    <div className="overflow-y-auto flex-grow"> {/* Scrollable container */}
                        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                            <div className="text-red-600">{formState.error}</div>

                            <div className="grid grid-cols-2 gap-4">
                                <InputTextField label="Name" value={formState.name} handleChange={handleChange("name")} />
                                <InputTextField label="HR Code" value={formState.hrCode} handleChange={handleChange("hrCode")} />
                                <InputTextField label="Email" value={formState.email || ""} handleChange={handleChange("email")} />
                                
                                <Dropdown
                                    label="Group"
                                    value={formState.group}
                                    options={groups}
                                    handleChange={handleChange("group")}
                                />

                                <Dropdown
                                    label="Role"
                                    value={formState.role !== null ? 
                                        roleOptions.find(option => option.id === formState.role) || null 
                                        : null
                                    }
                                    options={roleOptions}
                                    handleChange={(selected) => handleChange("role")(selected?.id ?? null)}
                                />

                                <Dropdown
                                    label="Account Type"
                                    value={formState.accountType}
                                    options={[
                                        { id: 0, name: "Internal" },
                                        { id: 1, name: "External" }
                                    ]}
                                    handleChange={handleChange("accountType")}
                                />

                                {formState.group && shouldShowTeamLeader(formState.role) && (
                                    <Dropdown
                                        label="Team Leader"
                                        value={formState.teamleader}
                                        options={teamLeaders}
                                        handleChange={handleTeamleaderchange}
                                    />
                                )}
                            </div>

                            {formState.accountType?.id === 0 && (
                                <div className="mt-4">
                                    <h3 className="text-md font-semibold mb-4">Vacations</h3>
                                    
                                    {/* Maximum Values Section */}
                                    <div className="mb-6">
                                        <h4 className="text-sm font-medium mb-2">Maximum Values</h4>
                                        <div className="grid grid-cols-2 gap-4">
                                            <InputTextField
                                                label="Annual"
                                                value={formState.vacation.annual_MAX.toString()}
                                                handleChange={(val) => handleVacationChange("annual_MAX", Number(val))}
                                            />
                                            <InputTextField
                                                label="Emergency"
                                                value={formState.vacation.emergency_MAX.toString()}
                                                handleChange={(val) => handleVacationChange("emergency_MAX", Number(val))}
                                            />
                                        </div>
                                    </div>

                                    {/* Current Values Section */}
                                    <div className="mb-6">
                                        <h4 className="text-sm font-medium mb-2">Used Values</h4>
                                        <div className="grid grid-cols-3 gap-4">
                                            <InputTextField
                                                label="Annual"
                                                value={formState.vacation.annual.toString()}
                                                handleChange={(val) => handleVacationChange("annual", Number(val))}
                                            />
                                            <InputTextField
                                                label="Sick"
                                                value={formState.vacation.sick.toString()}
                                                handleChange={(val) => handleVacationChange("sick", Number(val))}
                                            />
                                            <InputTextField
                                                label="Emergency"
                                                value={formState.vacation.emergency.toString()}
                                                handleChange={(val) => handleVacationChange("emergency", Number(val))}
                                            />
                                        </div>
                                    </div>

                                    {/* Permission Section */}
                                    {/* <div>
                                        <h4 className="text-sm font-medium mb-2">Permission</h4>
                                        <div className="grid grid-cols-1 gap-4">
                                            <InputTextField
                                                label="Permission"
                                                value={formState.permission?.toString() || "0"}
                                                handleChange={(val) => handlePermissionChange(Number(val))}
                                            />
                                        </div>
                                    </div> */}
                                    <div className="mb-6">
                                        <h4 className="text-sm font-medium mb-2">Permissions</h4>
                                        <div className="grid grid-cols-2 gap-4">
                                            <InputTextField
                                                label="Used Permissions"
                                                value={formState.permission.toString()??'0'}
                                                 handleChange={(val) => handlePermissionChange(Number(val))}
                                            />
                                            <InputTextField
                                                label="Max Permissions Allowed"
                                                value={formState.permission_MAX.toString()??'0'}
                                                handleChange={(val) => handlePermissionMaxChange(Number(val))}
                                            />
                                        </div>
                                    </div>

                                     <div className="mb-6">
                                            <h4 className="text-sm font-medium mb-2">Work From Home</h4>
                                            <div className="grid grid-cols-2 gap-4">
                                                <InputTextField
                                                    label="Used WFH Days"
                                                    value={formState.workFromHome.toString()}
                                                    handleChange={(val) => handleWorkFromHomeChange("workFromHome_Used", Number(val))}
                                                />
                                                <InputTextField
                                                    label="Max WFH Days Allowed"
                                                    value={formState.workFromHome_MAX.toString()}
                                                    handleChange={(val) => handleWorkFromHomeChange("workFromHome_MAX", Number(val))}
                                                />
                                            </div>
                                        </div>

                                </div>
                            )}
                            <div className="mt-auto pt-4"> {/* FormConclusion at the bottom */}
                                <FormConclusion pathname="/resources/users" submittable={true} />
                            </div>
                        </form>
                    </div>
                </>
            )}
        </motion.div>
    </motion.div>
);
};

export default AddUser;

import { useRouter } from "next/router";
import InputTextField from "../../formComponents/InputTextField";
import { useEffect, useState } from "react";
import FormConclusion from "../../formComponents/FormConclusion";
import Dropdown from "../../formComponents/DropDown";
import API from "../../../lib/API";
import { edit } from "../../../slices/userSlice";
import { motion } from "framer-motion";
import { useAppDispatch } from "../../../app/hooks";
import * as z from 'zod';

type UserRole = 0 | 1 | 2 | 3;
type AccountType = 0 | 1;

const phoneSchema = z.object({
  phone: z.string()
    .min(11, "Phone number must be 11 digits")
    .max(11, "Phone number must be 11 digits")
    .regex(/^01[0-2|5]{1}[0-9]{8}$/, 
      "Invalid Egyptian phone number. Must start with 010, 011, 012, or 015 followed by 8 digits")
});

type PhoneFormData = z.infer<typeof phoneSchema>;
interface IVacation {
    annual: number;
    sick: number;
    emergency: number;
    annual_MAX: number;
    emergency_MAX: number;
}

interface SimpleInfo {
    id: number;
    name: string;
}

const ACCOUNT_TYPE_OPTIONS = [
    { id: 0, name: "Internal" },
    { id: 1, name: "External" }
];

const EditUser = () => {
    const dispatch = useAppDispatch();
    const { query, push: routerPush, pathname } = useRouter();

    const [active, setActive] = useState<boolean>(false);
    const [name, setName] = useState("");
    const [hrCode, setHrCode] = useState("");
    const [email, setEmail] = useState("");
    const [onBoard, setOnBoard] = useState(true);
    const [archived, setArchived] = useState(false);
    const [group, setGroup] = useState<SimpleInfo | null>(null);
    const [groups, setGroups] = useState<SimpleInfo[]>([]);
    const [role, setRole] = useState<number | null>(null);
    const [accountType, setAccountType] = useState<{ id: AccountType; name: string } | null>(null);
    const [teamleader, setTeamleader] = useState<SimpleInfo | null>(null);
    const [teamleaderId, setTeamleaderId] = useState<number | null>(null);
    const [teamLeaders, setTeamLeaders] = useState<SimpleInfo[]>([]);
    const [title,setTite] = useState<string >("")
    const [phone,setPhone] = useState<string >("") 
    const [permissionObj, setPermission] = useState<{current:number,max:number} | null>(null);
    const [vacation, setVacation] = useState<IVacation>({
        annual: 0,
        sick: 0,
        emergency: 0,
        annual_MAX: 0,
        emergency_MAX: 0,
    });
    const [error, setError] = useState("");

    const roleOptions = [
        { id: 0, name: "Project Manager" },
        { id: 1, name: "Section Head" },
        { id: 2, name: "Team Leader" },
        { id: 3, name: "Member" },
    ];

      const handlePhoneChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = e.target.value;
        // Basic phone number formatting
        const formatted = value
        .replace(/\D/g, '') // Remove all non-digit characters
        .replace(/^(\d{3})(\d)/, '($1) $2') // Add parentheses for US format
        .replace(/^(\d{3})(\d{3})(\d)/, '($1) $2-$3');
        setPhone(formatted);
    };

    useEffect(() => {
        if (query.form === "edit-user" && query.userId) {
            API.RESOURCES.USERS.GET_ONE(query.userId.toString()).then((res) => {
                if (res && !res.error) {
                    const { name, hrCode, email, onBoard, archived, group, role, 
                            accountType, teamleader,teamleaderId, vacation, permission,permission_MAX ,title,phone} = res.data;

                            console.log(res.data);
                            
                    setName(name);
                    setHrCode(hrCode || "");
                    setEmail(email || "");
                    setOnBoard(onBoard);
                    setArchived(archived);
                    setGroup(group?? null);
                    setPermission({current:permission,max:permission_MAX} );
                    setTite(title)
                    setPhone(phone)
                    setAccountType(accountType == "Internal"?{id:0,name:"Internal"}:{id:1,name:"External"})
                    debugger
                    // if (accountType === 0 || accountType === 1) {
                    //     setAccountType({
                    //         id: accountType,
                    //         name: accountType === 0 ? "Internal" : "External"
                    //     });
                    // } else {
                    //     setAccountType(null);
                    // }
                    
                    setVacation({
                        annual: vacation?.annual || 0,
                        sick: vacation?.sick || 0,
                        emergency: vacation?.emergency || 0,
                        annual_MAX: vacation?.annual_MAX || 0,
                        emergency_MAX: vacation?.emergency_MAX || 0,
                    });

                    setRole(role);
                    setTeamleader(teamleader?? null);
                    setTeamleaderId(teamleaderId??null)
                }
            });
            return setActive(true);
        }
        setActive(false);
    }, [query]);

    useEffect(() => {
        if (active) {
            API.RESOURCES.GROUPS.GET_ALL_MINI().then((res) => {
                if (res && !res.error) setGroups(res.data);
            });
        }
    }, [active]);

    useEffect(() => {
        if (group?.id) {
            API.RESOURCES.GROUPS.Get_Tm_leaders(group.id).then((res) => {
                debugger
                if (res && !res.error)
                {
                    setTeamLeaders(res.data);
                    if(teamleaderId)
                        setTeamleader(res.data.find(x => x.id == teamleaderId) ?? null);
                }
            });
        }
    }, [group]);

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        e.stopPropagation()
        setError("");
        console.log("heello");
        

        if (!name) return setError("Please enter name");
        if (!group) return setError("Please select a group");
        if (!role) return setError("Please select a role");

        try {
            const res = await API.RESOURCES.USERS.EDIT({
                id: query.userId!.toString(),
                name,
                hrCode,
                email,
                onBoard,
                archived,
                groupId: group.id,
                role,
                accountType: accountType?.id ?? 0,
                teamLeaderId: shouldShowTeamLeader(role) ? teamleader?.id ?? null : null,
                title,
                phone,
                vacation,
                permission:permissionObj?.current??0,
                permission_MAX:permissionObj?.max??0
            });

            if (res?.data) {
                dispatch(edit(res.data));
                routerPush(pathname);
            } else if (res?.error) {
                setError(res.message || "Failed to update user");
            }
        } catch (error) {
            setError("An unexpected error occurred");
            console.error("Update error:", error);
        }
    };

    const handleVacationChange = (field: keyof IVacation, value: string) => {
        setVacation(prev => ({
            ...prev,
            [field]: Number(value)
        }));
    };

    const shouldShowTeamLeader = (role: number | null) => {
        return role === 3 || role === null;
    };

    if (!active) return <></>;

    return (
        <motion.div
            initial={{ backgroundColor: "#00000000" }}
            animate={{ backgroundColor: "#00000055", height: "auto" }}
            className="z-50 flex items-center justify-center fixed top-0 left-0 right-0 min-h-screen"
        >
            <motion.div
                initial={{ opacity: 0.1 }}
                animate={{ opacity: 1 }}
                className="bg-white px-5 py-4 rounded-lg w-[700px] max-h-[90vh] flex flex-col"
            >
                <h2 className="text-lg mb-4">Edit User</h2>
                <div className="overflow-y-auto flex-grow">
                    <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                        <div className="text-red-600">{error}</div>

                        <div className="grid grid-cols-2 gap-4">
                            <InputTextField label="Name" value={name} handleChange={setName} />
                            <InputTextField label="HR Code" value={hrCode} handleChange={setHrCode} />
                            <InputTextField label="Email" value={email} handleChange={setEmail} />
                            <InputTextField label="Title" value={title} handleChange={setTite} />
                            {/* <InputTextField label="Phone" value={phone} handleChange={setPhone} /> */}
                            <div>
                                <label htmlFor="phone" className="block text-sm font-medium text-gray-700">
                                    Phone Number
                                </label>
                                <input
                                    id="phone"
                                    type="tel"
                                    value={phone}
                                    onChange={handlePhoneChange}
                                    placeholder="(123) 456-7890"
                                    className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                                />
                                </div>


                            <Dropdown
                                label="Group"
                                value={group}
                                options={groups}
                                handleChange={setGroup}
                            />

                            <Dropdown
                                label="Role"
                                value={role !== null ? 
                                    roleOptions.find(option => option.id === role) || null 
                                    : null
                                }
                                options={roleOptions}
                                handleChange={(selected) => setRole(selected?.id ?? null)}
                            />

                            <Dropdown
                                label="Account Type"
                                value={accountType}
                                options={[
                                    { id: 0, name: "Internal" },
                                    { id: 1, name: "External" }
                                ]}
                                
                                handleChange={setAccountType as (v: { id: number; name: string }) => void}
                            />

                            {group && shouldShowTeamLeader(role) && (
                                <Dropdown
                                    label="Team Leader"
                                    value={teamleader}
                                    options={teamLeaders}
                                    handleChange={setTeamleader}
                                />
                            )}
                        </div>

                        {accountType?.id === 0 && (
                            <div className="mt-4">
                                <h3 className="text-md font-semibold mb-4">Vacations</h3>
                                
                                <div className="mb-6">
                                    <h4 className="text-sm font-medium mb-2">Maximum Values</h4>
                                    <div className="grid grid-cols-2 gap-4">
                                        <InputTextField
                                            label="Annual"
                                            value={vacation.annual_MAX.toString()}
                                            handleChange={(val) => handleVacationChange("annual_MAX", val)}
                                        />
                                        <InputTextField
                                            label="Emergency"
                                            value={vacation.emergency_MAX.toString()}
                                            handleChange={(val) => handleVacationChange("emergency_MAX", val)}
                                        />
                                    </div>
                                </div>

                                <div className="mb-6">
                                    <h4 className="text-sm font-medium mb-2">Current Values</h4>
                                    <div className="grid grid-cols-3 gap-4">
                                        <InputTextField
                                            label="Annual"
                                            value={vacation.annual.toString()}
                                            handleChange={(val) => handleVacationChange("annual", val)}
                                        />
                                        <InputTextField
                                            label="Sick"
                                            value={vacation.sick.toString()}
                                            handleChange={(val) => handleVacationChange("sick", val)}
                                        />
                                        <InputTextField
                                            label="Emergency"
                                            value={vacation.emergency.toString()}
                                            handleChange={(val) => handleVacationChange("emergency", val)}
                                        />
                                    </div>
                                </div>

                                <div className="mb-6">
                                    <h4 className="text-sm font-medium mb-2">Permissions</h4>
                                    <div className="grid grid-cols-2 gap-4">
                                        <InputTextField
                                            label="Current Permissions"
                                            value={permissionObj?.current.toString()??'0'}
                                            handleChange={(val) => setPermission(prev => ({
                                                current: Number(val),
                                                max: prev?.max??0  // Always include max to maintain the type
                                            }))}
                                        />
                                        <InputTextField
                                            label="Max Permissions Allowed"
                                            value={permissionObj?.max.toString()??'0'}
                                            handleChange={(val) => setPermission(prev => ({
                                                current: prev?.current??0,  // Always include current to maintain the type
                                                max: Number(val)
                                            }))}
                                        />
                                    </div>
                                </div>
                            </div>
                        )}

                        <div className="flex gap-4 mt-4">
                            <div className="flex items-center">
                                <input
                                    type="checkbox"
                                    id="onBoard"
                                    checked={onBoard}
                                    onChange={(e) => setOnBoard(e.target.checked)}
                                    className="mr-2"
                                />
                                <label htmlFor="onBoard">On Board</label>
                            </div>
                            <div className="flex items-center">
                                <input
                                    type="checkbox"
                                    id="archived"
                                    checked={archived}
                                    onChange={(e) => setArchived(e.target.checked)}
                                    className="mr-2"
                                />
                                <label htmlFor="archived">Archived</label>
                            </div>
                        </div>
                        <div className="mt-auto pt-4">
                            <FormConclusion pathname="/resources/users" submittable={true} />
                        </div>
                    </form>
                </div>
            </motion.div>
        </motion.div>
    );
};

export default EditUser;
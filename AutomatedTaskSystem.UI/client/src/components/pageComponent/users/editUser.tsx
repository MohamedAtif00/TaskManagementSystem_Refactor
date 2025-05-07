import { useRouter } from "next/router";
import InputTextField from "../../formComponents/InputTextField";
import { useEffect, useState } from "react";
import FormConclusion from "../../formComponents/FormConclusion";
import Dropdown from "../../formComponents/DropDown";
import API from "../../../lib/API";
import { edit } from "../../../slices/userSlice";
import { motion } from "framer-motion";
import { useAppDispatch } from "../../../app/hooks";

const EditUser = () => {
    const dispatch = useAppDispatch();
    const { query, push: routerPush, pathname } = useRouter();

    const [active, setActive] = useState<boolean>(false);
    const [name, setName] = useState("");
    const [code, setCode] = useState("");
    const [hrCode, setHrCode] = useState(""); // <-- Add hrCode state
    const [group, setGroup] = useState<{ id: number; name: string } | null>(null);
    const [groups, setGroups] = useState<{ id: number; name: string }[]>([]);
    const [role, setRole] = useState<{ id: UserRole; name: string } | null>(null);
    const [accountType, setAccountType] = useState<{ id: AccountType; name: string } | null>(null);
    const [vacation, setVacation] = useState<IVacation>({
        annual: 0,
        sick: 0,
        emergency: 0,
    });
    const [error, setError] = useState("");

    useEffect(() => {
        if (query.form === "edit-user" && query.userId) {
            API.RESOURCES.USERS.GET_ONE(query.userId.toString()).then((res) => {
                if (res && !res.error) {
                    const { role, code, group, accountType, vacation, hrCode } = res.data;

                    setName(res.data.name);
                    setCode(code || "");
                    setHrCode(hrCode || ""); // <-- Set hrCode value
                    setGroup(res.data.group);
                    if (accountType === 0 || accountType === 1) {
                        setAccountType({
                            id: accountType,
                            name: accountType === 0 ? "Internal" : "External"
                        });
                    } else {
                        setAccountType(null);
                    }

                    setVacation({
                        annual: vacation?.annual || 0,
                        sick: vacation?.sick || 0,
                        emergency: vacation?.emergency || 0,
                    });

                    setRole(
                        role === 0
                            ? { id: role, name: "Project Manager" }
                            : role === 1
                            ? { id: role, name: "Section Head" }
                            : role === 2
                            ? { id: role, name: "Team Leader" }
                            : { id: role, name: "Member" }
                    );
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

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setError("");

        if (!name) return setError("Please enter name");
        if (!group) return setError("Please select a group");
        if (!role) return setError("Please select a role");

        API.RESOURCES.USERS.EDIT({
            id: query.userId!.toString(),
            name,
            code,
            hrCode, // <-- Add hrCode to the request payload
            groupId: group.id,
            role: role.id,
            accountType: accountType?.id ?? 0,
            vacation,
        }).then((res) => {
            if (res && !res.error) {
                dispatch(edit(res.data));
                routerPush(pathname);
            }
        });
    };

    if (!active) return <></>;

    return (
        <motion.div
            initial={{ backgroundColor: "#00000000" }}
            animate={{ backgroundColor: "#00000055", height: "auto" }}
            className="z-50 flex items-center justify-center fixed top-0 left-0 right-0 min-h-screen "
        >
            <motion.div
                initial={{ opacity: 0.1 }}
                animate={{ opacity: 1 }}
                className="bg-white px-5 py-4  rounded-lg  w-[700px]"
            >
                <h2 className="text-lg mb-5">Edit user</h2>
                <form onSubmit={handleSubmit} className="flex flex-col gap-6">
                    <div className="text-red-600">{error}</div>

                    <div className="grid grid-cols-2 gap-4">
                        <InputTextField label="Name" value={name} handleChange={setName} />
                        <InputTextField label="Code" value={code} handleChange={setCode} />
                        <InputTextField label="HR Code" value={hrCode} handleChange={setHrCode} /> {/* <-- Add HR Code input field */}

                        <Dropdown
                            label="Group"
                            value={group}
                            options={groups}
                            handleChange={setGroup}
                        />

                        <Dropdown
                            label="Role"
                            value={role}
                            options={[
                                { id: 0, name: "Project Manager" },
                                { id: 1, name: "Section Head" },
                                { id: 2, name: "Team Leader" },
                                { id: 3, name: "Member" },
                            ]}
                            handleChange={setRole as (v: { id: number; name: string }) => void}
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
                    </div>
                    {accountType?.id === 0 && (
                        <div className="mt-4">
                            <h3 className="text-md font-semibold mb-2">Vacations</h3>
                            <div className="grid grid-cols-3 gap-4">
                                <InputTextField
                                    label="Annual"
                                    value={vacation.annual.toString()}
                                    handleChange={(val) => setVacation({ ...vacation, annual: Number(val) })}
                                />
                                <InputTextField
                                    label="Sick"
                                    value={vacation.sick.toString()}
                                    handleChange={(val) => setVacation({ ...vacation, sick: Number(val) })}
                                />
                                <InputTextField
                                    label="Emergency"
                                    value={vacation.emergency.toString()}
                                    handleChange={(val) => setVacation({ ...vacation, emergency: Number(val) })}
                                />
                            </div>
                        </div>
                    )}

                    <FormConclusion pathname="/resources/users" submittable={true} />
                </form>
            </motion.div>
        </motion.div>
    );
};

export default EditUser;

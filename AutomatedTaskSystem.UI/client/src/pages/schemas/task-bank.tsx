import Link from "next/link";
import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import PlusIcon from "../../assets/Icons/Plus";
import QueryButton from "../../components/button/queryButton";
import AddTaskBank from "../../components/forms/schemas/task-bank/AddTaskBank";
import EditTaskBank from "../../components/forms/schemas/task-bank/EditTaskBank";
import Header from "../../components/header/header";
import API from "../../lib/API";
import Head from "next/head";
import Loader from "../../components/loader";

export interface ITaskBank {
    id: number;
    name: string;
    tl: boolean;
    type: TaskBankType;
    group: {
        id: number;
        name: string;
    };
    duration: number;
}

const TaskBank = () => {
    const [bank, setBank] = useState<ITaskBank[]>();
    const [activeItem, setActiveItem] = useState<ITaskBank>();
    const router = useRouter();

    useEffect(() => {
        API.SCHEMAS.TASK_BANK.GET_ALL().then((res) => {
            if (res) setBank(res);
        });
    }, []);

    useEffect(() => {
        if (
            router.query.item &&
            router.query.form === "edit" &&
            bank !== undefined
        ) {
            const id = parseInt(router.query.item.toString());
            const foundIem = bank.find((_) => _.id === id);
            setActiveItem(foundIem);
        } else setActiveItem(undefined);
    }, [router.query.item, router.query.form, bank]);

    const HandleTaskAdd = (item: ITaskBank) => {
        setBank((ps) => (ps ? [...ps, item] : [item]));
        router.push("/schemas/task-bank");
    };

    const HandleTaskEdit = (item: ITaskBank) => {
        setBank((ps) => {
            const newState: ITaskBank[] = [];

            ps !== undefined &&
                ps.forEach((element) => {
                    if (element.id === item.id) return newState.push(item);
                    newState.push(element);
                });

            return newState;
        });
        router.push("/schemas/task-bank");
    };

    const HandleTaskDelete = (id: number) => {
        API.SCHEMAS.TASK_BANK.DELETE(id).then((res) => {
            if (res && !res.error)
                setBank((ps) => {
                    const newState: ITaskBank[] = [];

                    ps !== undefined &&
                        ps.forEach((element) => {
                            if (element.id !== id) newState.push(element);
                        });

                    return newState;
                });
        });
    };

    if (bank === undefined)
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Head>
                    <title>ATS - Loading</title>
                </Head>
                <Loader />
            </div>
        );

    return (
        <>
            <Head>
                <title>ATS - Task Bank</title>
            </Head>
            <div className="mainContainer">
                <Header text="Task Bank" icon="Schema">
                    <QueryButton
                        icon={<PlusIcon />}
                        iconLeft
                        iconRight={false}
                        text="Add Task"
                        url={{
                            pathname: "/schemas/task-bank",
                            query: {
                                form: "add",
                            },
                        }}
                    />
                </Header>
                <div className="flex flex-col gap-4 mt-4 pb-4">
                    {bank.map((b) => {
                        return (
                            <div
                                key={b.id}
                                className="group gap-2 px-4 text-black justify-between bg-white flex h-16 rounded-lg border-opacity-50 border-black border border-solid transition-all ease-linear hover:border-2 hover:border-opacity-100"
                            >
                                <div className="flex gap-2">
                                    <div className="flex items-center justify-end text-slate-500 hover:text-slate-600">
                                        {b.group.name}
                                    </div>
                                    {b.tl ? (
                                        <div className="flex items-center justify-end text-slate-500 hover:text-slate-600">
                                            Team Leader
                                        </div>
                                    ) : (
                                        ""
                                    )}
                                    <div className="col-span-3 transition-all ease-linear flex items-center group-hover:font-bold">
                                        {b.name}
                                    </div>
                                    <div className="flex items-center justify-start text-slate-500 hover:text-slate-600">
                                        {b.type === 0 ? "Creation" : b.type === 1 ? "Review": ""}
                                    </div>
                                    <div className="flex items-center justify-center">
                                        <div className="border border-solid border-slate-200 text-slate-500 px-2 py-1 rounded">
                                            {Math.floor(b.duration / 60)}:
                                            {b.duration % 60 < 10
                                                ? "0" + (b.duration % 60)
                                                : b.duration % 60}{" "}
                                            Hours
                                        </div>
                                    </div>
                                </div>
                                <div className="flex gap-4 items-center">
                                    <Link
                                        href={{
                                            pathname: `/schemas/task-bank`,
                                            query: { form: "edit", item: b.id },
                                        }}
                                    >
                                        <div className="cursor-pointer rounded bg-blue-500 text-white px-3 py-1">
                                            Edit
                                        </div>
                                    </Link>
                                    <div
                                        onClick={() => HandleTaskDelete(b.id)}
                                        className="cursor-pointer rounded bg-rose-500 text-white px-3 py-1"
                                    >
                                        Delete
                                    </div>
                                </div>
                            </div>
                        );
                    })}
                </div>
                {router.query.form === "add" ? (
                    <AddTaskBank complete={HandleTaskAdd} />
                ) : (
                    <></>
                )}
                {router.query.form === "edit" && activeItem ? (
                    <EditTaskBank
                        taskBank={activeItem}
                        complete={HandleTaskEdit}
                    />
                ) : (
                    <></>
                )}
            </div>
        </>
    );
};

export default TaskBank;

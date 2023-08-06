import React, { useEffect, useState } from "react";
import { LockClosedIcon } from "@heroicons/react/24/solid";
import { useAppDispatch, useAppSelector } from "../../app/hooks";
import authService from "../../lib/Auth";
import { login } from "../../slices/authSlice";
import Head from "next/head";
import Loader from "../loader";
import { ChevronRightIcon } from "@heroicons/react/20/solid";

interface Props {
    children?: JSX.Element | JSX.Element[];
}

const Login = () => {
    const dispatch = useAppDispatch();
    const [code, setCode] = useState("");
    const [err, setErr] = useState<string>("");
    const [loading, setLoading] = useState<boolean>(false);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setLoading(true);
        setTimeout(() => {
            if (code !== "" && code.length === 6) {
                return authService.login(code).then((res) => {
                    if (res) {
                        return authService.getUser().then((details) => {
                            if (details && !details.error)
                                dispatch(login(details.data));
                            setErr("Invalid code, Please try again.");
                            setLoading(false);
                        });
                    }
                    setErr("Invalid code, Please try again.");
                    setLoading(false);
                });
            }
            setErr("Make sure the code is 6 characters");
            setLoading(false);
        }, 2000);
    };

    return (
        <>
            <Head>
                <title>ATS - Login</title>
            </Head>
            <div className="bg-gradient-to-tr from-sky-300 to-slate-400 h-screen flex flex-col justify-center items-center gap-4">
                <div className="bg-white px-6 py-4 rounded-lg flex flex-col gap-2">
                    <h2 className="text-4xl">Selah El-Telmeez</h2>
                    <h3 className="text-2xl">Welcome to ATS</h3>
                    <p className={err === "" ? "" : "text-red-500"}>
                        {err !== ""
                            ? err
                            : "Please enter your login code to continue"}
                    </p>
                    <form
                        onSubmit={handleSubmit}
                        className="flex gap-2 px-2 bg-slate-200 rounded-md py-2"
                    >
                        <label
                            className={`transition-all ease-in flex gap-2 px-2 py-1 outline-none border border-solid ${
                                err === ""
                                    ? "focus-within:outline-sky-500 border-white"
                                    : "focus-within:outline-red-500 border-red-500"
                            } rounded-sm`}
                        >
                            <div className="w-6 h-6">
                                <LockClosedIcon
                                    className={`transition-all ease-in ${
                                        err === ""
                                            ? "fill-slate-700"
                                            : "fill-red-500"
                                    }`}
                                />
                            </div>
                            <input
                                maxLength={6}
                                className={`transition-all ease-in outline-none grow bg-opacity-0 rounded bg-slate-200 ${
                                    err === "" ? "text-black" : "text-red-500"
                                } font-medium text-lg`}
                                type="text"
                                value={code}
                                onChange={(e) => {
                                    if (err !== "") setErr("");
                                    setCode(e.target.value.toUpperCase());
                                }}
                                placeholder="Your Code"
                            />
                        </label>
                        <button
                            className={`bg-blue-600 text-white py-1 px-2 rounded flex items-center justify-center${
                                loading ? " opacity-60" : ""
                            } transition-opacity ease-out`}
                        >
                            {loading ? (
                                <div className="animate-spin p-[0.625rem] rounded-full border-solid border-2 border-t-white border-b-blue-400 border-x-blue-400"></div>
                            ) : (
                                <ChevronRightIcon className="w-6 h-6" />
                            )}
                        </button>
                    </form>
                </div>
            </div>
        </>
    );
};

const Auth = ({ children }: Props) => {
    const dispatch = useAppDispatch();
    const [loading, setLoading] = useState(true);
    const isAuth = useAppSelector((s) => s.authSlice);

    useEffect(() => {
        authService.getUser().then((res) => {
            if (res && !res.error) {
                setLoading(false);
                dispatch(login(res.data));
                return;
            }
            setLoading(false);
        });
    }, [setLoading, dispatch]);

    if (loading)
        return (
            <div className="flex items-center justify-center mx-auto h-screen bg-slate-200">
                <Head>
                    <title>ATS - Loading</title>
                </Head>
                <Loader />
            </div>
        );

    if (isAuth.isAuth) return <>{children}</>;

    return <Login />;
};

export default Auth;

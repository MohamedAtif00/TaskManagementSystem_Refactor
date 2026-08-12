import React, { useCallback, useEffect, useState } from "react";
import { LockClosedIcon } from "@heroicons/react/24/solid";
import { useAppDispatch, useAppSelector } from "../../app/hooks";
import authService from "../../lib/Auth";
import { login, logout } from "../../slices/authSlice";
import Head from "next/head";
import Loader from "../loader";
import { ChevronRightIcon } from "@heroicons/react/20/solid";
import { useRouter } from "next/router";

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
                <title>TMS - Login</title>
            </Head>
            <div className="bg-gradient-to-tr from-sky-300 to-slate-400 h-screen flex flex-col justify-center items-center gap-4">
                <div className="bg-white px-6 py-4 rounded-lg flex flex-col gap-2">
                    <h2 className="text-4xl">Selah El-Telmeez</h2>
                    <h3 className="text-2xl">Welcome to TMS</h3>
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
    const router = useRouter();
    const [loading, setLoading] = useState(true);
    const [tokenEpoch, setTokenEpoch] = useState(0);
    const auth = useAppSelector((s) => s.authSlice);

    const forceSessionExpired = useCallback(async () => {
        await authService.endSessionExpired();
        dispatch(logout());
    }, [dispatch]);

    const checkAuth = useCallback(async (): Promise<boolean> => {
        const valid = await authService.ensureValidSession();
        if (!valid) {
            dispatch(logout());
            return false;
        }
        setTokenEpoch((n) => n + 1);
        return true;
    }, [dispatch]);

    // Initial bootstrap
    useEffect(() => {
        authService.getUser().then((res) => {
            if (res && !res.error) {
                dispatch(login(res.data));
                setTokenEpoch((n) => n + 1);
            } else {
                dispatch(logout());
            }
            setLoading(false);
        });
    }, [dispatch]);

    // On route change: silently refresh if needed; only block if refresh fails
    useEffect(() => {
        if (!auth.isAuth) return;

        const onRouteChangeStart = () => {
            // Kick off refresh early if near expiry (non-blocking for navigation)
            if (authService.shouldRefreshAccessToken()) {
                void authService.refreshAccessToken().then((ok) => {
                    if (ok) setTokenEpoch((n) => n + 1);
                });
            }
        };

        const onRouteChangeComplete = () => {
            void checkAuth().then((ok) => {
                if (!ok) return;
            });
        };

        router.events.on("routeChangeStart", onRouteChangeStart);
        router.events.on("routeChangeComplete", onRouteChangeComplete);
        return () => {
            router.events.off("routeChangeStart", onRouteChangeStart);
            router.events.off("routeChangeComplete", onRouteChangeComplete);
        };
    }, [auth.isAuth, router.events, checkAuth]);

    // Silently renew access token before it expires while the user stays on a page
    useEffect(() => {
        if (!auth.isAuth) return;

        let cancelled = false;
        let timer: number | undefined;

        const scheduleRefresh = () => {
            const ms = authService.getMsUntilRefresh();
            timer = window.setTimeout(async () => {
                if (cancelled) return;
                const ok = await authService.refreshAccessToken();
                if (cancelled) return;
                if (!ok) {
                    await forceSessionExpired();
                    return;
                }
                setTokenEpoch((n) => n + 1);
                scheduleRefresh();
            }, Math.max(ms, 1_000));
        };

        scheduleRefresh();

        return () => {
            cancelled = true;
            if (timer !== undefined) window.clearTimeout(timer);
        };
    }, [auth.isAuth, auth.id, tokenEpoch, forceSessionExpired]);

    // Re-check / refresh when the browser tab becomes visible again
    useEffect(() => {
        if (!auth.isAuth) return;

        const onVisibility = () => {
            if (document.visibilityState === "visible") {
                void checkAuth();
            }
        };

        document.addEventListener("visibilitychange", onVisibility);
        return () => document.removeEventListener("visibilitychange", onVisibility);
    }, [auth.isAuth, checkAuth]);

    if (loading)
        return (
            <div className="flex items-center justify-center mx-auto h-screen bg-slate-200">
                <Head>
                    <title>TMS - Loading</title>
                </Head>
                <Loader />
            </div>
        );

    if (auth.isAuth) return <>{children}</>;

    return <Login />;
};

export default Auth;

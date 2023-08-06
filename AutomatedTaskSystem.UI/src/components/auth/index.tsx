import React, { useEffect, useRef, useState } from "react";
import { useAppDispatch, useAppSelector } from "../../app/hooks";
import authService from "../../lib/Auth";
import { login } from "../../slices/authSlice";
import styles from "./styles.module.scss";
import Head from "next/head";
import Loader from "../loader";

interface Props {
    children?: JSX.Element | JSX.Element[];
}

const OneCharInput = ({
    previousRef,
    currentRef,
    nextRef,
    value,
    onChange,
    onChangePrev,
}: {
    currentRef: React.RefObject<HTMLInputElement>;
    nextRef?: React.RefObject<HTMLInputElement>;
    previousRef?: React.RefObject<HTMLInputElement>;
    value: string;
    onChange: (s: string) => void;
    onChangePrev?: (s: string) => void;
}) => {
    return (
        <div>
            <input
                ref={currentRef}
                type="text"
                value={value}
                onKeyDown={(e) => {
                    if (onChangePrev && value === "" && e.key === "Backspace") {
                        onChangePrev("");
                        previousRef?.current?.focus();
                    }
                    if (value !== "" && e.key === "Backspace") {
                        onChange("");
                    }
                }}
                onChange={(e) => {
                    const newValue = e.target.value;
                    if (newValue !== " ") {
                        onChange(
                            newValue.toUpperCase().slice(newValue.length - 1)
                        );
                        if (nextRef && newValue != "") {
                            nextRef.current?.focus();
                        }
                    }
                }}
            />
        </div>
    );
};

const Login = () => {
    const dispatch = useAppDispatch();
    const [code1, setCode1] = useState("");
    const [code2, setCode2] = useState("");
    const [code3, setCode3] = useState("");
    const [code4, setCode4] = useState("");
    const [code5, setCode5] = useState("");
    const [code6, setCode6] = useState("");
    const inputRef1 = useRef<HTMLInputElement>(null);
    const inputRef2 = useRef<HTMLInputElement>(null);
    const inputRef3 = useRef<HTMLInputElement>(null);
    const inputRef4 = useRef<HTMLInputElement>(null);
    const inputRef5 = useRef<HTMLInputElement>(null);
    const inputRef6 = useRef<HTMLInputElement>(null);

    useEffect(() => {
        if (code1 && code2 && code3 && code4 && code5 && code6) {
            authService
                .login(code1 + code2 + code3 + code4 + code5 + code6)
                .then((res) => {
                    if (res) {
                        authService.getUser().then((details) => {
                            if (details && !details.error) {
                                dispatch(login(details.data));
                            }
                        });
                    }
                    setCode1("");
                    setCode2("");
                    setCode3("");
                    setCode4("");
                    setCode5("");
                    setCode6("");
                });
        }
    }, [
        code1,
        code2,
        code3,
        code4,
        code5,
        code6,
        setCode1,
        setCode2,
        setCode3,
        setCode4,
        setCode5,
        setCode6,
        dispatch,
    ]);
    return (
        <>
            <Head>
                <title>ATS - Login</title>
            </Head>
            <div className={styles.container}>
                <h2>Selah El-Telmeez</h2>
                <h1>Welcome to ATS</h1>
                <p>Please enter your login Code to Continue</p>
                <form>
                    <div className={styles.split}>
                        <OneCharInput
                            onChange={setCode1}
                            currentRef={inputRef1}
                            value={code1}
                            nextRef={inputRef2}
                        />
                        <OneCharInput
                            onChange={setCode2}
                            onChangePrev={setCode1}
                            value={code2}
                            previousRef={inputRef1}
                            currentRef={inputRef2}
                            nextRef={inputRef3}
                        />
                        <OneCharInput
                            onChangePrev={setCode2}
                            onChange={setCode3}
                            value={code3}
                            previousRef={inputRef2}
                            currentRef={inputRef3}
                            nextRef={inputRef4}
                        />
                    </div>
                    <div className={styles.dash}></div>
                    <div className={styles.split}>
                        <OneCharInput
                            onChange={setCode4}
                            onChangePrev={setCode3}
                            value={code4}
                            previousRef={inputRef3}
                            currentRef={inputRef4}
                            nextRef={inputRef5}
                        />
                        <OneCharInput
                            onChange={setCode5}
                            onChangePrev={setCode4}
                            value={code5}
                            previousRef={inputRef4}
                            currentRef={inputRef5}
                            nextRef={inputRef6}
                        />
                        <OneCharInput
                            onChange={setCode6}
                            onChangePrev={setCode5}
                            value={code6}
                            previousRef={inputRef5}
                            currentRef={inputRef6}
                        />
                    </div>
                </form>
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
            <div className="flex items-center justify-center mx-auto h-screen">
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

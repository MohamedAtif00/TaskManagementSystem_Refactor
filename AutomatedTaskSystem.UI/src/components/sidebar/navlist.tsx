import { useState } from "react";

interface Props {
    children?: JSX.Element | JSX.Element[];
    icon?: ({ className }: { className?: string }) => JSX.Element;
    label: string;
}

const NavList: React.FC<Props> = ({ children, icon: Icon, label }) => {
    const [toggle, setToggle] = useState(false);

    const handleToggle = () => setToggle((ps) => !ps);

    let numberOfChildren = 0;

    if (children) {
        if (Array.isArray(children)) numberOfChildren = children.length;
        else numberOfChildren = 1;
    }

    return (
        <div
            className="transition-all ease-out overflow-hidden"
            style={{
                height: toggle ? `${(numberOfChildren + 1) * 3}rem` : "3rem",
            }}
        >
            <div
                className="cursor-pointer group h-12 flex gap-5"
                onClick={handleToggle}
            >
                <div
                    className={`transition-all pl-2 ${
                        toggle ? "bg-cyan-400" : ""
                    } h-12 rounded-r`}
                ></div>
                <div className="grow pr-7 flex justify-between items-center text-slate-400 group-hover:text-white">
                    <div className="flex gap-3">
                        {Icon ? (
                            <Icon className="fill-slate-400 group-hover:fill-white h-7 w-7" />
                        ) : (
                            ""
                        )}
                        <div>{label}</div>
                    </div>
                    <div
                        className={`${
                            toggle ? "-rotate-90" : "rotate-90"
                        } transition-all ease-out duration-500`}
                    >
                        <svg
                            width="8"
                            height="17"
                            viewBox="0 0 8 17"
                            xmlns="http://www.w3.org/2000/svg"
                            className="fill-slate-400 group-hover:fill-white"
                        >
                            <path
                                fillRule="evenodd"
                                clipRule="evenodd"
                                d="M0.137832 0.646447C0.321608 0.451184 0.619568 0.451184 0.803344 0.646447L7.86217 8.14645C8.04594 8.34171 8.04594 8.65829 7.86217 8.85355L0.803344 16.3536C0.619568 16.5488 0.321608 16.5488 0.137832 16.3536C-0.045944 16.1583 -0.045944 15.8417 0.137832 15.6464L6.8639 8.5L0.137832 1.35355C-0.045944 1.15829 -0.045944 0.841709 0.137832 0.646447Z"
                            />
                        </svg>
                    </div>
                </div>
            </div>
            <div className="pl-2">{children}</div>
        </div>
    );
};

export default NavList;

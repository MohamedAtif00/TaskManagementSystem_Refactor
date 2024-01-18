import { useRouter } from "next/router";
import styles from "./styles.module.scss";

const Backdrop = (props: {
    mainRoute?: string;
    children?: JSX.Element | JSX.Element[];
}) => {
    const router = useRouter();

    return (
        <div
            id={styles.modal}
            onClick={(e) => {
                if (e.currentTarget == e.target) {
                    return props.mainRoute
                        ? router.replace(props.mainRoute)
                        : router.replace("/");
                }
            }}
        >
            {props.children}
        </div>
    );
};

export default Backdrop;

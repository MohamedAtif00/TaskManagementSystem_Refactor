import Header from "../../components/header/header";
import styles from "../../styles/resources.module.scss";
import Link from "next/link";
import { useAppSelector } from "../../app/hooks";
import { useRouter } from "next/router";
import Head from "next/head";

const Resources = () => {
    const auth = useAppSelector((s) => s.authSlice);
    const router = useRouter();

    // if (!auth.isAuth || auth.role !== 0 || ) {
    //     router.replace("/");
    // }

    return (
        <>
            <Head>
                <title>TMS - Resources</title>
            </Head>
            <div className="mainContainer">
                <Header text="Resources" icon="Resources"></Header>
                <div className={styles.container}>
                    <Link className={styles.resource} href="/resources/groups">
                        Groups
                    </Link>
                    <Link className={styles.resource} href="/resources/users">
                        Users
                    </Link>
                    <Link
                        className={styles.resource}
                        href="/resources/sections"
                    >
                        Sections
                    </Link>
                </div>
            </div>
        </>
    );
};

export default Resources;

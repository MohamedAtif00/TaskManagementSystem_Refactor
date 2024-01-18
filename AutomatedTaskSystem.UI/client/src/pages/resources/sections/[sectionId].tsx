import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import Header from "../../../components/header/header";
import styles from "../../../styles/resources.module.scss";
import API from "../../../lib/API";
import Head from "next/head";
import Loader from "../../../components/loader";

const Section = () => {
    const router = useRouter();
    const [section, setSection] = useState<ISection>();

    useEffect(() => {
        const id = router.query.sectionId;
        if (id !== undefined)
            API.RESOURCES.SECTIONS.GET_ONE(id).then((res) => {
                if (res && !res.error) setSection(res.data);
            });
    }, [setSection, router.query.sectionId]);

    if (section === undefined)
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
                <title>ATS - {section.name} Section</title>
            </Head>
            <div className="mainContainer">
                <Header text={section.name} icon="Resources"></Header>
                <div className={styles.container}>
                    <h2>Head:{section.head.name}</h2>
                    <div>
                        Groups:
                        <div>
                            {section.groups.map((g) => (
                                <div key={g.id}>{g.name}</div>
                            ))}
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
};

export default Section;

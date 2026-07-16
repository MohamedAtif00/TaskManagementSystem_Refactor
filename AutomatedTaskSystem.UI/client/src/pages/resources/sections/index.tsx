import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import PlusIcon from "../../../assets/Icons/Plus";
import QueryButton from "../../../components/button/queryButton";
import AddSection from "../../../components/forms/addSection";
import Header from "../../../components/header/header";
import SectionItem from "../../../components/sectionItem";
import API from "../../../lib/API";
import styles from "../../../styles/resources.module.scss";
import Head from "next/head";

const Sections = () => {
    const [users, setUsers] = useState<IUser[]>([]);
    const [groups, setGroups] = useState<IGroup[]>([]);
    const [sections, setSections] = useState<{ id: number; name: string }[]>(
        []
    );

    useEffect(() => {
        API.RESOURCES.USERS.GET_ALL().then((res) => {
            if (res && !res.error) {
                setUsers(res.data);
            }
        });
    }, [setUsers]);

    useEffect(() => {
        API.RESOURCES.GROUPS.GET_ALL().then((res) => {
            if (res && !res.error) {
                setGroups(res.data);
            }
        });
    }, [setGroups]);

    useEffect(() => {
        API.RESOURCES.SECTIONS.GET_ALL().then((res) => {
            if (res && !res.error) {
                setSections(res.data);
            }
        });
    }, [setSections]);

    const handleAdd = ({
        name,
        headId,
        groups,
    }: {
        name: string;
        headId: number;
        groups: number[];
    }) => {
        API.RESOURCES.SECTIONS.CREATE({ groups, headId, name }).then((res) => {
            if (res) {
                setSections((ps) => [...ps, res]);
            }
        });
    };

    return (
        <>
            <Head>
                <title>TMS - Sections</title>
            </Head>
            <div className="mainContainer">
                <Header text="Sections" icon="Resources">
                    <QueryButton
                        icon={<PlusIcon />}
                        iconLeft
                        iconRight={false}
                        text="Add"
                        url={{
                            pathname: "/resources/sections",
                            query: {
                                form: "section",
                            },
                        }}
                    />
                </Header>
                <div className={styles.container}>
                    {sections.map((s) => (
                        <SectionItem id={s.id} name={s.name} key={s.id} />
                    ))}
                </div>
                <AddSection
                    users={users}
                    groups={groups}
                    handleSubmit={handleAdd}
                />
            </div>
        </>
    );
};

export default Sections;

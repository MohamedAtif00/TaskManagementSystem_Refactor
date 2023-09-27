import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import { useAppDispatch, useAppSelector } from "../../app/hooks";
import PlusIcon from "../../assets/Icons/Plus";
import QueryButton from "../../components/button/queryButton";
import AddNode from "../../components/forms/schemas/nodes/addNode";
import EditNode from "../../components/forms/schemas/nodes/editNode";
import AddStep from "../../components/forms/schemas/nodes/steps/addStep";
import EditStep from "../../components/forms/schemas/nodes/steps/editStep";
import Header from "../../components/header/header";
import NodeItem from "../../components/nodeItem";
import API from "../../lib/API";
import { load } from "../../slices/nodesSlice";
import EditSchema from "../../components/pageComponent/schemas/editSchema";
import RemoveStep from "../../components/pageComponent/schemas/remoteStep";
import RemoveNode from "../../components/pageComponent/schemas/removeNode";
import Head from "next/head";
import Loader from "../../components/loader";

interface ISchemaLocal {
    description: string;
    id: number;
    name: string;
}

const Schema = () => {
    const [schema, setSchema] = useState<ISchemaLocal>();
    const nodes = useAppSelector((s) => s.nodesSlice);
    const dispatch = useAppDispatch();
    const router = useRouter();
    const auth = useAppSelector((s) => s.authSlice);
    const [editNode, setEditNode] = useState<INode>();
    const [editStep, setEditStep] = useState<IStep>();

    useEffect(() => {
        const id = router.query.stepId;
        if (
            router.query.form === "stepEdit" &&
            id !== undefined &&
            !isNaN(parseInt(id.toString()))
        )
            nodes.forEach((n) => {
                n.steps.forEach((s) => {
                    if (s.id === parseInt(id.toString())) {
                        setEditStep(s);
                    }
                });
            });
        else setEditStep(undefined);
    }, [router.query.form, router.query.stepId, nodes]);

    useEffect(() => {
        if (router.query.form === "nodeEdit" && router.query.nodeId) {
            const id = parseInt(router.query.nodeId.toString());
            if (isNaN(id)) return;
            const n = nodes.find((_) => _.id === id);
            setEditNode(n);
        } else setEditNode(undefined);
    }, [router.query.form, router.query.nodeId, nodes]);

    useEffect(() => {
        if (router.query.schemaId)
            API.SCHEMAS.GET_ONE(router.query.schemaId).then((res) => {
                if (res && !res.error) {
                    setSchema(res.data);
                }
            });
    }, [router.query.schemaId]);

    useEffect(() => {
        if (schema && schema.id > 0) {
            API.SCHEMAS.NODES.GET_ALL(schema.id).then((res) => {
                if (res)
                    dispatch(
                        load(
                            [...res].sort((a, b) => {
                                return a.order - b.order;
                            })
                        )
                    );
            });
        }
    }, [schema, dispatch]);

    const updateNodes = () => {
        if (schema && schema.id > 0) {
            API.SCHEMAS.NODES.GET_ALL(schema.id).then((res) => {
                if (res)
                    dispatch(load([...res].sort((a, b) => a.order - b.order)));
            });
        }
    };

    if (schema === undefined)
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
                <title>ATS - {schema.name} Schema</title>
            </Head>
            <div className="mainContainer">
                <Header text={schema.name} icon="Schema">
                    {auth.role === 0 ? (
                        <>
                            <QueryButton
                                icon={<PlusIcon />}
                                iconLeft
                                iconRight={false}
                                text="Edit"
                                url={{
                                    pathname: `/schemas/${schema.id}`,
                                    query: {
                                        form: "editSchema",
                                    },
                                }}
                            />
                            <QueryButton
                                icon={<PlusIcon />}
                                iconLeft
                                iconRight={false}
                                text="Node"
                                url={{
                                    pathname: `/schemas/${schema.id}`,
                                    query: {
                                        form: "node",
                                    },
                                }}
                            />
                        </>
                    ) : (
                        <></>
                    )}
                </Header>
                <div className="pt-4 flex flex-col gap-4">
                    {nodes.map((n) => {
                        return (
                            <NodeItem
                                updateNodes={updateNodes}
                                schemaId={schema.id}
                                id={n.id}
                                key={n.id}
                                name={n.name}
                                isStart={n.isStart}
                                previous={n.previous}
                                requires={n.requires}
                                steps={n.steps}
                            />
                        );
                    })}
                </div>
                {auth.role === 0 ? (
                    <>
                        <EditSchema />
                        <AddNode
                            updateList={updateNodes}
                            schemaId={schema.id}
                            nodes={nodes.map((n) => ({
                                name: n.name,
                                id: n.id,
                            }))}
                        />
                        <AddStep schemaId={schema.id} />
                        {editStep ? (
                            <EditStep step={editStep} schemaId={schema.id} />
                        ) : (
                            ""
                        )}
                        {editNode ? (
                            <EditNode
                                updateList={updateNodes}
                                node={editNode}
                                nodes={nodes
                                    .filter((n) => n.id !== editNode.id)
                                    .map(({ id, name }) => ({ id, name }))}
                                schemaId={schema.id}
                            />
                        ) : (
                            ""
                        )}
                    </>
                ) : (
                    <></>
                )}
                <RemoveStep />
                <RemoveNode OnSubmit={updateNodes} />
            </div>
        </>
    );
};

export default Schema;

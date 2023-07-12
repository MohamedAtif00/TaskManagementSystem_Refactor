import styles from "../styles.module.scss";
import Backdrop from "../backdrop";
import React, { useEffect, useState } from "react";
import FormField from "../field";
import { useRouter } from "next/router";
import Dropdown from "../dropdown";
import { useAppDispatch, useAppSelector } from "../../../app/hooks";
import API from "../../../lib/API";
import { load } from "../../../slices/schemaSlice";
import Combobox from "react-widgets/cjs/Combobox";

const AddLearningObjective = ({
    path,
    submit,
}: {
    path: string;
    submit: ({
        environment,
        name,
        schemaId,
        tag,
        template,
    }: {
        name: string;
        schemaId: number;
        tag: string;
        template: string;
        environment: string;
    }) => void;
}) => {
    const [submittable, setSubmittable] = useState(false);
    const [name, setName] = useState("");
    const [tag, setTag] = useState("");
    const [template, setTemplate] = useState("");
    const [environment, setEnvironment] = useState("");
    const [schema, setSchema] = useState<ISchema>();
    const [active, setActive] = useState(false);
    const router = useRouter();
    const schemas = useAppSelector((s) => s.schemasSlice);
    const dispatch = useAppDispatch();

    useEffect(() => {
        API.SCHEMAS.GET_ALL().then((res) => {
            if (res) {
                dispatch(load(res));
            }
        });
    }, [dispatch]);

    useEffect(() => {
        if (name === "" || schema === undefined) {
            setSubmittable(false);
        } else {
            setSubmittable(true);
        }
    }, [name, schema]);

    useEffect(() => {
        const _active = router.query.form === "learning-objective";
        setActive(_active);
        if (!_active) {
            setName("");
            setTag("");
            setTemplate("");
            setEnvironment("");
            setSchema(undefined);
        }
    }, [router]);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (submittable && schema)
            submit({ environment, name, schemaId: schema.id, tag, template });
    };

    if (active)
        return (
            <Backdrop mainRoute={path}>
                <div
                    className={[styles.form, styles.center, styles.loForm].join(
                        " "
                    )}
                >
                    <form onSubmit={handleSubmit}>
                        <div className={styles.inputs}>
                            <FormField
                                label="Name"
                                onChange={setName}
                                value={name}
                            />
                            <Combobox
                                data={schemas}
                                dataKey="id"
                                textField="name"
                                onChange={(e) => {
                                    if (typeof e !== "string") setSchema(e);
                                }}
                            />
                            {/* 
                            <Dropdown
                                id={schemaId}
                                label="Schema"
                                options={schemas}
                                handleChange={setSchemaId}
                            />
							*/}
                            <FormField
                                label="Tag"
                                onChange={setTag}
                                value={tag}
                            />
                            <FormField
                                label="Template"
                                onChange={setTemplate}
                                value={template}
                            />
                            <FormField
                                label="Environment"
                                onChange={setEnvironment}
                                value={environment}
                            />
                        </div>
                        <div>
                            <input
                                type="submit"
                                value="Add"
                                className={[
                                    styles.submit,
                                    submittable ? "" : styles.inactive,
                                ].join(" ")}
                            />
                        </div>
                    </form>
                </div>
            </Backdrop>
        );

    return <></>;
};

export default AddLearningObjective;

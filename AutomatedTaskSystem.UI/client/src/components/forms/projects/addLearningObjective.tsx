import styles from "../styles.module.scss";
import Backdrop from "../backdrop";
import React, { useEffect, useRef, useState } from "react";
import FormField from "../field";
import { useRouter } from "next/router";
import { useAppDispatch, useAppSelector } from "../../../app/hooks";
import API, { BasicInfo } from "../../../lib/API";
import { load } from "../../../slices/schemaSlice";
import CustomizedCombobox from "../../formComponents/Combobox";

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
    }) => void | Promise<boolean | void>;
}) => {
    const [submittable, setSubmittable] = useState(false);
    const [name, setName] = useState("");
    const [tag, setTag] = useState("");
    const [template, setTemplate] = useState("");
    const [environment, setEnvironment] = useState("");
    const [schema, setSchema] = useState<BasicInfo>();
    const [active, setActive] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const submittingRef = useRef(false);
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
            submittingRef.current = false;
            setIsSubmitting(false);
        }
    }, [router]);

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!submittable || !schema || submittingRef.current) return;
        submittingRef.current = true;
        setIsSubmitting(true);
        try {
            const ok = await submit({
                environment,
                name,
                schemaId: schema.id,
                tag,
                template,
            });
            if (ok === false) {
                submittingRef.current = false;
                setIsSubmitting(false);
            }
        } catch {
            submittingRef.current = false;
            setIsSubmitting(false);
        }
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
                            <div>
                                <div className="text-sm">Schema:</div>
                                <CustomizedCombobox
                                    value={schema}
                                    onChange={(e) => setSchema(e)}
                                    options={schemas}
                                />
                            </div>
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
                                value={isSubmitting ? "Adding..." : "Add"}
                                disabled={isSubmitting || !submittable}
                                className={[
                                    styles.submit,
                                    submittable && !isSubmitting ? "" : styles.inactive,
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

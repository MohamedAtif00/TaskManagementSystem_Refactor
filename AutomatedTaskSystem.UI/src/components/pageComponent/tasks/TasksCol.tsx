import TaskCard from "./TaskCard";

interface Props {
    items: TaskInfo[];
    label: string;
}

const TaskCol: React.FC<Props> = ({ items, label }) => {
    const highPrio = items.filter((t) => t.priority === 3);
    const medPrio = items.filter((t) => t.priority === 2);
    const lowPrio = items.filter((t) => t.priority === 1);
    const nonePrio = items.filter((t) => t.priority === null);

    return (
        <div
            className={
                "relative overflow-y-auto pb-6 w-96 box-content shrink-0 flex flex-col gap-6 bg-slate-200 px-4"
            }
        >
            <div className="sticky top-0 left-0 z-10 px-2 pt-2">
                <h3 className="px-4 font-bold text-lg bg-white bg-opacity-10 backdrop-blur border border-white border-solid rounded border-opacity-50">
                    {label}
                </h3>
            </div>
            {[...highPrio, ...medPrio, ...lowPrio, ...nonePrio].map((t) => {
                return (
                    <TaskCard
                        paused={t.paused}
                        priority={null}
                        id={t.id}
                        attention={t.attention}
                        key={t.id}
                        name={t.name}
                        lo={t.learningObjective.name}
                        flagged={t.flagged}
                        userName={t.user && t.user.name}
                        from={t.from}
                        isRollback={t.isRollback}
                        rollbackCount={t.rollbackCount}
                    />
                );
            })}
        </div>
    );
};

export default TaskCol;

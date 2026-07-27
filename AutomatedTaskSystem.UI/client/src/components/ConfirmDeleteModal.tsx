type ConfirmDeleteModalProps = {
    open: boolean;
    title: string;
    description: string;
    confirmLabel?: string;
    cancelLabel?: string;
    submitting?: boolean;
    blocked?: boolean;
    onClose: () => void;
    onConfirm?: () => void;
};

const ConfirmDeleteModal = ({
    open,
    title,
    description,
    confirmLabel = "Delete",
    cancelLabel = "Cancel",
    submitting = false,
    blocked = false,
    onClose,
    onConfirm,
}: ConfirmDeleteModalProps) => {
    if (!open) return null;

    return (
        <div
            className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4"
            role="dialog"
            aria-modal="true"
            aria-labelledby="confirm-delete-title"
            onClick={submitting ? undefined : onClose}
        >
            <div
                className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl"
                onClick={(e) => e.stopPropagation()}
            >
                <div className="flex items-start justify-between gap-4">
                    <div>
                        <h2 id="confirm-delete-title" className="text-xl font-bold text-slate-900">
                            {title}
                        </h2>
                        <p className="mt-2 text-sm text-slate-600">{description}</p>
                    </div>
                    <button
                        type="button"
                        className="rounded-md px-2 py-1 text-slate-400 hover:bg-slate-100 hover:text-slate-700"
                        onClick={onClose}
                        disabled={submitting}
                        aria-label="Close"
                    >
                        x
                    </button>
                </div>

                <div className="mt-6 flex justify-end gap-3">
                    {blocked ? (
                        <button
                            type="button"
                            className="rounded-lg bg-slate-800 px-4 py-2 text-white hover:bg-slate-900"
                            onClick={onClose}
                        >
                            OK
                        </button>
                    ) : (
                        <>
                            <button
                                type="button"
                                className="rounded-lg border border-slate-300 px-4 py-2 text-slate-700 hover:bg-slate-50"
                                onClick={onClose}
                                disabled={submitting}
                            >
                                {cancelLabel}
                            </button>
                            <button
                                type="button"
                                className="rounded-lg bg-red-600 px-4 py-2 text-white hover:bg-red-700 disabled:opacity-60"
                                onClick={onConfirm}
                                disabled={submitting}
                            >
                                {submitting ? "Deleting..." : confirmLabel}
                            </button>
                        </>
                    )}
                </div>
            </div>
        </div>
    );
};

export default ConfirmDeleteModal;

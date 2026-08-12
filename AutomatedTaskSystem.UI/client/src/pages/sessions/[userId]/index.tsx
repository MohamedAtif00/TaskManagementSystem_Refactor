import Head from "next/head";
import Link from "next/link";
import { useRouter } from "next/router";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useAppSelector } from "../../../app/hooks";
import API from "../../../lib/API";
import { DayStorySegment, UserDayStoryline } from "../../../lib/API/sessions";

const toDateInputValue = (d: Date) => {
    const y = d.getUTCFullYear();
    const m = String(d.getUTCMonth() + 1).padStart(2, "0");
    const day = String(d.getUTCDate()).padStart(2, "0");
    return `${y}-${m}-${day}`;
};

const formatClock = (value: string | Date) => {
    const d = value instanceof Date ? value : new Date(value);
    if (Number.isNaN(d.getTime())) return "—";
    return d.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit", second: "2-digit" });
};

const formatClockShort = (value: string | Date) => {
    const d = value instanceof Date ? value : new Date(value);
    if (Number.isNaN(d.getTime())) return "—";
    return d.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
};

const formatDateTime = (value?: string | null) => {
    if (!value) return "—";
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return value;
    return d.toLocaleString();
};

type HoverInfo = {
    clientX: number;
    clientY: number;
    pointerTime: Date;
    segment: DayStorySegment;
};

const DayBar = ({
    segments,
    dayStart,
    dayEnd,
}: {
    segments: DayStorySegment[];
    dayStart: string;
    dayEnd: string;
}) => {
    const barRef = useRef<HTMLDivElement>(null);
    const [hover, setHover] = useState<HoverInfo | null>(null);

    const startMs = new Date(dayStart).getTime();
    const endMs = new Date(dayEnd).getTime();
    const total = Math.max(1, endMs - startMs);

    const findSegmentAt = (timeMs: number): DayStorySegment | null => {
        for (const seg of segments) {
            const a = new Date(seg.start).getTime();
            const b = new Date(seg.end).getTime();
            if (timeMs >= a && timeMs < b) return seg;
        }
        // Edge: exactly at day end — use last segment
        if (segments.length > 0 && timeMs >= endMs - 1) {
            return segments[segments.length - 1];
        }
        return null;
    };

    const onMouseMove = (e: React.MouseEvent<HTMLDivElement>) => {
        const el = barRef.current;
        if (!el) return;
        const rect = el.getBoundingClientRect();
        const ratio = Math.min(1, Math.max(0, (e.clientX - rect.left) / rect.width));
        const timeMs = startMs + ratio * total;
        const segment = findSegmentAt(timeMs);
        if (!segment) {
            setHover(null);
            return;
        }
        setHover({
            clientX: e.clientX,
            clientY: e.clientY,
            pointerTime: new Date(timeMs),
            segment,
        });
    };

    const onMouseLeave = () => setHover(null);

    const hoverLeftPct = hover
        ? ((hover.pointerTime.getTime() - startMs) / total) * 100
        : 0;

    return (
        <div className="w-full">
            <div className="flex justify-between text-xs text-slate-500 mb-1 px-0.5">
                <span>00:00</span>
                <span>06:00</span>
                <span>12:00</span>
                <span>18:00</span>
                <span>24:00</span>
            </div>
            <div
                ref={barRef}
                className="relative h-14 w-full rounded-md overflow-hidden border border-slate-300 bg-slate-100 cursor-crosshair"
                onMouseMove={onMouseMove}
                onMouseLeave={onMouseLeave}
            >
                {segments.map((seg, idx) => {
                    const left = ((new Date(seg.start).getTime() - startMs) / total) * 100;
                    const width =
                        ((new Date(seg.end).getTime() - new Date(seg.start).getTime()) / total) * 100;
                    const isActive = seg.kind === "Active";
                    return (
                        <div
                            key={`${seg.start}-${idx}`}
                            className={`absolute top-0 bottom-0 ${
                                isActive ? "bg-emerald-500" : "bg-slate-300"
                            }`}
                            style={{ left: `${left}%`, width: `${Math.max(width, 0.15)}%` }}
                        />
                    );
                })}

                {hover && (
                    <>
                        <div
                            className="pointer-events-none absolute top-0 bottom-0 w-0.5 bg-slate-800/70 z-10"
                            style={{ left: `${hoverLeftPct}%` }}
                        />
                        <div
                            className="pointer-events-none fixed z-50 min-w-[200px] max-w-xs rounded-md bg-slate-900 text-white text-xs shadow-lg px-3 py-2"
                            style={{
                                left: Math.min(hover.clientX + 14, window.innerWidth - 220),
                                top: Math.max(8, hover.clientY - 72),
                            }}
                        >
                            <div className="font-semibold text-sm mb-1">
                                {formatClock(hover.pointerTime)}
                            </div>
                            <div
                                className={
                                    hover.segment.kind === "Active"
                                        ? "text-emerald-300"
                                        : "text-slate-300"
                                }
                            >
                                {hover.segment.kind}
                            </div>
                            <div className="mt-1 text-slate-200">
                                {formatClockShort(hover.segment.start)} –{" "}
                                {formatClockShort(hover.segment.end)}
                            </div>
                            <div className="text-slate-200">
                                Duration: {hover.segment.durationFormatted} (
                                {hover.segment.hours.toFixed(2)}h)
                            </div>
                            {hover.segment.kind === "Active" && hover.segment.reason && (
                                <div className="mt-1 text-slate-400">{hover.segment.reason}</div>
                            )}
                        </div>
                    </>
                )}
            </div>
            <div className="flex gap-4 mt-2 text-sm">
                <span className="flex items-center gap-2">
                    <span className="inline-block w-3 h-3 rounded bg-emerald-500" /> Active
                </span>
                <span className="flex items-center gap-2">
                    <span className="inline-block w-3 h-3 rounded bg-slate-300" /> Inactive
                </span>
            </div>
        </div>
    );
};

const SessionDayPage = () => {
    const auth = useAppSelector((s) => s.authSlice);
    const router = useRouter();
    const userId = Number(router.query.userId);
    const dateQuery = typeof router.query.date === "string" ? router.query.date : "";

    const [date, setDate] = useState(dateQuery || toDateInputValue(new Date()));
    const [loading, setLoading] = useState(false);
    const [story, setStory] = useState<UserDayStoryline | null>(null);

    useEffect(() => {
        if (auth.isAuth && auth.role !== 4) {
            router.replace("/");
        }
    }, [auth.isAuth, auth.role, router]);

    useEffect(() => {
        if (dateQuery && dateQuery !== date) {
            setDate(dateQuery);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [dateQuery]);

    const load = useCallback(async () => {
        if (!userId || Number.isNaN(userId) || !date) return;
        setLoading(true);
        const res = await API.SESSIONS.GET_DAY(userId, date);
        setLoading(false);
        if (res && !res.error && res.data) {
            setStory(res.data);
        } else {
            setStory(null);
        }
    }, [userId, date]);

    useEffect(() => {
        if (auth.isAuth && auth.role === 4 && router.isReady) {
            void load();
        }
    }, [auth.isAuth, auth.role, router.isReady, load]);

    const onDateChange = (value: string) => {
        setDate(value);
        void router.replace(
            { pathname: `/sessions/${userId}`, query: { date: value } },
            undefined,
            { shallow: true }
        );
    };

    const title = useMemo(
        () => (story ? `${story.userName} — ${date}` : "Day storyline"),
        [story, date]
    );

    if (!auth.isAuth || auth.role !== 4) {
        return null;
    }

    return (
        <>
            <Head>
                <title>TMS - {title}</title>
            </Head>
            <div className="w-full max-w-none relative max-h-screen overflow-y-auto px-2 md:px-4 lg:px-6">
                <div className="bg-white border-solid border border-gray-300 rounded-b-md px-6 md:px-10 z-10 sticky top-0 left-0 right-0 py-4 w-full">
                    <div className="flex flex-wrap items-center justify-between gap-3 mb-4">
                        <div>
                            <Link href="/sessions" className="text-sm text-blue-600 hover:underline">
                                ← Back to sessions
                            </Link>
                            <h1 className="font-bold text-2xl mt-1">
                                {story?.userName || "User"} — Day storyline
                            </h1>
                        </div>
                        <label className="flex flex-col text-sm">
                            Date
                            <input
                                type="date"
                                className="border border-gray-300 rounded px-2 py-1"
                                value={date}
                                onChange={(e) => onDateChange(e.target.value)}
                            />
                        </label>
                    </div>

                    {loading && <p className="text-slate-500">Loading…</p>}

                    {!loading && story && (
                        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-2">
                            <div className="rounded border border-slate-200 bg-slate-50 px-4 py-3">
                                <div className="text-xs text-slate-500">Active hours</div>
                                <div className="text-2xl font-semibold text-emerald-700">
                                    {story.activeHours.toFixed(2)}h
                                </div>
                            </div>
                            <div className="rounded border border-slate-200 bg-slate-50 px-4 py-3">
                                <div className="text-xs text-slate-500">Inactive hours</div>
                                <div className="text-2xl font-semibold text-slate-600">
                                    {story.inactiveHours.toFixed(2)}h
                                </div>
                            </div>
                            <div className="rounded border border-slate-200 bg-slate-50 px-4 py-3">
                                <div className="text-xs text-slate-500">Sessions</div>
                                <div className="text-2xl font-semibold">{story.sessionCount}</div>
                            </div>
                            <div className="rounded border border-slate-200 bg-slate-50 px-4 py-3">
                                <div className="text-xs text-slate-500">Date</div>
                                <div className="text-2xl font-semibold">{date}</div>
                            </div>
                        </div>
                    )}
                </div>

                {!loading && story && (
                    <div className="mt-4 space-y-4 pb-8 w-full">
                        <div className="bg-white border border-gray-300 rounded-md p-6 md:p-8 w-full">
                            <h2 className="font-semibold text-lg mb-4">24-hour storyline</h2>
                            <DayBar
                                segments={story.segments}
                                dayStart={story.dayStart}
                                dayEnd={story.dayEnd}
                            />
                        </div>

                        <div className="bg-white border border-gray-300 rounded-md p-6 md:p-8 w-full">
                            <h2 className="font-semibold text-lg mb-4">Timeline</h2>
                            <ol className="relative border-l border-slate-300 ml-3 space-y-4">
                                {story.segments.map((seg, idx) => {
                                    const isActive = seg.kind === "Active";
                                    return (
                                        <li key={`${seg.start}-${idx}`} className="ml-6">
                                            <span
                                                className={`absolute -left-1.5 mt-1.5 h-3 w-3 rounded-full ${
                                                    isActive ? "bg-emerald-500" : "bg-slate-400"
                                                }`}
                                            />
                                            <div
                                                className={`rounded-md border px-4 py-3 ${
                                                    isActive
                                                        ? "border-emerald-200 bg-emerald-50"
                                                        : "border-slate-200 bg-slate-50"
                                                }`}
                                            >
                                                <div className="flex flex-wrap items-center justify-between gap-2">
                                                    <span
                                                        className={`text-sm font-semibold ${
                                                            isActive ? "text-emerald-800" : "text-slate-700"
                                                        }`}
                                                    >
                                                        {isActive ? "Active" : "Inactive"}
                                                    </span>
                                                    <span className="text-sm text-slate-600">
                                                        {formatClockShort(seg.start)} –{" "}
                                                        {formatClockShort(seg.end)} ·{" "}
                                                        {seg.durationFormatted} ({seg.hours.toFixed(2)}h)
                                                    </span>
                                                </div>
                                                {isActive && seg.reason && (
                                                    <p className="text-sm text-slate-600 mt-1">{seg.reason}</p>
                                                )}
                                            </div>
                                        </li>
                                    );
                                })}
                            </ol>
                        </div>

                        <div className="bg-white border border-gray-300 rounded-md p-6 md:p-8 w-full">
                            <h2 className="font-semibold text-lg mb-4">Sessions that day</h2>
                            {story.sessions.length === 0 ? (
                                <p className="text-slate-500">No sessions on this day.</p>
                            ) : (
                                <div className="overflow-x-auto w-full">
                                    <table className="min-w-full w-full text-sm">
                                        <thead>
                                            <tr className="text-left border-b border-slate-200">
                                                <th className="py-2 pr-4">Login</th>
                                                <th className="py-2 pr-4">Logout</th>
                                                <th className="py-2 pr-4">Duration</th>
                                                <th className="py-2 pr-4">Hours</th>
                                                <th className="py-2">Reason</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            {story.sessions.map((s) => (
                                                <tr key={s.id} className="border-b border-slate-100">
                                                    <td className="py-2 pr-4">{formatDateTime(s.loginAt)}</td>
                                                    <td className="py-2 pr-4">{formatDateTime(s.logoutAt)}</td>
                                                    <td className="py-2 pr-4">{s.durationFormatted}</td>
                                                    <td className="py-2 pr-4">
                                                        {s.totalHours == null ? "—" : s.totalHours.toFixed(2)}
                                                    </td>
                                                    <td className="py-2">{s.reason || "Active"}</td>
                                                </tr>
                                            ))}
                                        </tbody>
                                    </table>
                                </div>
                            )}
                        </div>
                    </div>
                )}

                {!loading && !story && (
                    <div className="mt-4 bg-white border border-gray-300 rounded-md p-6 text-slate-500 w-full">
                        Could not load day details.
                    </div>
                )}
            </div>
        </>
    );
};

export default SessionDayPage;

import Link from "next/link";

export type Crumb = { label: string; href?: string };

const CurriculumBreadcrumb = ({ items }: { items: Crumb[] }) => (
    <nav className="text-sm text-slate-600 mb-4 flex flex-wrap items-center gap-x-2 gap-y-1">
        {items.map((item, i) => (
            <span key={`${item.label}-${i}`} className="flex items-center gap-2">
                {i > 0 && <span className="text-slate-400">/</span>}
                {item.href ? (
                    <Link href={item.href} className="text-blue-600 hover:underline">
                        {item.label}
                    </Link>
                ) : (
                    <span className="font-medium text-slate-800">{item.label}</span>
                )}
            </span>
        ))}
    </nav>
);

export default CurriculumBreadcrumb;

import type { NextRouter } from "next/router";

/** Current URL path without query/hash — safe on dynamic `[param]` routes. */
export function currentPath(router: NextRouter): string {
    return router.asPath.split("?")[0].split("#")[0];
}

/** Close a query-driven modal and stay on the same page. */
export function dismissFormModal(router: NextRouter): void {
    router.push(currentPath(router));
}

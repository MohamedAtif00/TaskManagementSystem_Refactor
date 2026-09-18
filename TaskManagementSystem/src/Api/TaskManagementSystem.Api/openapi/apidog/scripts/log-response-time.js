// Apidog Public Script: TMS — Log Response Time
// Attach as a Post-processor on timing scenario steps (GET or all-transactions).
// Appends { method, path, operationId, status, ms } to environment variable queryTimings.

var entry = {
  method: pm.request.method,
  path: pm.request.url.getPath(),
  operationId: pm.info && pm.info.operationName ? pm.info.operationName : "",
  status: pm.response.code,
  ms: pm.response.responseTime
};

var timings = JSON.parse(pm.environment.get("queryTimings") || "[]");
timings.push(entry);
pm.environment.set("queryTimings", JSON.stringify(timings));

console.log("[timing] " + entry.method + " " + entry.path + " → " + entry.status + " (" + entry.ms + " ms)");

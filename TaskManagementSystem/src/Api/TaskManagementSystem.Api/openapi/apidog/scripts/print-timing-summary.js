// Apidog Public Script: TMS — Print Query Timing Summary
// Attach as the final step of get-queries-timing.json or all-transactions-timing.json.

var timings = JSON.parse(pm.environment.get("queryTimings") || "[]");

if (timings.length === 0) {
  throw new Error("No timings recorded. Ensure log-response-time.js ran on scenario steps.");
}

timings.sort(function (a, b) {
  return b.ms - a.ms;
});

console.log("=== TMS Request Timing Summary (" + timings.length + " requests) ===");
console.table(
  timings.map(function (t) {
    return {
      ms: t.ms,
      status: t.status,
      method: t.method || "",
      path: t.path,
      operationId: t.operationId || ""
    };
  })
);

var totalMs = timings.reduce(function (sum, t) {
  return sum + t.ms;
}, 0);

var successCount = timings.filter(function (t) {
  return t.status >= 200 && t.status < 300;
}).length;

console.log(
  "Total: " + totalMs + " ms | Avg: " + Math.round(totalMs / timings.length) + " ms | 2xx: " + successCount + "/" + timings.length
);

pm.test("At least one request returned 2xx", function () {
  pm.expect(successCount).to.be.above(0);
});

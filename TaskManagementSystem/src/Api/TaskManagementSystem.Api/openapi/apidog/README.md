# Sync the API endpoints in Apidog

Import `../tms-openapi.json` as OpenAPI/Swagger into the existing TMS API module. This is the complete source: 160 business HTTP operations, 61 JSON request-body definitions and 62 media examples (including the leave-request multipart form).

In the import options:

1. Select **Overwrite** for matching endpoints (matched by method and path) to update schemas and request examples.
2. Select **Delete (Sync)** for resources absent from the imported file to remove obsolete endpoints from the target TMS module.
3. Review the import preview and apply it.

Use the combined `tms-openapi.json` for this full synchronization. `hr-openapi.json` (48 operations) and `forgot-clock-openapi.json` (13 operations) are subsets including Auth; using either as a whole-module replacement would remove the other modules' endpoints.

See [Apidog import options](https://docs.apidog.com/import-options-633930m0) for the matching and deletion rules.

## Request bodies

Every endpoint that consumes JSON has a schema and an explicit JSON example. Requests that consume no body are intentionally bodyless; see `endpoint-inventory.md` for the complete list. Refresh and logout accept an optional `refreshToken` JSON property if the cookie is unavailable. The cookie takes precedence.

For leave requests, select JSON or multipart/form-data; attach the medical certificate as a file in the multipart body. Replace example IDs and dates with valid data for the selected environment. Task-bank type is numeric (0 = Creation, 1 = Review), and subject status is numeric (0 = Active, 1 = Closed, 2 = Hold, 3 = Reopened).

Import `environment.local-dev.postman.json` through the environment importer and use the existing login token post-processor. Timing scenarios are separate test artifacts; their import does not synchronize the endpoint catalog. They exercise state-changing operations and are not a guarantee that every sample succeeds against arbitrary data.

These files were checked against the current local registration chain and request contracts. The remote Apidog project has not been modified by this file update.

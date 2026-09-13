// Apidog Public Script: TMS — Save Access Token
// Attach as a Post-processor on POST /auth/login and POST /auth/refresh-token.
// Scope: Environment variable `accessToken` (used by bearerAuth as {{accessToken}}).

pm.test("Login/refresh returns 200", function () {
  pm.response.to.have.status(200);
});

const body = pm.response.json();

if (!body || !body.accessToken) {
  throw new Error("Response JSON missing accessToken field.");
}

pm.environment.set("accessToken", body.accessToken);
console.log("accessToken saved to environment (length: " + body.accessToken.length + ")");

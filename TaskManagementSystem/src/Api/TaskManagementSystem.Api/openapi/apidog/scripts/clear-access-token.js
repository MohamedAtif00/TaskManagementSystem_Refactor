// Apidog Public Script: TMS — Clear Access Token
// Attach as a Post-processor on POST /auth/logout.

pm.test("Logout returns 204", function () {
  pm.response.to.have.status(204);
});

pm.environment.unset("accessToken");
console.log("accessToken cleared from environment");

# Admin OIDC timeout investigation and rollout

## Evidence (2026-11-10, Asia/Shanghai)

- 14:28:18: `/auth` failed after 60 seconds with IDX20803/IDX20804 while fetching
  `https://auth.1mingo.com/master/.well-known/openid-configuration`.
- The inner stack was reading TLS / chunked response content. This proves a
  backchannel HTTP timeout, not a password rejection. It does not isolate the
  exact failing hop in the public proxy/Tunnel path.
- YARP also recorded a 100-second upstream timeout for `/master/clients`.
- Cloudflare's sampled Security Events did not show a matching block during the
  incident window. Sampling is not proof that every edge request succeeded.
- Correlation-cookie failure at 14:26:46 is a separate problem; this change does
  not claim to fix old or missing login cookies.

## Configuration

Configure the Admin container with:

```dotenv
IdserverBaseUrl=https://auth.1mingo.com
BackchannelBaseUrl=http://idserver:5001
BackchannelTimeoutSeconds=15
IsForwardedEnabled=true
```

`idserver` must resolve from the Admin container on its private Docker network.
For the checked-in compose files use `IDSERVER_BASE_URL`,
`ADMIN_BACKCHANNEL_BASE_URL`, and `ADMIN_BACKCHANNEL_TIMEOUT_SECONDS` instead.
The deployment observed during diagnosis uses different container names; verify
the private DNS alias before rollout. Do not point the private URL at the Admin
service or the public Cloudflare URL.

Only server-side OIDC requests matching the public provider origin and base path
are rewritten. Realm paths and query parameters are preserved. Discovery, JWKS,
code redemption and userinfo all use the same transport. Browser authorization
URLs, metadata issuer and token validation remain public. The private HTTP hop
is intended for the trusted service network; HTTPS can also be configured.
Certificate-validation behavior is inherited from the existing Admin settings.
No retry is added to token exchanges, avoiding replay of authorization codes.

The upstream Website API client used for realm/resource calls is separate from
the OIDC client. This change does not reroute its API calls; the observed 100s
`/master/clients` timeout should be checked separately after the login fix.

An empty `BackchannelBaseUrl` retains the public route. Restoring it to empty is
the configuration rollback. The former timeout can be restored with
`BackchannelTimeoutSeconds=60`.

## Debug logs

The startup log reports effective transport and timeout. Per-request logs record
method, public host/path, destination host, status, duration and activity TraceId.
Failures identify `response_headers` versus `response_body`, exception types and
cancellation. Query strings, request/response bodies, cookies and tokens are not
logged by the new diagnostics.

For header timing and Cloudflare Ray IDs on the public route, temporarily set:

```dotenv
Serilog__MinimumLevel__Override__MinGo.IdServer.Admin.OidcBackchannelHandler=Debug
```

There is no Cloudflare Ray ID when the request goes directly to IdServer. Existing
framework/upstream logs are independent and may include full authentication URLs;
avoid enabling blanket authentication Trace logging.

## Build and verify

The new `.github/workflows/admin-build.yml` uses GitHub Actions on push, PR and
manual dispatch. It builds Admin, runs unit tests and builds the Admin Docker
image, publishing SHA-labelled images to `ghcr.io/fffirer/identity-admin` and uploading a TRX report.
Pull requests build without publishing. The workflow does not deploy production. The existing `.gitea` workflows
are unchanged; their presence is not evidence of a GitHub build.

1. Push the change to the intended GitHub repository and wait for the workflow
   to succeed. Record the run URL and exact commit SHA.
2. Pin the published GHCR image digest in the deployment repository and
   apply the private URL settings above as part of the deployment.
3. Start a new browser login and confirm discovery and JWKS complete promptly;
   then verify token and userinfo calls and the final authenticated Admin page.
4. Match request TraceIds and paths against IdServer logs. A successful `/readyz`
   response alone does not verify OIDC.

The local checkout originally has only a Gitea remote. A GitHub target and working
GitHub authentication are required before the workflow can be run remotely.

Local verification: Admin Release build passed; 11 focused
`OidcBackchannelHandlerTests` passed; actionlint 1.7.7 accepted the GitHub
workflow. `scripts/smoke-admin-oidc.py` launches the real Admin process against a
loopback provider: realm discovery/JWKS completed and returned a public
authorization redirect in 0.20s; a response-body stall with a 1s test timeout
failed in 1.17s and emitted the `response_body` diagnostic. These are controlled
local checks, not production login or GitHub-run evidence.

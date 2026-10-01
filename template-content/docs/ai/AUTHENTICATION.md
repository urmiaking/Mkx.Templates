# Authentication and authorization

Interactive application pages run in WASM without prerendering. Account/login pages are static SSR. Never inject client-only JS/storage providers into SSR account flows. Shared Routes injects the framework AuthenticationStateProvider, which resolves to the server provider during SSR and the client provider in WASM. Client-only network retry UI must be conditional on the actual provider type; never register the WASM provider on Server to satisfy shared routing. Account GET integration tests cover Login and ForgotPassword. HttpOnly Identity cookies authenticate same-origin API requests.

AppPolicies -> AppPolicyProvider -> AuthorizationPolicyProvider is shared by Client and Server. Standard capability definitions require an authenticated user and the corresponding claim. Unknown or unregistered policy names return no policy and framework authorization rejects them. Tests cover missing providers, typo names, anonymous callers, missing claims and read-versus-manage access. Client hiding is only UI behavior; server enforcement is required.

The WASM PersistentAuthenticationStateProvider fetches auth-state, refreshes every five minutes and supports explicit retry. Transport failures retain the last known principal and expose HasConnectionError; confirmed anonymous/401 clears it. Server authorization remains authoritative during network failure. Its lifetime loop and cancellation are disposed with the scope.

Cookie events are configured after Identity defaults, preserving security-stamp validation. Validation interval is five minutes; user/role claim changes invalidate stamps rather than copying old principal claims into renewed cookies. Existing UI claims can remain until the next refresh, so sensitive changes should prompt re-login when appropriate.

BootstrapAdmin defaults to disabled and never contains fixed credentials. Opt-in configuration supplies username and a strong password through secrets. Identity minimum password length is 12, with at least four unique characters. Login failures lock out by default. Bootstrap creation is idempotent by username; it does not reset or promote an existing account silently.

The administrative screen accepts only built-in roles, protects administrator accounts from deletion/demotion through generic user management, and resets contact verification when contact details change. Do not key administrator protections to a literal username.

MVC mutations require antiforgery; logout is POST. The client obtains a fresh token for each mutation. SSR forms retain their framework antiforgery support. Auth-state/token responses and API responses are no-store; PWA never caches them.

Phone changes resolve the current authenticated user and require that user's confirmed existing number; a client-supplied number must never select another account. Validate the new number before sending and start the per-user cooldown only after the provider accepts the message. Recovery requires a confirmed phone and honors Identity lockout; invalid recovery codes count as failed access attempts. Do not log phone numbers or codes.

See [OPERATIONS.md](OPERATIONS.md) for rate limits, proxy trust, SMS and deployment credentials.

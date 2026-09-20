# Authentication & Authorization Architecture

Normal interactive pages run in Interactive WebAssembly without prerendering. Authentication/account surfaces may use static SSR. Never assume an interactive client service exists on a static SSR page.

Inspect the current authentication-state provider and Account endpoints before changing auth behavior; do not create a parallel auth-state store.

Policy-based authorization is preferred:
```text
AppPolicies -> AppPolicyProvider -> runtime policy provider
            -> server authorization + client/page checks
            -> RoleSeeder grants registered policy claims to Administrators
```
A policy constant alone is insufficient; register it in the provider.

User claim editing manages direct claims; role claims are separate. Preserve unrelated Identity claims and reject unknown policy names.

For protected functionality: define policy, register it, protect server endpoints, protect/condition client actions, verify administrator seeding, and test authorized/unauthorized behavior. Client hiding is never security. Existing sessions can carry stale claims, so re-authenticate during verification when needed.

Never expose secrets/tokens to WASM, trust client authorization for server operations, weaken authorization to fix UI/session issues, or invent a parallel auth protocol without a requirement.

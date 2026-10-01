# 0006: Explicit persistence and host boundaries

Status: accepted.

Identity models stay in SDK Domain under `Identity/`, with their existing namespace and EF model names. Core Domain may reference them through SDK Domain for user relationships. Only Microsoft.Extensions.Identity.Stores is needed for the model base classes; EF configuration, DbContext, concrete stores and migrations remain in Infrastructure. Tests reject EF/Infrastructure references from Domain assemblies and verify AppUser belongs to SDK Domain.

Application keeps the existing Infrastructure repository/specification dependency to avoid a second abstraction layer and wholesale CQRS rewrite. This is pragmatic layered architecture, not strict Clean Architecture dependency inversion. Pure business invariants remain in Domain, transport contracts in Shared, HTTP/cookie adapters in Server.

UserAccountService remains a Server host adapter due to SignInManager/HttpContext/cookie workflows. Move independent business rules out as they develop; do not move host-dependent code into Application merely to match a directory diagram. The complete Test CRUD feature is the authoritative extension reference.

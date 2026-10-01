# ADR 0001: Clean Architecture and DDD Boundaries

**Status:** Accepted

Domain owns business rules; Infrastructure persistence; Application use cases; Shared contracts; Server HTTP/hosting; Client Blazor presentation. This keeps business logic independent of UI, HTTP and database frameworks. Do not move business rules into controllers/components or persistence concerns into Domain/Shared merely to reduce file count.

Clarified by [0006](0006-persistence-and-host-boundaries.md): Application deliberately references Infrastructure. Shared Identity models belong to SDK Domain so Core Domain can use AppUser/AppRole relationships; EF contexts, mappings and store implementations remain in Infrastructure.

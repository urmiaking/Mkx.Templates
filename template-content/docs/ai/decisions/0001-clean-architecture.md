# ADR 0001: Clean Architecture and DDD Boundaries

**Status:** Accepted

Domain owns business rules; Infrastructure persistence; Application use cases; Shared contracts; Server HTTP/hosting; Client Blazor presentation. This keeps business logic independent of UI, HTTP and database frameworks. Do not move business rules into controllers/components or persistence concerns into Domain/Shared merely to reduce file count.

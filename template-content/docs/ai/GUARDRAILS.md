# Agent Guardrails

- Do not introduce MediatR/CQRS frameworks, another unit-of-work layer, or a new architecture merely because it is familiar.
- Do not bypass Shared service contracts, inject repositories/DbContext into controllers, access EF from Client/Shared, or put business invariants in controllers/UI.
- Do not replace reusable specifications with duplicated service LINQ, expose aggregate setters for convenience, add a second mapping approach without need, hardcode routes, or duplicate auto-scanned DI.
- Make the smallest coherent change; avoid unrelated refactors and generated `bin/`, `obj/`, publish output, caches.
- Never commit secrets or machine-private configuration.
- Before adding a package, verify framework/existing dependencies cannot solve the need.
- Keep `AGENTS.md` concise; detailed durable knowledge belongs in `docs/ai/`.
- Never claim build/test/migration/browser/runtime verification unless actually executed. If blocked, state exactly what remains unverified.

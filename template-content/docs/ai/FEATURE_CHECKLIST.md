# Feature Implementation Checklist

## Preflight
- [ ] Read `AGENTS.md` and relevant guides; search the closest existing feature.
- [ ] Identify affected layers, persistence, authorization and UI.

## Domain & Infrastructure
- [ ] Aggregate/entity/value objects, strong ID and domain invariants.
- [ ] Domain tests for meaningful rules.
- [ ] EF configuration including conversions, precision, relationships, indexes and delete behavior.
- [ ] Specifications/repository changes only as needed.
- [ ] Migration created and inspected for schema changes.

## Shared & Application
- [ ] Immutable DTOs and shared service contract.
- [ ] `ApiRoutes`, `ApiUrls`, `ClientRoutes` as applicable.
- [ ] Policy constants/definitions if protected.
- [ ] Application service, FluentValidation, Mapster and cancellation propagation.

## Server & Client
- [ ] Thin authorized controller using shared routes.
- [ ] Client service using `ApiUrls` and standard HTTP exception flow.
- [ ] Blazor markup + code-behind using `AppComponentBase` and `SendRequestAsync`.
- [ ] Loading/error/empty/responsive/theme/accessibility behavior.
- [ ] NavMenu entry only if appropriate.

## Verification
- [ ] Build affected projects and full solution; run relevant tests.
- [ ] Verify migration/runtime UI/auth paths when applicable.
- [ ] Search for duplicate DI/hardcoded routes.
- [ ] Update durable documentation when behavior/conventions changed.

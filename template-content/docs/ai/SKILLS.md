# Agent Skills & Compact Rules: Mkx.Templates

Use this file for high-frequency rules. Detailed guidance lives in the linked docs.

## Domain
- Protect aggregate state with private/protected setters.
- Use factory/mutation methods for invariants.
- Prefer strong typed IDs for domain entities.
- Use `Guid.CreateVersion7()` for new Guid-backed IDs unless requirements dictate otherwise.
- See [CONVENTIONS.md](CONVENTIONS.md).

## API & Routing
- Controllers are thin.
- Never hardcode application routes when `ApiRoutes`, `ApiUrls`, or `ClientRoutes` applies.
- Enforce authorization on the server; client visibility is not a security boundary.

## UI Service Calls
- Non-trivial pages use markup + code-behind.
- Inherit from `AppComponentBase` where application requests are made.
- Use `TryRequestAsync` (explicit success; legacy `SendRequestAsync` remains for compatibility) rather than ad-hoc request try/catch flows.
- Propagate cancellation tokens.

## Native UI & Styling
- Prefer semantic HTML and existing Components/UI primitives.
- Use the established central `app.css` for project-wide custom styling unless an existing component intentionally uses scoped CSS.
- Use --ui-* theme tokens instead of hardcoded theme colors.
- Static SSR pages must not depend on interactive scoped client services.
- See [UI_GUIDE.md](UI_GUIDE.md).

## Responsive Navigation
The source implementation in `Layout/MainLayout.razor` and `wwwroot/js/navigation.js` is authoritative for responsive navigation behavior.
- Desktop: fixed 238px sidebar with full labels; no mini state or palette selector.
- Mobile below the configured breakpoint: Temporary/overlay behavior; it must not permanently consume body width.
- Do not copy stale parameter values from documentation.

## Mobile Input Modes
- Decimal quantities/weights/percentages: decimal input mode and LTR numeric direction.
- Phone: tel input mode and LTR direction.
- OTP/2FA/security codes: reuse UiOtpField; one native numeric input, six decorative cells, LTR order and full-code paste.

## Persistence
- Reusable queries use specifications.
- Explicitly configure important EF relationships, precision, indexes and delete behavior.
- Inspect generated migrations.
- See [DATABASE.md](DATABASE.md).

## Before Finishing
- Search for accidental duplication/hardcoded routes.
- Build affected projects and the solution as appropriate.
- Run relevant tests.
- Verify runtime UI/auth/migrations when the change requires it.
- Never report an unexecuted verification as passed.
- See [VERIFICATION.md](VERIFICATION.md).

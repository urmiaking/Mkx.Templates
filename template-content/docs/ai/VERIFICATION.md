# Verification and definition of done

Palette regressions exercise all four palettes in light/dark, contrast, circular new-snapshot reveals and suppression of competing transitions, mode/palette persistence, mouse and real browser touch-event dragging, drag-vs-click separation, saved position after reload, viewport clamping, keyboard placement, close-animation reversal, Escape/focus restoration, light dismiss, reduced motion and 320/375px sheet placement. Mobile action bounds have equal 44px targets and 8px spacing. Screenshots remain outside the template package; browser touch emulation is not a physical-device performance measurement.

```powershell
dotnet build Mkx.Templates.slnx
dotnet test --solution Mkx.Templates.slnx
./scripts/verify.ps1
```

verify.ps1 builds Release, executes tests using the Microsoft.Testing.Platform runner selected by global.json, generates TRX with xUnit's built-in reporter, checks TRX executed count >0 and runs dependency-free Node PWA, OTP and appearance behavior tests. Run these commands from the generated solution root; SDK test-runner selection depends on the current directory. Root packaging verification separately performs pack -> inspect -> isolated install -> generate/build/test both Acme.Starter (sample included) and Acme.Minimal (`--IncludeSample false`), rejecting leaked sample source in the minimal variant. An exit code zero with zero discovered tests is a failure. The SQL Server model/snapshot and migration-script checks run without opening a database connection; they also assert sample table inclusion/omission.

| Change | Required evidence |
|---|---|
| Business invariant | Positive/negative domain tests |
| Use case/API | Integration tests including validation, missing records and errors |
| Authorization | Anonymous 401, missing claim 403, correct policy allowed, typo/missing provider rejected |
| Contract | Full solution and generated-name build |
| EF schema | Inspected Up/Down, persistence test, no pending model changes; actual SQL deployment where available |
| UI | Navigate real flow; loading/empty/error/retry, save failure retained, duplicate submit and dirty cancel |
| Responsive/style | Fixed desktop sidebar after refresh, mobile overlay and raised center action, light/dark, keyboard, OS reduced motion and dialog scroll |
| PWA | Cache allowlist/privacy, own-prefix cleanup, offline fallback, deliberate update and dirty guard |
| Deployment | Configuration, migration strategy, health and image startup when Docker is available |

The SQLite integration suite uses actual repositories/validators/mapping and MVC middleware, with a test-only authentication scheme and a separate database per factory. It is not proof of SQL Server migration compatibility; inspect/generate the SQL migration script too.

The optional real-browser regression suite is `Tests/browser/ui-smoke.mjs`. With Playwright available and the local development server running, set `UI_BASE_URL` and run `node Tests/browser/ui-smoke.mjs` from the solution root. `UI_PLAYWRIGHT_MODULE` can identify an existing Playwright installation; `UI_BROWSER_CHANNEL` selects a system browser and `UI_ARTIFACTS` optionally saves screenshots outside the project. It intercepts API calls with fixture data and makes no database mutations. It checks native validation, dirty cancel, failed-save retry, pagination/search, account page rendering, static SSR field names, full-height notification behavior, responsive navigation, dark mode, OTP normalization/completion/resend and reduced motion. This supplements the real database/API tests; it does not exercise hardware passkeys, live SMS/OIDC providers or target-phone performance.

The browser suite detects whether the sample is present and skips its editor/paging flow when omitted. It verifies that the page body containing the Users table participates in entry motion, Persian numeral shaping is inherited, explicit theme changes reveal a circular View Transition (including keyboard activation), appearance persists after reload, and reduced motion changes themes immediately. Run the theme checks in a browser supporting View Transitions; the dependency-free appearance suite separately covers unsupported-browser fallback, rejected snapshots and interruption.

The reverse-theme regression deliberately delays snapshot removal beyond the circle animation and asserts that the old dark snapshot remains clipped to zero. This exposes the end-of-animation flash even when the normal browser timing only shows it for one frame. The appearance unit suite also checks that the retained animation is released after snapshot removal.

Route-motion checks cover account and Users headers/bodies (including the table) at 375px portrait, 844px landscape, 768px tablet and the 960px desktop boundary. Home and SSR identity cards also enter on mobile. Reduced-motion checks separately assert that page roots, headers, bodies and identity cards disable their animations. Viewport emulation verifies behavior/layout, not real-device frame rate.

The UI suite advances the browser clock through the five-minute authentication refresh and delays its response, checking that Home retains its DOM identity and expanded guide both during and after polling. Auth-state unit tests also cover pending I/O, reordered/unchanged claims, changed capabilities, connection failures and logout. Role permissions checks cover flat list layout, aligned search/bulk actions at 1440px and 375px, compact tree rows, search, global selection, failed-save retention and successful payloads without category nodes.

PWA VM tests exercise worker/helper behavior but do not substitute for real browser service-worker installation/update. Browser layout checks and real-device frame measurements have separate purposes. Do not claim mobile FPS or deployment success from compile-only checks. Record any missing environment/device/provider explicitly.

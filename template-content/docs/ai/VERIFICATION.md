# Verification and definition of done

```powershell
dotnet build Mkx.Templates.slnx
dotnet test --solution Mkx.Templates.slnx
./scripts/verify.ps1
```

verify.ps1 builds Release, executes tests using the Microsoft.Testing.Platform runner selected by global.json, generates TRX with xUnit's built-in reporter, checks TRX executed count >0 and runs dependency-free Node PWA and OTP behavior tests. Run these commands from the generated solution root; SDK test-runner selection depends on the current directory. Root packaging verification separately performs pack -> inspect -> isolated install -> generate Acme.Starter -> build/test. An exit code zero with zero discovered tests is a failure.

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

PWA VM tests exercise worker/helper behavior but do not substitute for real browser service-worker installation/update. Browser layout checks and real-device frame measurements have separate purposes. Do not claim mobile FPS or deployment success from compile-only checks. Record any missing environment/device/provider explicitly.

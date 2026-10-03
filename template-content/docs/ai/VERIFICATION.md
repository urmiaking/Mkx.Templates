# Verification and definition of done

```powershell
dotnet build Mkx.Templates.slnx
dotnet test --solution Mkx.Templates.slnx
./scripts/verify.ps1
```

verify.ps1 builds Release, executes tests using the Microsoft.Testing.Platform runner selected by global.json, generates TRX with xUnit's built-in reporter, checks TRX executed count >0 and runs dependency-free Node PWA behavior tests. Run these commands from the generated solution root; SDK test-runner selection depends on the current directory. Root packaging verification separately performs pack -> inspect -> isolated install -> generate Acme.Starter -> build/test. An exit code zero with zero discovered tests is a failure.

| Change | Required evidence |
|---|---|
| Business invariant | Positive/negative domain tests |
| Use case/API | Integration tests including validation, missing records and errors |
| Authorization | Anonymous 401, missing claim 403, correct policy allowed, typo/missing provider rejected |
| Contract | Full solution and generated-name build |
| EF schema | Inspected Up/Down, persistence test, no pending model changes; actual SQL deployment where available |
| UI | Navigate real flow; loading/empty/error/retry, save failure retained, duplicate submit and dirty cancel |
| Responsive/style | Desktop initial mini state after refresh, mobile overlay, light/dark, keyboard, OS reduced motion and dialog scroll |
| PWA | Cache allowlist/privacy, own-prefix cleanup, offline fallback, deliberate update and dirty guard |
| Deployment | Configuration, migration strategy, health and image startup when Docker is available |

The SQLite integration suite uses actual repositories/validators/mapping and MVC middleware, with a test-only authentication scheme and a separate database per factory. It is not proof of SQL Server migration compatibility; inspect/generate the SQL migration script too.

PWA VM tests exercise worker/helper behavior but do not substitute for real browser service-worker installation/update. Browser layout checks and real-device frame measurements have separate purposes. Do not claim mobile FPS or deployment success from compile-only checks. Record any missing environment/device/provider explicitly.

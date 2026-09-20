# Verification & Definition of Done

Code generation is not completion.

```powershell
dotnet build Mkx.Templates.slnx
dotnet test
```

| Change | Minimum verification |
|---|---|
| Domain/business rule | Build + relevant unit tests |
| Application/service | Build + relevant tests |
| Shared contract | Full build; verify implementations |
| API | Build + integration/runtime check when available |
| EF schema | Build + create/inspect migration + persistence tests |
| Blazor UI | Client/full build + runtime UI check |
| Responsive UI | Desktop + mobile viewport |
| Authorization | Authorized + unauthorized paths + registration |
| CSS/theme | Light/dark and responsive when relevant |
| Docker/deployment | Image/startup/health check when environment permits |

For UI, navigate the real flow and check relevant loading/success/empty/failure states. For migrations inspect Up/Down, destructive operations, nullability, FKs, indexes, precision and delete behavior. For policies verify constant, provider, server enforcement, client behavior and seeding.

Never report an unexecuted check as passed. If blocked, name the blocker exactly.

# Mkx.Templates.Blazor Template Repository

This repository packages the `template-content/` application as a .NET project template.

For changes to the generated application, the authoritative agent entry point is [template-content/AGENTS.md](template-content/AGENTS.md). Read that file and the task-relevant documents under `template-content/docs/ai/`.

## Repository-Level Rules
- Treat `template-content/` as the source that consumers receive when instantiating the template.
- Keep template packaging concerns at the repository root and application architecture guidance inside `template-content/`.
- Do not duplicate the generated application's full architecture manual in this root file; duplication causes documentation drift.
- When a change affects generated application behavior, update the documentation inside `template-content/`.
- Never edit `bin/`, `obj/`, package caches, or other generated build output.

## Template Verification
After modifying generated content, verify the generated solution:
```powershell
dotnet build template-content/Mkx.Templates.slnx
dotnet test template-content/Mkx.Templates.slnx
```

For packaging/install/instantiation commands, see the root [README.md](README.md).

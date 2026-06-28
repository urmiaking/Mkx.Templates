# Developer Guide for AI Agents: Mkx.Templates

Welcome to the `Mkx.Templates` codebase. This document is the primary entry point for AI coding agents to understand the workspace structure, architectural patterns, and development guidelines of this project.

> [!IMPORTANT]
> **CRITICAL RULE FOR AGENTS**:
> Whenever you implement a new feature, modify existing services, introduce refactorings, or make breaking changes, you **must** update the corresponding documentation files in the `docs/ai/` folder and this `AGENTS.md` file. Keeping documentation in sync with code is mandatory.

---

## 1. Documentation Map

To understand the project and build features correctly, refer to the following documents in the `docs/ai/` directory:

| Document | Purpose |
| :--- | :--- |
| **[Architecture Guide](file:///docs/ai/ARCHITECTURE.md)** | Explains the Clean Architecture layers, project relations, DDD boundaries, and query specification patterns. |
| **[Development Guide](file:///docs/ai/DEVELOPMENT_GUIDE.md)** | Step-by-step walkthrough for adding a new business feature/aggregate from scratch. |
| **[Service Architecture](file:///docs/ai/SERVICE_ARCHITECTURE.md)** | Covers Dependency Injection auto-scanning, client-server polymorphism, and exception/error handling. |
| **[Tools & CLI Guide](file:///docs/ai/TOOLS.md)** | Standard dotnet commands, EF migrations, build, and test execution details. |
| **[Agent Guidelines & Skills](file:///docs/ai/SKILLS.md)** | Coding style guidelines, rules for aggregates, styling patterns, and best practices. |
| **[Domain Knowledge](file:///docs/ai/DOMAIN_KNOWLEDGE.md)** | Reserved workspace containing the business glossary, policies, and domain logic (updated by human developers). |

---

## 2. Solution Structure at a Glance

This repository is built as a modern, clean-architecture template for .NET 10 Blazor Web Apps, using the XML-based `.slnx` solution configuration.

```
Mkx.Templates/
├── Mkx.Templates.slnx                 # XML-based solution configuration
├── docs/
│   └── ai/                            # Detailed AI guidance and architecture logs
│       ├── ARCHITECTURE.md
│       ├── DEVELOPMENT_GUIDE.md
│       ├── DOMAIN_KNOWLEDGE.md
│       ├── SERVICE_ARCHITECTURE.md
│       ├── SKILLS.md
│       └── TOOLS.md
├── src/
│   ├── Core/
│   │   ├── Mkx.Templates.Domain/          # Core Domain entities and aggregates
│   │   └── Mkx.Templates.Infrastructure/  # EF Core Context, configurations, repositories
│   ├── Server/
│   │   ├── Mkx.Templates.Application/     # Use case orchestrators, mappers, validators
│   │   └── Mkx.Templates.Server/          # Server API Controllers, Program.cs, CSS/Assets
│   ├── Client/
│   │   └── Mkx.Templates.Client/          # Blazor WASM views, client services, custom UI
│   ├── Shared/
│   │   └── Mkx.Templates.Shared/          # Shared interfaces, contracts, DTOs, routes
│   └── Sdk/                               # Shared base models and helper utilities
└── AGENTS.md                              # This main guide
```

---

## 3. Getting Started CLI Commands

Use the following commands to restore, compile, or run the project:

- **Restore NuGet Packages**:
  ```powershell
  dotnet restore
  ```
- **Build Solution**:
  ```powershell
  dotnet build Mkx.Templates.slnx
  ```
- **Run Backend Web API / Host**:
  ```powershell
  dotnet run --project src/Server/Mkx.Templates.Server/Mkx.Templates.Server.csproj
  ```

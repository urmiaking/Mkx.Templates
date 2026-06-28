# Tools & CLI Guide: Mkx.Templates

Use this guide to run, build, test, and manage EF Core migrations across the solution.

---

## 1. Project Build & Build Verification

To verify that the entire project compiles successfully:
```powershell
dotnet build Mkx.Templates.slnx
```

---

## 2. Managing Entity Framework Migrations

EF Core migrations should be run against the `Mkx.Templates.Infrastructure` project, pointing to `Mkx.Templates.Server` as the startup host:

- **Add a New Migration**:
  ```powershell
  dotnet ef migrations add <MigrationName> --project src/Core/Mkx.Templates.Infrastructure --startup-project src/Server/Mkx.Templates.Server
  ```
- **Remove Last Migration**:
  ```powershell
  dotnet ef migrations remove --project src/Core/Mkx.Templates.Infrastructure --startup-project src/Server/Mkx.Templates.Server
  ```
- **Apply Migrations directly to Database**:
  ```powershell
  dotnet ef database update --project src/Core/Mkx.Templates.Infrastructure --startup-project src/Server/Mkx.Templates.Server
  ```

*Note: In production and staging runs, the Server host automatically executes pending database migrations on startup using `app.ApplyDatabaseMigrations<AppDbContext>()`.*

---

## 3. Database Seeders

System and initial metadata should be seeded using the `IDbSeeder` scanning mechanism:
- Create a seeder class implementing `IDbSeeder` inside `Mkx.Templates.Application/Seeders/`.
- Decorate it with `[ScopedService]`.
- Define an execution order priority using the `Order` property.
- The startup task will automatically resolve all db seeders and run `SeedAsync` sequentially inside a database transaction scope.

---

## 4. Running Automated Tests

Run all unit and integration tests across the test project assemblies:
```powershell
dotnet test
```
To run targeted tests matching a specific test class:
```powershell
dotnet test --filter "FullyQualifiedName~[YourTestClass]"
```

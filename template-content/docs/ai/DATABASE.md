# Database & Persistence Guide

EF Core belongs in Infrastructure. Domain/Shared remain persistence-agnostic and Client never accesses DbContext.

Use `IEntityTypeConfiguration<T>` and explicitly configure keys, strong-ID conversions, required/optional columns, string lengths, decimal precision, relationships/FKs, indexes/uniqueness and delete behavior where integrity depends on them.

Reusable filtering/includes/order/paging/single-result intent belongs in specifications. Avoid duplicated predicates across services. Use no-tracking reads when supported and mutation is unnecessary.

Do not create feature repositories merely to rename generic CRUD. Add repository behavior only when existing repository/specification APIs cannot express it cleanly. Use established transaction boundaries for atomic use cases.

For migrations use `TOOLS.md`; inspect Up/Down, destructive operations, nullability, FK/delete behavior, indexes and precision. Do not rewrite deployed migration history as normal development.

Seeders use `IDbSeeder`, must be idempotent, and should not inject transient test data into production startup.

Use DB constraints/indexes for concurrency-critical invariants, consider optimistic concurrency for concurrent editing, page large results, avoid N+1 patterns and index actual query/filter/sort paths.

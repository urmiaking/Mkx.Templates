# Starting a product from the template

The template provides infrastructure and a tested development reference. Its default identity is a placeholder: when an agent creates a new product, replacing the default application icon is part of completing project setup.

## Product identity

- Choose a product-specific mark from the user's brief or supplied assets. If no asset is supplied, create a simple original mark appropriate to the product; do not keep the template's grid mark or invent a third-party logo.
- Replace `favicon.png`, `icon-192.png` and `icon-512.png` under `src/Server/Mkx.Templates.Server/wwwroot/` with consistent artwork at their actual declared dimensions. Keep icon silhouettes readable at favicon size and in light/dark browser surfaces.
- Update `src/Server/Mkx.Templates.Server/wwwroot/manifest.json` name, short name, colors and icon metadata; keep `src/Server/Mkx.Templates.Server/Components/App.razor` favicon and Apple touch-icon links consistent.
- Replace the mark and copy in `src/Client/Mkx.Templates.Client/Components/BrandLockup.razor`, identity-page branding and the splash screen as appropriate. Search for remaining template identity before finishing.
- Check favicon and application branding in a real browser, inspect manifest/icon requests, and check service-worker update behavior when PWA is enabled. Do not claim an installed-PWA check from source inspection alone.

Paths above are relative to the generated solution and use the template's original project names; project generation substitutes the actual solution name.

## Optional SampleItem reference

By default, `/sample-items` provides a tested CRUD reference called «نمونهٔ CRUD». `SampleItem` is a demonstration record, not an automated test or a product requirement. Its complete file map is in [DEVELOPMENT_GUIDE.md](DEVELOPMENT_GUIDE.md).

The template can also generate a solution using `--IncludeSample false`. That variant omits sample domain, repository, contracts, services, API, page/editor, policies, sample schema and sample-specific .NET checks. Shared Identity, authorization, UI primitives, PWA/OTP checks and the test project remain. Mobile's central action opens the account area.

If the reference is included, use it to establish the first real business feature. Once that feature has equivalent domain/API/UI checks, move the guide's reference links to it and remove the sample's routes, policies, UI, persistence and sample-specific checks together. Preserve infrastructure and independent regression tests. For a deployed database, use a reviewed migration to retire the sample table; do not rewrite applied migration history.

The renamed sample deliberately retains the existing SQL table name `Tests` so the rename itself does not delete or recreate records. Its historical migrations retain their original identities. New sample routes and policy names use `SampleItems`; existing consumers adopting this change must update navigation and permissions accordingly.

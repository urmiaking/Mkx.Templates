# Antigravity Developer Guide: Mkx.Templates.Blazor

Welcome to the Mkx.Templates.Blazor solution. This document summarizes key architecture patterns, custom styling mechanisms, UI components, and design decisions learned during development.

---

## 1. Project Overview & Architecture

This repository is a .NET 10 Blazor Web App template configured with the modern `.slnx` solution structure. It compiles cleanly with zero warnings or errors.

### Directory Structure

```
d:\source\Mkx.Templates.Blazor
├── template-content/
│   ├── Mkx.Templates.Blazor.slnx   # Modern XML-based solution configuration
│   └── src/
│       ├── Client/
│       │   └── Mkx.Templates.Client/
│       │       ├── Layout/
│       │       │   ├── Components/
│       │       │   │   ├── AppBar.razor
│       │       │   │   ├── Drawer.razor
│       │       │   │   └── NavMenu.razor
│       │       │   ├── Themes/
│       │       │   │   └── ColorPalettes.cs
│       │       │   ├── EmptyBoxLayout.razor  # Static SSR-safe layout
│       │       │   └── ReconnectModal.razor
│       │       └── ...
│       ├── Server/
│       │   └── Mkx.Templates.Server/
│       │       ├── wwwroot/
│       │       │   └── css/
│       │       │       └── app.css     # Central layout stylesheet
│       │       └── ...
│       ├── Core/
│       ├── Sdk/
│       └── Shared/
└── AGENTS.md                           # This Developer Guide
```

---

## 2. Design System & Styling (app.css)

All layout-specific, custom glassmorphic and animation styles are centralized in the Server's `app.css` file (`template-content/src/Server/Mkx.Templates.Server/wwwroot/css/app.css`) rather than scoped CSS files.

### Global CSS Best Practices
- **CSS Variables for Theming**: To ensure styles are connection-resilient and compatible with Server-Side Rendering (SSR) pages (like Login and AccessDenied), styles should be entirely pure-CSS based utilizing native MudBlazor variables such as `rgba(var(--mud-palette-primary-rgb), opacity)`.
- **Glassmorphism**: Premium iOS-like frosted-glass effects are created using:
  - `backdrop-filter: blur(40px) saturate(200%) !important;`
  - A subtle gradient background combining primary and surface variables at high transparency (e.g., `0.3` to `0.5`).
  - Solid border strokes and internal white inset shadows: `box-shadow: ..., inset 0 1px 0 rgba(255, 255, 255, 0.35)`.

---

## 3. Core UI Elements & Layouts

### Brand Text & Shine Animation
The brand text logo (e.g. `Mkx.Templates`) uses the `.brand-text` class which implements a premium glowing shine animation:
- **Gradient**: Bound to the active theme's primary color `var(--mud-palette-primary)` with a bright white shine in the center (`rgba(255, 255, 255, 0.9)`).
- **Animation**: The `shine` keyframe animation translates the `background-position` from `120% center` to `-20% center` over a `6s` duration.
- **Tighter Bounds**: Keeping translation range tight (e.g., `120%` to `-20%`) rather than wide offsets keeps the reflection active across the letters rather than spending half the time hidden off-screen.
- **Shorthand / Important Override**: Do not define `background-position` with `!important` on the class level, as it overrides and locks keyframe animation translations.

### Empty Box Layout (Static SSR Safe)
`EmptyBoxLayout.razor` is the layout wrapper for static SSR pages (Login, AccessDenied, NotFound).
- **No ThemeService Injection**: Because these pages render in static SSR mode, they cannot depend on interactive scoped client services like `ThemeService` in C#. Instead, the theme layout adapts natively through standard CSS variables.
- **Dynamic Background**: The background class `.box-bg` utilizes a radial gradient blending the theme's primary color into the main background:
  `radial-gradient(circle at 12% 12%, rgba(var(--mud-palette-primary-rgb), 0.15) 0%, var(--mud-palette-background) 75%)`
- **Glass Card**: The card is styled under `.glass-card` as a semi-translucent pane:
  - Light mode: Soft translucent white background.
  - Dark mode: Dark slate translucent background.

### Frosted Drawer
Defined in `Drawer.razor` with custom classes `.fancy-drawer.light-mode` and `.fancy-drawer.dark-mode`:
- **Light Mode Drawer**: Uses a translucent frosted appearance (`background-color: rgba(var(--mud-palette-surface-rgb), 0.45)`) combined with a primary border stroke.
- **Borders & Buttons**: In light mode, shortcut icon buttons (`.shortcut-btn`) have primary-tinted backgrounds and borders (`rgba(var(--mud-palette-primary-rgb), 0.22)`) so they stand out. The bottom exit button (`.close-drawer-btn`) uses a stronger divider border.

### Reconnect Modal
A custom connection-status handler in `ReconnectModal.razor` and `ReconnectModal.razor.css`:
- **UX Styling**: Uses a glassmorphic card with a backdrop blur overlay (`#components-reconnect-modal`).
- **Persian Copywriting**: Replaced default messages with polite, inline Persian copy (e.g., "اتصال شما قطع شده است. تلاش مجدد در X ثانیه...").
- **Display Mechanics**: Active state paragraph overrides (`display: block !important`) force the inline sentence flow while inactive states are hidden using (`display: none !important`).

---

## 4. Compilation & Verification

Before finalizing changes, compile the entire solution file:
```powershell
dotnet build template-content/Mkx.Templates.slnx
```
The client and server applications run seamlessly on the local development environment (`http://localhost:7007` and `https://localhost:7006`).

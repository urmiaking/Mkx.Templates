# UI & UX Guide

Use MudBlazor as the primary component system and preserve the existing theme-aware visual language. Reuse application components before custom equivalents.

## Pages and Styling
Non-trivial pages normally use shared `ClientRoutes`, authorization, `AppComponentBase`, separate markup/code-behind, and `SendRequestAsync`. Use central project CSS unless an existing component intentionally uses scoped CSS. Prefer MudBlazor palette variables and preserve light/dark, focus, hover, disabled and error states.

## Responsive Navigation
`Layout/Components/Drawer.razor` is authoritative for exact parameters. Desktop navigation is Mini/collapsible. Below the configured breakpoint it must be Temporary/overlay and must not permanently consume body width.

## Motion
Use the global page transition rather than competing per-page route animations. Motion must be short, non-blocking, avoid layout shift, and respect `prefers-reduced-motion`.

## Forms, Tables, Dialogs
Use established validation/error UX and prevent duplicate submits while busy. Use decimal/tel/numeric `InputMode` appropriately. Page/filter large datasets server-side. Provide loading/empty states and confirm destructive actions. Dialogs should be focused and return deterministic results.

## Accessibility and SSR
Give icon-only actions accessible names, preserve keyboard/focus behavior, do not rely only on color, and maintain contrast. Static SSR pages must not depend on interactive scoped client services.

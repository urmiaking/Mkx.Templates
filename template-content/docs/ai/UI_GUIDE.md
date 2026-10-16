# UI and interaction

Use semantic HTML, native Blazor controls and the primitives in `Components/UI`. The application has no external component-library runtime. The design follows the reference workspace: navy `#123D51`, teal `#187F87`, blue-grey background `#F4F8FB`, white opaque surfaces, rounded controls and 16â€“17px panels. Use central `app.css` and semantic `--ui-*` tokens, including persisted light/dark appearance. Dark mode uses charcoal surfaces, saturated jade action backgrounds and brighter mint links. Vazirmatn is the primary local font, with IRANSans as fallback; no remote font request is required.

Non-trivial pages use markup/code-behind, `AppComponentBase`, shared routes and policies. Use `TryRequestAsync` with explicit loading, empty, error and retry states. Client visibility never replaces server authorization.

## Forms
`UiTextField` renders labeled native inputs/textareas; `UiCheckBox` and `UiSelect` use native controls. Interactive `UiForm.ValidateAsync()` collects required, DataAnnotations property and existing FluentValidation field rules. `For` expressions integrate with an enclosing EditContext. Native `EditForm` + `InputText`/`InputCheckbox` serve static SSR account pages and preserve form field names, antiforgery and server form mapping. Do not inject interactive-only services into static SSR pages. Preserve password-manager autocomplete, paste and LTR numeric input modes.

Keep busy actions disabled, retain input on failed saves and show field/request errors. `Tests/TestEditor` demonstrates versioned updates, duplicate-submit guards and dirty navigation/cancel protection. A failed request must allow correction and retry.

The recovery phone field uses one bordered wrapper around its icon and native telephone input. Keep the input border and shadow reset more specific than the global input styles; show focus on the wrapper with `:focus-within`.

## OTP fields
Reuse `UiOtpField` for password recovery, phone-change verification and authenticator setup. It presents six visual cells over one labeled native input, preserving full-code paste, password-manager/SMS autofill, leading zeros, numeric keyboard and LTR order. Both JS and `OtpCode.Normalize` accept Persian/Arabic digits. Decorative cells are hidden from assistive technology. `Sms=false` prevents WebOTP requests for authenticator codes.

Static SSR recovery gives the input its native `Input.OtpCode` name and opts into native form autosubmit; request/resend/verify submitter names, antiforgery tokens and server-side cooldown/verification must remain intact. Interactive fields bind to the existing model; use `OnCompleted` only for a guarded verification action. The shared JS initializes on initial load, enhanced navigation and interactive insertion. Do not create six separately focusable inputs or block paste.

## Dialogs and messages
`DialogService` hosts dynamic component dialogs via `UiDialogHost`. `UiModal` uses native `dialog.showModal()` and the browser top layer, focus containment, Escape/backdrop policies and focus restoration from `js/modal.js`. Dirty editors disable direct backdrop/Escape closing and run their explicit discard confirmation. Nested confirmations must restore focus to the editor. The content scrolls within `dvh` bounds and the action footer remains reachable. `ToastService` displays dismissible live-region messages; messages contain actual outcomes.

## Tables
`UiTable<T>` renders a semantic table, with labeled stacked rows below 650px and server paging controls. It cancels stale requests and prevents stale results from replacing newer data. Keep initial pages <=25 rows and page sizes <=100. Search uses cancellable debounce. Sorting is server-side through `TableState`; mutations reload the current page, adjusting a now-empty last page. Every icon action needs an accessible name.

## Navigation and motion
MainLayout supplies a skip link and stable `#main-content` route-focus target. The desktop sidebar is fixed at 238px, with brand, primary create action, full nav labels and account controls. The 82px topbar lives inside the workspace. Below 960px, the sidebar slides over a scrim, traps keyboard focus, makes background content inert and closes on navigation/Escape. The mobile bottom bar has a raised central create action and safe-area padding. Its create link opens the actual Tests editor through the shared create query route. Notifications use a full-height native dialog at the left edge (420px desktop, full width on phones), with summary, list/empty state and footer. It is an extension surface; do not add fake counts or delivery settings without a notification service.

AccountShell provides a breadcrumb, overline, title, description, actions and grouped body. Account overview combines profile rows, security tiles and a separate sign-out section. Static SSR identity pages use a centered 440px card. Desktop page entry uses 340ms opacity with 380â€“500ms section reveals, without remounting route content. Mobile skips page entry; controls keep brief interaction feedback. Respect `prefers-reduced-motion` automatically. Avoid blur, continuous decorative motion, transition:all and layout animation.

## Verification
Run `scripts/verify.ps1`, then inspect desktop/mobile, light/dark, keyboard and reduced motion. Exercise SSR login/recovery field mapping, OTP request/cooldown/verification, interactive forms, sorting/search/page changes, failed save retry, dirty cancel, nested confirmation, modal focus and notification overlay. Build success alone does not prove browser behavior.

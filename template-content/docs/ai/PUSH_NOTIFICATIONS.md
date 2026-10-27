# Web Push notifications

Mkx.Templates.Blazor includes a **server-sent Web Push foundation**, adapted to the existing PWA worker (not a second worker). Push is opt-in per browser device and tied to the current authenticated account. The bell panel allows the user to enable/disable the current device; permission is requested only from a user click. An already granted permission may be restored on authenticated app startup. Explicit opt-out survives reload.

## Initial configuration

Each generated application **must have its own VAPID key pair**. Run from the generated solution root:

```powershell
./scripts/generate-vapid.ps1 -Subject "mailto:ops@your-domain.example"
```

The script creates `src/Server/Mkx.Templates.Server/appsettings.Push.local.json` with `Push:Enabled=true`, `Push:Subject`, `Push:PublicKey` and `Push:PrivateKey`. This file is gitignored and explicitly excluded from the `dotnet new` source and the template NuGet package. Do not rotate the keys once users have subscribed: existing browser subscriptions will require a fresh subscribe. The script refuses to overwrite existing keys without `-Force`.

On deployed servers, supply the same keys through secret-backed configuration or environment variables (`Push__Enabled`, `Push__Subject`, `Push__PublicKey`, `Push__PrivateKey`). The committed `appsettings.json` intentionally contains **disabled placeholders**, never shared VAPID private keys. The contact subject should be a reachable `mailto:` address or HTTPS URL. Keep the private key out of client assets, git history and logs. Avoid committing copied keys into a product repository. `Push:AllowedEndpointHosts` contains trusted browser push providers and can be extended by operators.

## Usage

1. Apply EF migration `AddWebPushSubscriptions` before first registration (startup migration applies when explicitly enabled). The `UserPushSubscriptions` table binds each endpoint to one account, has a unique SHA-256 endpoint hash, and cascades on user deletion.
2. Serve the app on **HTTPS** (or loopback localhost for development). The user opens the bell panel and opts into notifications. Safari on iOS/iPadOS requires an installed Home Screen web app for Web Push; the browser controls permission.
3. Inject `Mkx.Templates.Application.Abstractions.IWebPushSender` into an Application or Server-side service and send after the business transaction commits:

```csharp
using Mkx.Templates.Application.Abstractions;
using Mkx.Templates.Shared.DTOs.Push;

var result = await pushSender.SendToUserAsync(
    recipientUserId,
    new PushMessage("عنوان اعلان", "متن اعلان", "/account", "business-event-id"),
    cancellationToken);
```

`Accepted` means the push provider accepted the HTTP request, **not** that the device displayed the notification. `Revoked` covers provider 404/410 and invalid saved endpoints. `Unknown` represents ambiguous transport failure and must not be blindly retried. For business-critical events, wrap the call in an application-owned transactional outbox with idempotency and explicit delivery state; this template foundation does not include an outbox or recurring background dispatcher.

## API and security

All `/api/push` endpoints require authentication and the application's normal anti-forgery protection:

- `GET /api/push/configuration`: exposes only whether push is configured and the **public** VAPID key.
- `POST /api/push/subscriptions`: validates the endpoint and stores/rebinds the current user's device.
- `DELETE /api/push/subscriptions`: revokes only the current user's endpoint.

There is **no arbitrary send-to-user API**. Only trusted server-side code invokes `IWebPushSender`. Trusted push endpoint hosts are allowlisted to mitigate SSRF; endpoint URLs never come from arbitrary external targets. Notification click URLs resolve to the same origin. Logout unsubscribes the current browser before removing the authenticated session; a new login silently rebinds an already permitted device. Maximum 20 devices per user.

The service worker preserves the existing cache privacy rules (no HTML, API data or credentials are cached), explicit update activation and offline fallback. Notification content is browser-visible: don't include secrets or confidential business data. Tests and builds cannot prove receipt on a physical device or in every browser; verify a real HTTPS deployment and representative browsers/devices separately.

## Verification

```powershell
./scripts/verify.ps1
dotnet ef migrations has-pending-model-changes --context AppDbContext --project src/Core/Mkx.Templates.Infrastructure --startup-project src/Server/Mkx.Templates.Server
```

The generated-app template packaging flow also verifies that no local VAPID settings file ships with the package.

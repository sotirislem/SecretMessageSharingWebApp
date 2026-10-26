# SecretMessageSharingWebApp

Create an encrypted message, share a single-use link, and let the recipient read it once.
The message is encrypted and decrypted in the browser with **AES-256-GCM**; the server only ever
stores ciphertext and a hash of the key. When the message is read it is deleted, and unread
messages expire automatically after one hour.

> **Live app:** <https://secret-message-sharing-web-app.azurewebsites.net/>

---

## Why this exists

Sending a password over chat or email leaves it in multiple histories forever. This app gives you
a one-off link instead: the secret is encrypted on your device, the key travels only in the URL
fragment (the part after `#`, which browsers never send to the server), and the server copy is
destroyed on first read.

It is a good fit for passwords, API keys, connection strings, recovery codes, and short
confidential notes. It is not a document vault, and it cannot protect a compromised device or a
link sent to the wrong person.

## Features

- **Local encryption** &mdash; AES-256-GCM via the native Web Crypto API (`SubtleCrypto`), with a
  fresh 96-bit random IV per message.
- **Zero-knowledge storage** &mdash; the server stores the IV, ciphertext, metadata, and a SHA-256
  hash of the key. It never receives the key or the plaintext.
- **Single retrieval** &mdash; the ciphertext is deleted immediately after a successful read.
- **Automatic expiry** &mdash; unread messages are removed after one hour.
- **File attachments** &mdash; any file type up to 1 MB, encrypted with the message.
- **Optional credentials** &mdash; username and password fields are encrypted into the same payload.
- **Optional OTP** &mdash; a one-time code is emailed to the recipient, valid for 3 minutes with
  3 validation attempts. The expiry shown in the email is rendered in the recipient's own time zone
  when the browser supplies it.
- **Delivery notifications** &mdash; the sender's browser receives a SignalR notification when the
  recipient opens the message.
- **Delivery details by email** &mdash; optionally have the same delivery details emailed to the
  sender: when the message was created and opened, how long it stayed unread, where it was opened,
  and the recipient's masked IP, device, network provider, time zone, browser language and OTP use.
- **Delivery location** &mdash; delivery details show where the message was opened: the city,
  region and country (with a flag), the network provider, the recipient's time zone and browser
  language, and how long the message stayed unread. The city comes from device location when the
  recipient grants browser permission, and is otherwise estimated from the IP address.
- **Session history** &mdash; a browser-session list of messages you created, with delivery details
  and the option to delete an unread message early.

## Security model

| Concern | How it is handled |
| --- | --- |
| Message confidentiality | AES-256-GCM, encrypted and decrypted only in the browser |
| Key exposure | Key is generated locally, kept in the URL fragment, never sent to the server |
| Wrong-key retrieval | The server compares a SHA-256 key hash supplied in the `X-Key-Hash` header |
| Tampering | GCM authentication tag rejects modified ciphertext |
| Replay / reuse | Message is deleted on first successful retrieval |
| Abandoned messages | Background cleaner removes unread messages after 1 hour |
| Transport | HTTPS, HSTS in production, strict security headers and CSP |
| Brute force | Rate limiting, short OTP lifetime, limited OTP attempts |
| Caching | API and SignalR responses are marked `no-store` |
| Referrer leakage | `Referrer-Policy: no-referrer`; the fragment is never sent in requests |
| Location disclosure | Coordinates are rounded in the browser, sent as request headers (never in the URL), and never stored &mdash; only the resolved city, region, country and network provider are kept |
| Geolocation consent | Location is requested only after the recipient confirms retrieval; the browser prompt decides, and the IP estimate is the fallback when it is denied |

**What the server can see:** message ID, timestamps, IV + ciphertext, key hash, creator IP and
user-agent (logs kept 24 hours), the recipient email when OTP is enabled, the sender's optional
notification email, and delivery metadata after the message is read. Delivery metadata covers the
time and duration until the message was opened, the recipient's masked IP, browser and device type,
time zone, browser language, whether OTP protection was used, and the city, region, country and
network provider resolved for the recipient.

**What the server cannot see:** the plaintext, the attachment contents, the encryption key, the
recipient's exact device coordinates (they are rounded, used only for a city lookup, and never
stored), or the full URL including its fragment.

No web application can promise absolute security. This design removes the server as a source of
plaintext, but it cannot protect you from a compromised browser, a malicious extension, an
insecure recipient device, or sharing the link with the wrong person. Treat the full link as the
secret.

## Architecture

```
┌────────────────────────────── Browser ──────────────────────────────┐
│  Angular 22 SPA                                                     │
│  ├─ Web Crypto API  ──  AES-256-GCM encrypt / decrypt               │
│  ├─ QR code generation                                              │
│  ├─ SignalR client  ──  delivery notifications                      │
│  └─ Session storage ──  recently created messages                   │
└───────────────┬─────────────────────────────────────┬───────────────┘
                │ HTTPS (ciphertext + key hash only)   │ WebSocket
                ▼                                     ▼
┌────────────────────────────── .NET 10 ──────────────────────────────┐
│  FastEndpoints API                                                  │
│  ├─ SecretMessagesManager  ──  validation, key-hash check, deletion │
│  ├─ Recipient location     ──  device geocoding, IP fallback        │
│  ├─ OTP service            ──  SendGrid email delivery              │
│  ├─ SignalR hub            ──  creator notifications                │
│  ├─ Background service     ──  expiry + log cleanup                 │
│  └─ Security headers / rate limiting / sessions                     │
└───────────────────────────────┬─────────────────────────────────────┘
                                ▼
                    Azure Cosmos DB (encrypted payloads)
```

Apart from SendGrid email delivery, `RecipientLocationService` makes the only outbound calls:
reverse geocoding of the recipient's rounded coordinates (BigDataCloud) and IP geolocation as the
fallback and ISP source (ipwho.is). Both are optional, and either endpoint can be swapped for
another provider.

### Request flow

1. The browser generates a 256-bit key and a 96-bit IV, encrypts the message payload, and sends
   the ciphertext plus the SHA-256 key hash to `POST /api/secret-messages`.
2. The recipient opens `/get-secret-message?id=…<key in fragment>`. The SPA verifies the message
   exists and asks for confirmation. On confirmation it sends the key hash plus the browser's time
   zone, language, and device model when available (and, if the browser grants location access,
   rounded coordinates) as request headers. Retrieval waits for the recipient to allow or block the
   location prompt.
3. The server compares hashes, resolves a city-level location, returns the ciphertext, records the
   delivery details, and deletes the stored message.
4. The browser decrypts locally. If the sender's page is still open, SignalR reports delivery.

## Tech stack

**Client:** Angular 22 (standalone components, signals, zoneless change detection), Bootstrap 5,
ng-bootstrap, RxJS, `qrcode`, `@microsoft/signalr`, Web Crypto API.

**Server:** .NET 10, FastEndpoints, Entity Framework Core with the Cosmos DB provider, SignalR,
SendGrid, Application Insights (optional).

**Infrastructure:** Azure App Service, Azure Cosmos DB, SendGrid.

## Project layout

```
SecretMessageSharingWebApp/
├─ SecretMessageSharingWebApp/              # .NET 10 web application
│  ├─ ClientApp/                            # Angular 22 SPA
│  │  ├─ src/app/components/                # pages, modals, loader, toasts
│  │  ├─ src/app/services/                  # crypto, API, modal, toast, SignalR, location
│  │  ├─ src/styles.css                     # design system
│  │  └─ angular.json
│  ├─ Endpoints/                            # FastEndpoints routes
│  ├─ Services/                             # message, OTP, JWT, delivery and location logic
│  ├─ EmailTemplates/                       # embedded HTML email templates
│  ├─ Data/                                 # EF Core context and entities
│  ├─ Middlewares/                          # security headers, timing
│  └─ Program.cs
└─ SecretMessageSharingWebApp.UnitTests/    # xUnit + FluentAssertions
```

## API endpoints

| Method | Route | Purpose |
| --- | --- | --- |
| `POST` | `/api/secret-messages` | Store a new encrypted message |
| `GET` | `/api/secret-messages/verify/{id}` | Check whether a message exists and needs OTP |
| `GET` | `/api/secret-messages/{id}` | Retrieve and delete a message (requires `X-Key-Hash`) |
| `GET` | `/api/secret-messages` | List messages created in the current session |
| `DELETE` | `/api/secret-messages/{id}` | Delete an unread message early |
| `GET` | `/api/secret-messages/otp/{id}` | Request an OTP for a protected message |
| `POST` | `/api/secret-messages/otp/{id}` | Validate an OTP and receive a short-lived token |

`GET /api/secret-messages/{id}` also accepts optional request-context headers, all of which are
ignored when absent: `X-Client-Latitude` and `X-Client-Longitude` (rounded to 3 decimals by the
browser), `X-Client-Timezone` (IANA id), `X-Client-Utc-Offset-Minutes`, `X-Client-Language`, and
`X-Client-Device-Model` (Android device model when the browser exposes it; desktop browsers do not).

`GET /api/secret-messages/otp/{id}` reads `X-Client-Timezone` and `X-Client-Utc-Offset-Minutes` too,
so the emailed code can show its expiry in the recipient's time zone. It falls back to the UTC offset
when the IANA id is missing or unknown, and to UTC when neither header is usable.

Swagger UI is available in development at `/swagger/index.html`.

## Local setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 24+](https://nodejs.org/) and npm 11+
- A Cosmos DB instance: the [Azure Cosmos DB Emulator](https://learn.microsoft.com/azure/cosmos-db/local-emulator)
  is the easiest local option
- HTTPS development certificate: `dotnet dev-certs https --trust`

### Configuration

Local settings live in `SecretMessageSharingWebApp/appsettings.Development.json`:

```json
{
  "CosmosDb": {
    "Endpoint": "https://localhost:8081",
    "ApiKey": "<cosmos-emulator-key>",
    "DbName": "secret-messages-db"
  },
  "SendGrid": {
    "ApiKey": "<sendgrid-api-key>",
    "AuthSender": "<sender@your-authenticated-domain.com>"
  },
  "Jwt": {
    "SigningKey": "<base64-key>"
  },
  "RecipientLocation": {
    "Enabled": true,
    "TimeoutSeconds": 2.5,
    "IpCacheMinutes": 1440,
    "ResolveLocalRequestsThroughPublicIp": true
  }
}
```

- The Cosmos DB emulator's well-known key works as-is for local development.
- SendGrid is used for OTP email and the optional delivery-details email. With a debugger attached
  the OTP is printed to the server console and delivery emails are skipped, so you can test the
  flow without an account.
- Both emails are rendered from the HTML files in `EmailTemplates/`, which are embedded into the
  assembly at build time. They ship with the published app, are never read from disk at runtime,
  and an edit only takes effect after a rebuild.
- `SendGrid:AuthSender` should be an address on a domain authenticated in SendGrid (domain
  authentication with the SPF and DKIM records SendGrid provides). Single Sender Verification of a
  consumer address (for example `@outlook.com`) is accepted by SendGrid, but strict receivers such
  as Gmail reject it with `550 5.7.40` because the From domain does not align with SendGrid's
  authenticated domain.
- SendGrid calls are guarded by a Polly pipeline: transient responses (`408`, `429`, `500`, `502`,
  `503`, `504`) and network/timeout failures are retried up to 3 times with jittered exponential
  backoff starting at 1 second. Every attempt gets its own 3-second timeout, so a retry is not
  starved by the previous attempt, and a send that fails on every attempt returns after roughly
  19 seconds. Permanent failures such as a bad API key are not retried. See
  `Providers/SendGridResiliencePipeline.cs`.
- `RecipientLocation` controls the delivery-city lookup. Its defaults live in `appsettings.json`;
  the block above shows those defaults plus the development-only
  `ResolveLocalRequestsThroughPublicIp` override. Requests from your own machine report
  `localhost`, so that switch asks the IP provider to resolve your egress address instead. Set
  `Enabled` to `false` to skip the lookup entirely.
- The lookup is capped at 2.5 seconds so it never delays a retrieval noticeably, and IP results
  are cached for 24 hours. `ReverseGeocodeEndpoint` and `IpGeolocationEndpoint` can be pointed at
  another provider.
- For production, never commit secrets. Use Azure App Service application settings, user secrets,
  or Key Vault. `Jwt:SigningKey` must be a strong base64 value.

### Run

Restore and start the API and the Angular dev server:

```bash
# terminal 1 — API on https://localhost:7275
dotnet run --project SecretMessageSharingWebApp

# terminal 2 — SPA on https://localhost:44490
cd SecretMessageSharingWebApp/ClientApp
npm install
npm start
```

Open <https://localhost:44490>. The SPA proxies API calls to the .NET host during development.

Alternatively, run only `dotnet run` and let the ASP.NET Core SPA proxy launch `npm start` for you.

## Tests

```bash
# .NET unit tests
dotnet test

# Angular production build
cd SecretMessageSharingWebApp/ClientApp
npx ng build --configuration production

# Angular unit tests (headless Chrome)
npx ng test --watch=false --browsers=ChromeHeadless
```

The .NET test project covers validators, mappings, services (including recipient location
resolution, request-header parsing, SendGrid email rendering and the retry pipeline), security
headers, and the key-hash verifier. Angular tests cover the components, services, guards,
interceptors, pipes, and string extensions.

## Deployment

1. Create an Azure App Service (Linux, .NET 10) and an Azure Cosmos DB account.
2. Configure app settings: `CosmosDb__Endpoint`, `CosmosDb__ApiKey`, `CosmosDb__DbName`,
   `SendGrid__ApiKey`, `SendGrid__AuthSender`, `Jwt__SigningKey`. `RecipientLocation` is optional
   and enabled by default; override `RecipientLocation__Enabled`,
   `RecipientLocation__TimeoutSeconds`, `RecipientLocation__IpCacheMinutes`, or either endpoint
   only if you need to. Keep `ResolveLocalRequestsThroughPublicIp` at `false` in production.
3. Publish the app:

   ```bash
   dotnet publish SecretMessageSharingWebApp -c Release -o ./publish
   ```

   The Angular production build is part of the publish output and is served from `wwwroot`.
4. Ensure HTTPS-only is enabled. HSTS, security headers, and `no-store` API caching are applied by
   the application.

## Notes and limitations

- The Cosmos DB container is created on startup with `EnsureCreatedAsync`.
- Message cleanup runs in a background service; unread messages expire after one hour.
- Delivery location is resolved after a successful retrieval by two external services: rounded
  device coordinates are reverse geocoded (BigDataCloud), and the recipient IP is geolocated as the
  fallback and as the ISP source (ipwho.is). Only the resulting city, region, country, network
  provider and the source (`device` or `ip`) are stored; coordinates are never logged or persisted.
- The browser asks for location permission only when the recipient confirms retrieval. If the
  prompt is denied, ignored, unsupported, or times out, only the time zone and language are sent
  and the city falls back to the IP estimate. An ignored prompt is abandoned after 30 seconds so
  the retrieval cannot stall.
- The lookup is best-effort: a provider error or timeout leaves the location fields empty and never
  blocks the retrieval. If the recipient disconnects while it runs, the one-time message is not
  consumed. Delivery details recorded before location tracking show "Not available".
- Delivery notifications are process-local (in-memory connection map and pending queue), which
  assumes a single application instance. A queued notification is kept for 15 minutes, the same
  lifetime as the browser session history, and is replayed when the sender's browser reconnects
  with the same client id.
- Every browser tab also has its own id, kept in `sessionStorage` so it survives a refresh. When
  the creator opens their own message, the notification is sent only to the tab that created it;
  the other tabs of the same browser are left alone.
- The recipient email address for OTP is stored server-side so the code can be delivered. The
  sender's optional notification email is stored for the same reason: to send the delivery-details
  email when the message is opened. Sending that email is best-effort, so a SendGrid failure is
  logged but never blocks or fails the retrieval.
- Attachments are limited to 1 MB before Base64 encoding; the server caps the encrypted
  payload at 1.5 MB (decoded ciphertext) and the request body at 3 MiB. Cosmos DB enforces a
  2 MB item limit, which is the practical ceiling: a 1 MB attachment produces a document of
  roughly 1.9 MB. The browser also checks the encrypted payload size before sending and reports
  a friendly error when the message text and attachment together are too large.
- No accounts, no analytics profiles, and no third-party trackers in the browser. The only
  outbound third-party calls are the optional server-side location lookups described above.

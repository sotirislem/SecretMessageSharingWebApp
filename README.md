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
  3 validation attempts.
- **Delivery notifications** &mdash; the sender's browser receives a SignalR notification when the
  recipient opens the message.
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

**What the server can see:** message ID, timestamps, IV + ciphertext, key hash, creator IP and
user-agent (logs kept 24 hours), the recipient email when OTP is enabled, and delivery metadata
after the message is read.

**What the server cannot see:** the plaintext, the attachment contents, the encryption key, or the
full URL including its fragment.

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
│  ├─ OTP service            ──  SendGrid email delivery              │
│  ├─ SignalR hub            ──  creator notifications                │
│  ├─ Background service     ──  expiry + log cleanup                 │
│  └─ Security headers / rate limiting / sessions                     │
└───────────────────────────────┬─────────────────────────────────────┘
                                ▼
                    Azure Cosmos DB (encrypted payloads)
```

### Request flow

1. The browser generates a 256-bit key and a 96-bit IV, encrypts the message payload, and sends
   the ciphertext plus the SHA-256 key hash to `POST /api/secret-messages`.
2. The recipient opens `/get-secret-message?id=…<key in fragment>`. The SPA verifies the message
   exists, asks for confirmation, and sends the key hash in the `X-Key-Hash` header.
3. The server compares hashes, returns the ciphertext, and deletes the stored message.
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
│  │  ├─ src/app/services/                  # crypto, API, modal, toast, SignalR
│  │  ├─ src/styles.css                     # design system
│  │  └─ angular.json
│  ├─ Endpoints/                            # FastEndpoints routes
│  ├─ Services/                             # message, OTP, JWT, delivery logic
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
    "AuthSender": "<verified-sender@example.com>"
  },
  "Jwt": {
    "SigningKey": "<base64-key>"
  }
}
```

- The Cosmos DB emulator's well-known key works as-is for local development.
- SendGrid is only needed for OTP email. With a debugger attached the OTP is printed to the
  server console instead, so you can test the flow without an account.
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

The .NET test project covers validators, mappings, services, security headers, and the key-hash
verifier. Angular tests cover the route guard, crypto service, and string extensions.

## Deployment

1. Create an Azure App Service (Linux, .NET 10) and an Azure Cosmos DB account.
2. Configure app settings: `CosmosDb__Endpoint`, `CosmosDb__ApiKey`, `CosmosDb__DbName`,
   `SendGrid__ApiKey`, `SendGrid__AuthSender`, `Jwt__SigningKey`.
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
- The recipient email address for OTP is stored server-side so the code can be delivered.
- Attachments are limited to 1 MB before Base64 encoding; the server caps the encrypted
  payload at 1.5 MB (decoded ciphertext) and the request body at 3 MiB. Cosmos DB enforces a
  2 MB item limit, which is the practical ceiling: a 1 MB attachment produces a document of
  roughly 1.9 MB. The browser also checks the encrypted payload size before sending and reports
  a friendly error when the message text and attachment together are too large.
- No accounts, no analytics profiles, no third-party trackers.

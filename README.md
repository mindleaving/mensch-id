# 웃ID - Mensch.ID

Your global ID

## Using mensch.ID to sign people in to your service

External services (websites, apps, kiosks, …) can let people sign in with their mensch.ID. The person enters their mensch.ID at your service, and you create a *challenge* for it. The person approves or rejects the challenge in the mensch.ID app, and your service receives a **signed result token** (a JWT) that proves the decision.

Results are delivered over a connection that **your service opens**: a SignalR hub, or polling. mensch.ID never calls your servers, so services on private networks work too.

The examples below use `https://mensch.id` as the API base URL.

### Overview

```
 Person            Your service                     mensch.ID API                  mensch.ID app
   │  enters ID  ───▶ │                                    │                              │
   │                  │ POST /api/service-challenges ────▶ │                              │
   │                  │ ◀──── challenge_id, app_link       │                              │
   │ ◀─ QR code/link ─│                                    │                              │
   │ ─────────────────────── scans QR / taps link ──────────────────────────────────────▶ │
   │                  │                                    │ ◀── accept / reject ──────── │
   │                  │ ◀── ChallengeAnswered (hub)        │                              │
   │                  │     or POST …/result (polling)     │                              │
   │                  │ verify result_token, sign person in│                              │
```

1. **Register** your service once. You receive a `client_id` and a `client_secret`.
2. **Confirm** the e-mail address from the verification link. An administrator then approves the service, and your credentials start working.
3. For each sign-in, **create a challenge**, show its link as a QR code or tappable link, and **wait for the result** via the hub or by polling.
4. **Verify the result token** and sign the person in.

### 1. Register your service

You can register on the website (About → "Use mensch.ID for login") or through the API:

```http
POST /api/externalServices
Content-Type: application/json

{
  "displayName": "Example Shop",
  "email": "it@example-shop.org",
  "contactInformation": "Example Shop Ltd.\nMain Street 1\n12345 Example City"
}
```

- `displayName` (required, max. 100 characters) and `contactInformation` (optional, max. 1000 characters, multi-line) are shown to people in the mensch.ID app when you ask them to approve a sign-in.
- `email` (required) receives a verification link. Your service can only be approved after the address is verified.

Response `200 OK`:

```json
{
  "id": "6f1c2b0e-8a4d-4f3e-9b7a-2d5c1e0f9a33",
  "client_id": "0b9e4c7a-3f21-4d8e-a6b5-91c2d7e4f810",
  "client_secret": "q3Zk2l0yJm8sV1n4cXr7tB9wHf6uPeDaGo5iLxCvNbE"
}
```

> **Store the `client_secret` securely now. It is returned only once.** The credentials are rejected until an administrator has approved your service.

If the verification e-mail got lost, request a new one:

```http
POST /api/externalServices/{id}/resend-verification-email
```

#### Rotating the client secret

```http
POST /api/externalServices/rotate-secret
Content-Type: application/json

{
  "client_id": "0b9e4c7a-3f21-4d8e-a6b5-91c2d7e4f810",
  "client_secret": "<current secret>"
}
```

```json
{ "client_secret": "<new secret>" }
```

The old secret stops working immediately.

### 2. Create a challenge

When a person enters their mensch.ID at your service:

```http
POST /api/service-challenges
Content-Type: application/json

{
  "client_id": "0b9e4c7a-3f21-4d8e-a6b5-91c2d7e4f810",
  "client_secret": "q3Zk2l0yJm8sV1n4cXr7tB9wHf6uPeDaGo5iLxCvNbE",
  "mensch_id": "19900101-ACDEF",
  "description": "Sign in to Example Shop\nCustomer account: jane.doe",
  "state": "c2b7f0a1e9d34b6a"
}
```

| Field | Description |
|---|---|
| `mensch_id` | The mensch.ID the person entered (`yyyyMMdd-XXXXX`). |
| `description` | Shown in the app. Explain what the person is approving. Multi-line, max. 2000 characters. |
| `state` | Optional, max. 512 characters. An opaque value of your choice, for example tied to the browser session. It comes back as the `nonce` claim in the result token. |

Response `200 OK`:

```json
{
  "challenge_id": "K7QW2MZP9XHT4ARVJ3N6YBC8LDGE5SUFK7QW2MZP9XHT4ARVJ3N6",
  "expires_at": "2026-11-14T10:05:00Z",
  "app_link": "menschid://service-challenge/K7QW2MZP9XHT4ARVJ3N6YBC8LDGE5SUFK7QW2MZP9XHT4ARVJ3N6"
}
```

- Show `app_link` as a **QR code** if the person uses another device, or as a **link** if they are on their phone. The mensch.ID app handles both.
- `challenge_id` is 52 characters from `ABCDEFGHJKLMNPQRSTUVWXYZ23456789`, which is about 260 bits of entropy.
- Challenges **expire after 5 minutes** and can be answered only once.
- The response looks the same whether or not the mensch.ID exists, so you can't use it to probe which IDs are in use.

| Status | Meaning |
|---|---|
| `400` | Invalid mensch.ID or missing description |
| `401` | Wrong credentials, or the service isn't approved (yet) |
| `429` | Too many pending challenges for this mensch.ID |

### 3a. Receive the result via the SignalR hub (recommended)

First get a short-lived access token with the OAuth 2.0 client credentials grant:

```http
POST /api/oidc/token
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials&client_id=0b9e4c7a-3f21-4d8e-a6b5-91c2d7e4f810&client_secret=q3Zk2l0yJm8sV1n4cXr7tB9wHf6uPeDaGo5iLxCvNbE
```

```json
{
  "access_token": "eyJhbGciOi…",
  "token_type": "Bearer",
  "expires_in": 900
}
```

Then connect to the hub at `https://mensch.id/hubs/service-challenges`. Use the access token as a bearer token, or as the `access_token` query parameter for WebSockets. Fetch a new token whenever the connection (re)connects. You receive the answers to **your own** challenges only.

The hub sends `ChallengeAnswered` with this payload. Enums are sent as strings.

```json
{
  "challenge_id": "K7QW2MZP9XHT4ARVJ3N6YBC8LDGE5SUFK7QW2MZP9XHT4ARVJ3N6",
  "status": "Answered",
  "result_token": "eyJhbGciOiJSUzI1NiIsImtpZCI6Ij…"
}
```

JavaScript example (`@microsoft/signalr`):

```ts
import { HubConnectionBuilder } from '@microsoft/signalr';

const getToken = async () => {
    const response = await fetch('https://mensch.id/api/oidc/token', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({ grant_type: 'client_credentials', client_id: CLIENT_ID, client_secret: CLIENT_SECRET })
    });
    return (await response.json()).access_token as string;
};

const connection = new HubConnectionBuilder()
    .withUrl('https://mensch.id/hubs/service-challenges', { accessTokenFactory: getToken })
    .withAutomaticReconnect()
    .build();
connection.on('ChallengeAnswered', (result: { challenge_id: string; result_token: string }) => {
    handleResult(result.challenge_id, result.result_token);
});
await connection.start();
```

Hub messages are not guaranteed to arrive, for example while you are reconnecting. Use polling (below) as a fallback.

### 3b. Receive the result by polling

```http
POST /api/service-challenges/{challenge_id}/result
Content-Type: application/json

{
  "client_id": "0b9e4c7a-3f21-4d8e-a6b5-91c2d7e4f810",
  "client_secret": "q3Zk2l0yJm8sV1n4cXr7tB9wHf6uPeDaGo5iLxCvNbE"
}
```

| `status` | Meaning |
|---|---|
| `Pending` | Not answered yet. Poll again in a few seconds. |
| `Answered` | `result_token` is set (accepted **or** rejected; check its `result` claim). |
| `Expired` | Not answered within 5 minutes. |

```json
{
  "challenge_id": "K7QW2MZP9XHT4ARVJ3N6YBC8LDGE5SUFK7QW2MZP9XHT4ARVJ3N6",
  "status": "Answered",
  "result_token": "eyJhbGciOiJSUzI1NiIsImtpZCI6Ij…"
}
```

The endpoint returns `404` for challenges created by another service.

### 4. Verify the result token

The result token is a JWT in ID-token format, signed by mensch.ID. It is **not** an access token and can't be used to call the mensch.ID API.

Decoded payload:

```json
{
  "iss": "https://mensch.id/api/oidc/",
  "aud": "0b9e4c7a-3f21-4d8e-a6b5-91c2d7e4f810",
  "sub": "19900101-ACDEF",
  "challenge_id": "K7QW2MZP9XHT4ARVJ3N6YBC8LDGE5SUFK7QW2MZP9XHT4ARVJ3N6",
  "result": "accepted",
  "nonce": "c2b7f0a1e9d34b6a",
  "iat": 1794650460,
  "nbf": 1794650460,
  "exp": 1794650760
}
```

Before signing the person in, check all of the following:

1. The **signature**, against the keys at `https://mensch.id/api/oidc/.well-known/jwks`. The OIDC discovery document is at `https://mensch.id/api/oidc/.well-known/openid-configuration`.
2. `iss` is `https://mensch.id/api/oidc/`.
3. `aud` is **your** `client_id`.
4. `exp` / `nbf` (the token is valid for 5 minutes).
5. `challenge_id` is the challenge you created for this sign-in attempt.
6. `nonce` is the `state` you sent, if you sent one.
7. `result` is `accepted`. `rejected` means the person declined.

`sub` is the person's mensch.ID. No other personal data is included.

Example in Node.js with [`jose`](https://github.com/panva/jose):

```ts
import { createRemoteJWKSet, jwtVerify } from 'jose';

const jwks = createRemoteJWKSet(new URL('https://mensch.id/api/oidc/.well-known/jwks'));

const verifyResult = async (resultToken: string, expectedChallengeId: string, expectedState: string) => {
    const { payload } = await jwtVerify(resultToken, jwks, {
        issuer: 'https://mensch.id/api/oidc/',
        audience: CLIENT_ID
    });
    if(payload.challenge_id !== expectedChallengeId || payload.nonce !== expectedState) {
        throw new Error('Result token does not belong to this sign-in attempt');
    }
    return { menschId: payload.sub as string, isAccepted: payload.result === 'accepted' };
};
```

### Notes

- Credentials (`client_id`, `client_secret`) go in the request body for the REST endpoints and in the form body for the token endpoint, as with OAuth 2.0 `client_secret_post`. Keep them on your server, never in browser or mobile code.
- If an administrator revokes your service, its credentials and any access tokens already issued stop working immediately.
- The flow resembles OpenID CIBA (poll/push mode), but it has its own endpoints and never issues access or refresh tokens to your service.

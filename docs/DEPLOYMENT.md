# UT4MasterServer — deployment guide

How the master server runs locally and in prod, how UT4 clients talk to
it, and the moving parts behind Quick Play + password login.

The sibling repo's [docs/DEPLOYMENT.md](https://github.com/ut4-hub/ut4-install/blob/main/docs/DEPLOYMENT.md)
covers the client and dedicated-server side. This document covers the
master-server side — containers, controllers, mongo, and the request flow
the UT4 client exercises.

## Architecture

```
        +-------------------------+
        |  UT4 client (Linux)     |
        |  XAN-3525360            |
        +-----------+-------------+
                    |
        Engine.ini overrides + DNS hijack
                    |
                    v
  +---------------------------------------+
  |  nginx (ut4-redirect, on the host)    |
  |  intercepts *.epicgames.com on :443   |
  +-----------------+---------------------+
                    | http (local)
                    v
  +---------------------------------------+
  |  ut4ms-smoke-api  (.NET 6, :5000)     |
  |  controllers below                    |
  +-----+--------+----------+-------------+
        |        |          |
        v        v          v
   +--------+ +------+ +---------+
   | mongo  | | xmpp | | mailpit |
   | :27017 | | :5222| | :8025   |
   +--------+ +------+ +---------+
                +-- ejabberd (XMPP + REST on :5280)
                +-- mailpit  (SMTP capture in dev)
   +--------+
   | web    | (nginx, :5001 — serves /news/, /assets, etc.)
   +--------+
```

## The smoke stack

All five components run as docker containers on the host:

```bash
cd /path/to/UT4MasterServer
docker compose -f docker-compose.yml -f docker-compose.override.yml up -d
```

| Container         | Image / role                    | Host port  |
|-------------------|---------------------------------|-----------:|
| `ut4ms-smoke-api` | .NET 6 master server            | `:5000`    |
| `ut4ms-smoke-web` | nginx — static assets, news     | `:5001`    |
| `ut4ms-smoke-xmpp`| ejabberd                        | `:5222`, `:5280` (REST) |
| `ut4ms-smoke-mongo`| MongoDB 7                      | `:27017`   |
| `ut4ms-smoke-mailpit`| Mailpit dev SMTP/web         | `:1025` (SMTP), `:8025` (web) |

## Controllers and the endpoints UT4 hits

The UT4 binary has Epic prod hostnames hardcoded; the
[`ut4-redirect`](https://github.com/itpick/nix-config/blob/main/modules/nixos/ut4-redirect/default.nix)
nginx module hijacks them and proxies to `127.0.0.1:5000`. Routes the
client uses:

| Route prefix                              | Controller                                    |
|-------------------------------------------|-----------------------------------------------|
| `/account/api/oauth/token`                | `AccountController` — password + refresh grants |
| `/account/api/public/account/{id}`        | `AccountController` — profile lookup          |
| `/account/api/forgot-password`            | `AccountController` — request password reset  |
| `/account/api/reset-password`             | `AccountController` — apply password reset    |
| `/ut/api/matchmaking/session`             | `MatchmakingController` — server reg + search |
| `/ut/api/cloudstorage/system/{filename}`  | `CloudStorageController` — JSON cloud files   |
| `/friends/api/public/friends/{id}`        | `FriendsController` — friends list (partial)  |

Bearer-token auth comes from `SessionService` against the `sessions` mongo
collection. Tokens are JWTs signed with the configured signing key
(`appsettings*.json`).

## Login without an "auth token"

The UT4 main-menu login dialog accepts **username or email + password**
directly — no copy-paste auth tokens. This flows as:

```
[client] POST /account/api/oauth/token  (Basic Auth: client id+secret)
         grant_type=password&username={user-or-email}&password={pass}
[server] AccountController.GetToken → AccountService.GetAccountAsync
         + password verify → SessionService.CreateSessionAsync (JWT)
[server] -> 200 {access_token, refresh_token, account_id, displayName, ...}
```

The `username` field accepts either the email address or the display name —
`AccountService` checks both columns.

If the user clicks **"Forgot password"**, the in-game dialog opens the
master server's `/forgot-password` page in an external browser (binary
string-patch on the client to swap the hardcoded `accounts.unrealengine.com`
URL — see ut4-install's note on hostname hijacking). The reset page POSTs
to `/account/api/forgot-password`, the server emails a reset link (Resend
in prod / mailpit locally) that returns the user to `/reset-password` with
a one-time token validated against the `password_reset_tokens` mongo
collection.

## Quick Play end-to-end

The flow:

```
[client] click Quick Play → Blitz
[client] POST /ut/api/matchmaking/session/matchMakingRequest
         body { "criteria": [{"keyName":"UT_PLAYLISTID_i","keyValue":51}], ... }
[server] MatchmakingService searches the gameservers collection for either
         UT_PLAYLISTID_i=51 OR UT_RANKED_i=1 (auto-promote)
[server] returns a server with reservation address
[client] POST <server>/reserve  (party beacon reservation)
[server-blitz] AUTPartyBeaconHost.ProcessReservationRequest
              (the binary patch in ut4-install lets the empty-mode server
              accept this; otherwise blocked by IsValid check)
[client] JoinSession → loads FR-MeltDown
```

### Auto-inject + auto-promote (master-side)

Dedicated UT4 servers PUT their attribute bag to mongo periodically. Some
attrs the in-game matchmaker filters on (`UT_RULETAG_s`, `UT_PLAYLISTID_i`,
etc.) aren't always set by the server. To keep ranked Blitz servers
QuickPlay-visible across the lifecycle:

- `UT4MasterServer.Models/Database/GameServer.cs` — `ToJson` auto-injects
  `UT_NEEDS_i`, `UT_TEAMELO_i`, `UT_TEAMELO2_i`, plus the QuickPlay-required
  attrs (`UT_RULETAG_s`, `UT_PLAYLISTID_i`, `PLAYLISTID_i`,
  `UT_SERVERTRUSTLEVEL_i`, `UT_GAMEINSTANCE_i`, `UT_MATCHSTATE_s`,
  `UT_SERVERNAME_s`) whenever `UT_RANKED_i=1`.
- `UT4MasterServer.Services/Scoped/MatchmakingService.cs` — when a search
  criterion is `UT_PLAYLISTID_i`, also match any `UT_RANKED_i=1` server via
  a BsonDocument `$or`. This is the "auto-promote" — any ranked server can
  serve any playlist, so the client's QuickPlay tile lands somewhere even
  if the server hasn't written the exact playlist id yet.
- `MatchmakingController.CreateGameServer` — rewrites docker-bridge IPs
  (`172.16.0.0/12` → `127.0.0.1`) so a containerized server still gets a
  reachable address when listed for local clients.

## Watchdog for ranked Blitz servers

`scripts/blitz-watchdog.sh` keeps a Blitz dedicated server alive across
match cycles. Why it's needed:

`AUTGameSessionRanked` transitions the world to `UTEmptyServerGameMode`
after each match. The binary patch in ut4-install lets the empty-mode
server still accept QuickPlay reservations, but the client lands on the
void `ut-entry` map. The watchdog tails the server log, detects the
`Server switch level: ut-entry?game=empty` line, kills the process, and
restarts it back into `FR-MeltDown` for the next QuickPlay cycle.

Configurable via env vars (defaults in the script):

```bash
INSTALL_DIR=/home/lucas/Games/UT4-server/LinuxServer \
PORT=7779 QUERY_PORT=7789 \
MAP=FR-MeltDown GAMEMODE=/Script/UnrealTournament.UTFlagRunGame \
BOT_FILL=10 MIN_PLAYERS=1 \
./scripts/blitz-watchdog.sh
```

## CloudStorage / MOTD / Announcements

`UT4MasterServer/CloudStorageSystemFiles/UnrealTournmentMCPAnnouncement.json`
seeds the in-game announcement panel.

**The body box is dead code** in this UT4 build:
`SUTWebBrowserPanel::Construct` never invokes `ConstructPanel`, so the
inner `SWebBrowser` (CEF) is never instantiated for inline panels — the
body just spins forever. Workaround: set `MinHeight: 0` in the JSON to
collapse the box; pack readable content into the `Title` field (it renders
as a multi-line `STextBlock`). ASCII only — the bundled font lacks bullets
/ em-dashes / arrows.

## Local dev recipes

### Bring up the smoke stack

```bash
docker compose -f docker-compose.yml -f docker-compose.override.yml up -d
# Wait for ut4ms-smoke-api to log "Now listening on: http://[::]:80"
curl -s http://127.0.0.1:5000/health  # 200 OK
```

### Seed an account

```bash
curl -s -XPOST http://127.0.0.1:5000/account/api/create/account \
  -H 'Content-Type: application/json' \
  -d '{"username":"demo","email":"demo@example.com","password":"demopass123"}'
```

### Trigger password reset (mailpit captures the email)

```bash
curl -XPOST http://127.0.0.1:5000/account/api/forgot-password \
  -d 'email=demo@example.com'
# Inspect at http://127.0.0.1:8025
```

### Tail the matchmaker

```bash
docker logs -f ut4ms-smoke-api 2>&1 | grep -E 'MM_REQUEST|MATCHFOUND'
```

## HTTP vs HTTPS

The smoke stack runs everything on plain HTTP. The UT4 client doesn't care
about TLS for the master server itself — the
[`ut4-redirect`](https://github.com/itpick/nix-config/blob/main/modules/nixos/ut4-redirect/default.nix)
nginx module on the host terminates TLS for the Epic hostnames the client
expects on `:443`, then proxies plaintext to `127.0.0.1:5000`.

Friends-list and similar features may need additional Epic hostname
hijacks. The wildcard `*.epicgames.com` cert does NOT cover
`friends-public-service-prod06.ol.epicgames.com` (one-label wildcards) —
extend the cert SAN or issue per-service certs as you add hostnames.

## See also

- [`scripts/blitz-watchdog.sh`](../scripts/blitz-watchdog.sh) — server lifecycle watchdog
- [`docker-compose.yml`](../docker-compose.yml) + override — smoke stack definition
- [`UT4MasterServer/Controllers/Epic/FriendsController.cs`](../UT4MasterServer/Controllers/Epic/FriendsController.cs) — partial friends impl (see open issues)
- [ut4-install/docs/DEPLOYMENT.md](https://github.com/ut4-hub/ut4-install/blob/main/docs/DEPLOYMENT.md) — client + dedicated-server side

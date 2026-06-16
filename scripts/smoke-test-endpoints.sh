#!/usr/bin/env bash
# Smoke-test the master server's UT4-facing endpoints.
#
# Picks the freshest live session from mongo and curls each endpoint,
# reporting PASS / FAIL with the HTTP code + a short slice of the body.
#
# Usage:
#   ./scripts/smoke-test-endpoints.sh
#
# Requires:
#   - docker compose stack up (ut4ms-smoke-api, ut4ms-smoke-mongo on 5000/27017)
#   - mongo creds smoke/smokepass (the smoke compose defaults)

set -uo pipefail

API=http://127.0.0.1:5000
MONGO_URI=${MONGO_URI:-mongodb://smoke:smokepass@127.0.0.1:27017/?authSource=admin}

# Grab freshest live session (token + accountID).
session_json=$(docker exec ut4ms-smoke-mongo mongosh "$MONGO_URI" --quiet --eval '
db = db.getSiblingDB("ut4master");
const s = db.sessions.find({"AccessToken.Expiration":{$gt:new Date()}})
  .sort({"AccessToken.Expiration":-1}).limit(1).toArray()[0];
print(JSON.stringify({token:s?.AccessToken?.Value, acct:s?.AccountID}));' 2>/dev/null | tail -1)

token=$(echo "$session_json" | jq -r .token 2>/dev/null || \
        echo "$session_json" | python3 -c 'import json,sys;print(json.load(sys.stdin)["token"])')
acct=$(echo "$session_json"  | jq -r .acct  2>/dev/null || \
        echo "$session_json" | python3 -c 'import json,sys;print(json.load(sys.stdin)["acct"])')

if [[ -z $token || $token == "null" ]]; then
  echo "FATAL: no live session in mongo. Log in via the game or the web UI first." >&2
  exit 2
fi

echo "Using session for account $acct"
echo "Token: ${token:0:16}..."
echo

fail=0
check() {
  local desc=$1 method=$2 path=$3 expected=$4 body_grep=${5:-}
  local code body
  if [[ $method == GET ]]; then
    body=$(curl -s -o /tmp/smoke-body.out -w "%{http_code}" "$API$path" -H "Authorization: bearer $token")
  elif [[ $method == DELETE ]]; then
    body=$(curl -s -o /tmp/smoke-body.out -w "%{http_code}" -X DELETE "$API$path" -H "Authorization: bearer $token")
  elif [[ $method == POST ]]; then
    body=$(curl -s -o /tmp/smoke-body.out -w "%{http_code}" -X POST "$API$path" -H "Authorization: bearer $token" -H "Content-Type: application/json" -d '{}')
  fi
  if [[ $body == "$expected" ]]; then
    if [[ -n $body_grep ]] && ! grep -q "$body_grep" /tmp/smoke-body.out; then
      printf "  FAIL  %-45s HTTP %s (body missing %q)\n" "$desc" "$body" "$body_grep" >&2
      fail=$((fail+1))
    else
      printf "  PASS  %-45s HTTP %s\n" "$desc" "$body"
    fi
  else
    printf "  FAIL  %-45s HTTP %s (expected %s)\n" "$desc" "$body" "$expected" >&2
    fail=$((fail+1))
  fi
}

echo "=== Friends / Blocklist / Recent ==="
check "GET friends"           GET    "/friends/api/public/friends/$acct?includePending=true" 200 "\["
check "GET blocklist"         GET    "/friends/api/public/blocklist/$acct"                  200 "blockedUsers"
check "GET recentPlayers"     GET    "/friends/api/public/list/ut/$acct/recentPlayers"      200 "recentplayers"
echo

echo "=== Cloud storage ==="
check "GET system files list" GET    "/ut/api/cloudstorage/system"                          200 ""
check "GET user_profile_2"    GET    "/ut/api/cloudstorage/user/$acct/user_profile_2"       200 ""
check "GET user_progression"  GET    "/ut/api/cloudstorage/user/$acct/user_progression_1"   200 ""
check "GET stats.json (auto)" GET    "/ut/api/cloudstorage/user/$acct/stats.json"           200 "PlayerName"
check "GET oldplayercard stub" GET   "/ut/api/cloudstorage/user/$acct/oldplayercard"         200 ""
check "GET system announcement" GET  "/ut/api/cloudstorage/system/UnrealTournmentMCPAnnouncement.json" 200 "Title"
echo

echo "=== Profile / matchmaking ==="
check "POST QueryProfile"     POST   "/ut/api/game/v2/profile/$acct/client/QueryProfile?profileId=profile0&rvn=-1" 200 "profileRevision"
check "POST mm sessionSearch" POST   "/ut/api/matchmaking/session/matchMakingRequest"       200 ""
echo

if [[ $fail -gt 0 ]]; then
  echo
  echo "$fail check(s) failed" >&2
  exit 1
fi

echo
echo "All endpoints OK"

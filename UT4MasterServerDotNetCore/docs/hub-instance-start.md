## Hub Instance Creation Flow

These are the calls specific to launching a hub instance.

# Auth

Identical to hub launch auth

# Start Instance Session


```
POST https://ut-public-service-prod10.ol.epicgames.com/ut/api/matchmaking/session HTTP/1.1
Host: ut-public-service-prod10.ol.epicgames.com
Accept: */*
Content-Type: application/json
User-Agent: game=UnrealTournament, engine=UE4, build=++UT+Release-Next-CL-3525360
X-Epic-Correlation-ID: UE4-6277930D4EF798AB6ECC2A9DF8B26049
Authorization: bearer de936fc420a94742a42cfbd06e7945d1
Content-Length: 1469
Pragma: no-cache

{
	"ownerId": "D1E2CEDA40050AAF3E205185E1697234",
	"ownerName": "[DS]DESKTOP-IQJVTED-15520",
	"serverName": "[DS]DESKTOP-IQJVTED-15520",
	"maxPublicPlayers": 10000,
	"maxPrivatePlayers": 0,
	"shouldAdvertise": true,
	"allowJoinInProgress": true,
	"isDedicated": true,
	"usesStats": false,
	"allowInvites": true,
	"usesPresence": false,
	"allowJoinViaPresence": true,
	"allowJoinViaPresenceFriendsOnly": true,
	"buildUniqueId": "256652735",
	"attributes":
	{
		"BEACONPORT_i": 7788,
		"GAMEMODE_s": "/Script/UnrealTournament.UTCTFGameMode",
		"MAPNAME_s": "CTF-TitanPass",
		"GAMENAME_s": "Capture the Flag",
		"UT_MATCHELO_i": 0,
		"UT_SERVERNAME_s": "Capture the Flag on CTF-TitanPass",
		"UT_SERVERMOTD_s": "This is the ServerMOTD string from Game.ini",
		"UT_MATCHDURATION_i": 0,
		"UT_HUBGUID_s": "516E6C8C4595684421A2A3AB7BA704F6",
		"UT_REDTEAMSIZE_i": 0,
		"UT_BLUETEAMSIZE_i": 0,
		"UT_TRAININGGROUND_b": false,
		"UT_MINELO_i": 0,
		"UT_MAXELO_i": 0,
		"UT_PLAYERONLINE_i": 0,
		"UT_SPECTATORSONLINE_i": 0,
		"UT_SERVERVERSION_s": "3525360",
		"UT_SERVERINSTANCEGUID_s": "9E61ACB74C95EFDEDAF11080C26D211E",
		"UT_GAMEINSTANCE_i": 1,
		"UT_SERVERFLAGS_i": 0,
		"UT_NUMMATCHES_i": 1,
		"UT_MAXPLAYERS_i": 10,
		"UT_MAXSPECTATORS_i": 7,
		"UT_MATCHSTATE_s": "EnteringMap"
	},
	"serverPort": 8000,
	"openPrivatePlayers": 0,
	"openPublicPlayers": 10000,
	"sortWeight": 0,
	"publicPlayers": [],
	"privatePlayers": []
}
```

Very similar to hub session announcement, but with added `attributes` fields:

```
GAMENAME_s: string // "Capture the Flag" aka gametype name
UT_MATCHELO_i: int // probably min ELO required to join
```

## Response

```
{
    "id": "29013d8988ca45fdb632546f93990426",
    "ownerId": "D1E2CEDA40050AAF3E205185E1697234",
    "ownerName": "[DS]DESKTOP-IQJVTED-15520",
    "serverName": "[DS]DESKTOP-IQJVTED-15520",
    "serverAddress": "108.172.186.109",
    "serverPort": 8000,
    "maxPublicPlayers": 10000,
    "openPublicPlayers": 10000,
    "maxPrivatePlayers": 0,
    "openPrivatePlayers": 0,
    "attributes": {
        "UT_REDTEAMSIZE_i": 0,
        "BEACONPORT_i": 7788,
        "UT_PLAYERONLINE_i": 0,
        "UT_MATCHSTATE_s": "EnteringMap",
        "UT_SERVERINSTANCEGUID_s": "9E61ACB74C95EFDEDAF11080C26D211E",
        "UT_SPECTATORSONLINE_i": 0,
        "UT_MAXPLAYERS_i": 10,
        "UT_MATCHELO_i": 0,
        "UT_SERVERNAME_s": "Capture the Flag on CTF-TitanPass",
        "GAMENAME_s": "Capture the Flag",
        "UT_NUMMATCHES_i": 1,
        "UT_GAMEINSTANCE_i": 1,
        "UT_MAXSPECTATORS_i": 7,
        "UT_SERVERVERSION_s": "3525360",
        "GAMEMODE_s": "/Script/UnrealTournament.UTCTFGameMode",
        "UT_HUBGUID_s": "516E6C8C4595684421A2A3AB7BA704F6",
        "UT_BLUETEAMSIZE_i": 0,
        "UT_SERVERTRUSTLEVEL_i": 2,
        "UT_TRAININGGROUND_b": false,
        "UT_MINELO_i": 0,
        "UT_MAXELO_i": 0,
        "UT_SERVERMOTD_s": "This is the ServerMOTD string from Game.ini",
        "MAPNAME_s": "CTF-TitanPass",
        "UT_MATCHDURATION_i": 0,
        "UT_SERVERFLAGS_i": 0
    },
    "publicPlayers": [],
    "privatePlayers": [],
    "totalPlayers": 0,
    "allowJoinInProgress": true,
    "shouldAdvertise": true,
    "isDedicated": true,
    "usesStats": false,
    "allowInvites": true,
    "usesPresence": false,
    "allowJoinViaPresence": true,
    "allowJoinViaPresenceFriendsOnly": true,
    "buildUniqueId": "256652735",
    "lastUpdated": "2022-12-21T03:48:20.258Z",
    "started": false
}
```

No different in format than hub launch response.
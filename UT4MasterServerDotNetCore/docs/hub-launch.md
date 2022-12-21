
## Hub Startup Flow

These are the calls specific to launching a hub. There are other calls made to the
cloudstorage endpoints, but they are the same calls made by clients.

# Auth

```
POST https://account-public-service-prod03.ol.epicgames.com/account/api/oauth/token HTTP/1.1
Host: account-public-service-prod03.ol.epicgames.com
Accept: */*
Content-Type: application/x-www-form-urlencoded
User-Agent: game=UnrealTournament, engine=UE4, build=++UT+Release-Next-CL-3525360
Authorization: basic NmZmNDNlNzQzZWRjNGQxZGJhYzM1OTQ4NzdiNGJlZDk6NTQ2MTlkNmY4NGQ0NDNlMTk1MjAwYjU0YWI2NDlhNTM=
Content-Length: 29
Pragma: no-cache

grant_type=client_credentials
```

All hub and instance auth requests use the same basic auth data, where the username is 
`6ff43e743edc4d1dbac3594877b4bed9` and the password is `54619d6f84d443e195200b54ab649a53`


# Start Hub Session

```
POST https://ut-public-service-prod10.ol.epicgames.com/ut/api/matchmaking/session HTTP/1.1
Host: ut-public-service-prod10.ol.epicgames.com
Accept: */*
Content-Type: application/json
User-Agent: game=UnrealTournament, engine=UE4, build=++UT+Release-Next-CL-3525360
X-Epic-Correlation-ID: UE4-C88EEEF44FD10436955A35BD21C075DE
Authorization: bearer 47c88ffa188c46afa3807c1aefdcfcc8
Content-Length: 1417
Pragma: no-cache

{
	"ownerId": "02494C20477617250BB7ED984530AB99",
	"ownerName": "[DS]DESKTOP-IQJVTED-27320",
	"serverName": "[DS]DESKTOP-IQJVTED-27320",
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
		"BEACONPORT_i": 7787,
		"GAMEMODE_s": "/Script/UnrealTournament.UTLobbyGameMode",
		"MAPNAME_s": "UT-Entry",
		"UT_SERVERNAME_s": "This is the ServerName string from Game.ini",
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
		"UT_SERVERINSTANCEGUID_s": "516E6C8C4595684421A2A3AB7BA704F6",
		"UT_GAMEINSTANCE_i": 0,
		"UT_SERVERFLAGS_i": 0,
		"UT_NUMMATCHES_i": 0,
		"UT_MAXPLAYERS_i": 200,
		"UT_MAXSPECTATORS_i": 7,
		"UT_MATCHSTATE_s": "EnteringMap"
	},
	"serverPort": 7777,
	"openPrivatePlayers": 0,
	"openPublicPlayers": 10000,
	"sortWeight": 0,
	"publicPlayers": [],
	"privatePlayers": []
}
```

This POST request announces a new instance. `ownerName` and `serverName` are always `[DS]`
followed by the system device name (`DESKTOP-IQJVTED`) followed by some numbers.

`UT_HUBGUID_s` and `UT_SERVERINSTANCEGUID_s` are found in the server's Game.ini:

```
[/Script/UnrealTournament.UTBaseGameMode]
ServerInstanceID=516E6C8C4595684421A2A3AB7BA704F6
```

## Response

```
{
  "id": "4366368a402a4dd1adf3a9fd9fa15a36",
  "ownerId": "02494C20477617250BB7ED984530AB99",
  "ownerName": "[DS]DESKTOP-IQJVTED-27320",
  "serverName": "[DS]DESKTOP-IQJVTED-27320",
  "serverAddress": "108.172.186.109",
  "serverPort": 7777,
  "maxPublicPlayers": 10000,
  "openPublicPlayers": 10000,
  "maxPrivatePlayers": 0,
  "openPrivatePlayers": 0,
  "attributes": {
    "UT_SERVERNAME_s": "This is the ServerName string from Game.ini",
    "UT_REDTEAMSIZE_i": 0,
    "UT_NUMMATCHES_i": 0,
    "UT_GAMEINSTANCE_i": 0,
    "UT_MAXSPECTATORS_i": 7,
    "BEACONPORT_i": 7787,
    "UT_PLAYERONLINE_i": 0,
    "UT_SERVERVERSION_s": "3525360",
    "GAMEMODE_s": "/Script/UnrealTournament.UTLobbyGameMode",
    "UT_HUBGUID_s": "516E6C8C4595684421A2A3AB7BA704F6",
    "UT_BLUETEAMSIZE_i": 0,
    "UT_MATCHSTATE_s": "EnteringMap",
    "UT_SERVERTRUSTLEVEL_i": 2,
    "UT_SERVERINSTANCEGUID_s": "516E6C8C4595684421A2A3AB7BA704F6",
    "UT_TRAININGGROUND_b": false,
    "UT_MINELO_i": 0,
    "UT_MAXELO_i": 0,
    "UT_SPECTATORSONLINE_i": 0,
    "UT_MAXPLAYERS_i": 200,
    "UT_SERVERMOTD_s": "This is the ServerMOTD string from Game.ini",
    "MAPNAME_s": "UT-Entry",
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
  "lastUpdated": "2022-12-21T03:46:12.280Z",
  "started": false
}
```

Responds with a 200 including all passed in data plus some additional fields:
```
id: string            // a guid
lastUpdated:  string  // an iso8601 datetime
serverAddress: string // an ipv4 address
started: false        // whether or not the hub has started
totalPlayers: 0
```

# Update Hub Session

```
PUT https://ut-public-service-prod10.ol.epicgames.com/ut/api/matchmaking/session/4366368a402a4dd1adf3a9fd9fa15a36 HTTP/1.1
Host: ut-public-service-prod10.ol.epicgames.com
Accept: */*
Cookie: AWSELB=E9958FDB1448870EBFBE28377F3D10376F43CDC4B19CDE0B558DCA7837C13530AF8AEE356833F1E77089BA61742B66DC413B1809DF694FA70E14833E719C9EECB1D2256DBC; AWSELBCORS=E9958FDB1448870EBFBE28377F3D10376F43CDC4B19CDE0B558DCA7837C13530AF8AEE356833F1E77089BA61742B66DC413B1809DF694FA70E14833E719C9EECB1D2256DBC
Content-Type: application/json
User-Agent: game=UnrealTournament, engine=UE4, build=++UT+Release-Next-CL-3525360
X-Epic-Correlation-ID: UE4-56E0785C4AC160A710AB45B06CB4113B
Authorization: bearer 47c88ffa188c46afa3807c1aefdcfcc8
Content-Length: 1526
Pragma: no-cache

{
	"ownerId": "02494C20477617250BB7ED984530AB99",
	"ownerName": "[DS]DESKTOP-IQJVTED-27320",
	"serverName": "[DS]DESKTOP-IQJVTED-27320",
	"maxPublicPlayers": 10000,
	"maxPrivatePlayers": 0,
	"shouldAdvertise": true,
	"allowJoinInProgress": true,
	"isDedicated": true,
	"usesStats": false,
	"allowInvites": true,
	"usesPresence": false,
	"allowJoinViaPresence": true,
	"allowJoinViaPresenceFriendsOnly": false,
	"buildUniqueId": "256652735",
	"attributes":
	{
		"BEACONPORT_i": 7787,
		"GAMEMODE_s": "/Script/UnrealTournament.UTLobbyGameMode",
		"MAPNAME_s": "UT-Entry",
		"UT_SERVERNAME_s": "This is the ServerName string from Game.ini",
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
		"UT_SERVERINSTANCEGUID_s": "516E6C8C4595684421A2A3AB7BA704F6",
		"UT_GAMEINSTANCE_i": 0,
		"UT_SERVERFLAGS_i": 0,
		"UT_NUMMATCHES_i": 0,
		"UT_MAXPLAYERS_i": 200,
		"UT_MAXSPECTATORS_i": 7,
		"UT_MATCHSTATE_s": "EnteringMap",
		"UT_SERVERTRUSTLEVEL_i": 2
	},
	"id": "4366368a402a4dd1adf3a9fd9fa15a36",
	"serverAddress": "1823259245",
	"serverPort": 7777,
	"openPrivatePlayers": 0,
	"openPublicPlayers": 10000,
	"sortWeight": 0,
	"publicPlayers": [],
	"privatePlayers": []
}
```

Same deal as the initial announce, just with a PUT to update. For some reason sends a 
strange `serverAddress` field. Response is in the same format as the initial POST.

# Start Hub Session

```
POST https://ut-public-service-prod10.ol.epicgames.com/ut/api/matchmaking/session/4366368a402a4dd1adf3a9fd9fa15a36/start HTTP/1.1
Host: ut-public-service-prod10.ol.epicgames.com
Accept: */*
Cookie: AWSELB=E9958FDB1448870EBFBE28377F3D10376F43CDC4B19CDE0B558DCA7837C13530AF8AEE356833F1E77089BA61742B66DC413B1809DF694FA70E14833E719C9EECB1D2256DBC; AWSELBCORS=E9958FDB1448870EBFBE28377F3D10376F43CDC4B19CDE0B558DCA7837C13530AF8AEE356833F1E77089BA61742B66DC413B1809DF694FA70E14833E719C9EECB1D2256DBC
Content-Type: application/json
User-Agent: game=UnrealTournament, engine=UE4, build=++UT+Release-Next-CL-3525360
X-Epic-Correlation-ID: UE4-A93DEE5342F1F3D3E4A74CB8762F645C
Authorization: bearer 47c88ffa188c46afa3807c1aefdcfcc8
Content-Length: 0
Pragma: no-cache
```

Sends no data, receives none back. Just a 204 response. Probably indicating that the hub is ready.

# Update Hub Session

```
PUT https://ut-public-service-prod10.ol.epicgames.com/ut/api/matchmaking/session/4366368a402a4dd1adf3a9fd9fa15a36 HTTP/1.1
Host: ut-public-service-prod10.ol.epicgames.com
Accept: */*
Cookie: AWSELB=E9958FDB1448870EBFBE28377F3D10376F43CDC4B19CDE0B558DCA7837C13530AF8AEE356833F1E77089BA61742B66DC413B1809DF694FA70E14833E719C9EECB1D2256DBC; AWSELBCORS=E9958FDB1448870EBFBE28377F3D10376F43CDC4B19CDE0B558DCA7837C13530AF8AEE356833F1E77089BA61742B66DC413B1809DF694FA70E14833E719C9EECB1D2256DBC
Content-Type: application/json
User-Agent: game=UnrealTournament, engine=UE4, build=++UT+Release-Next-CL-3525360
X-Epic-Correlation-ID: UE4-18A71DF0426B405BEA1389BACEF5E72F
Authorization: bearer 47c88ffa188c46afa3807c1aefdcfcc8
Content-Length: 1531
Pragma: no-cache

{
	"ownerId": "02494C20477617250BB7ED984530AB99",
	"ownerName": "[DS]DESKTOP-IQJVTED-27320",
	"serverName": "[DS]DESKTOP-IQJVTED-27320",
	"maxPublicPlayers": 10000,
	"maxPrivatePlayers": 0,
	"shouldAdvertise": true,
	"allowJoinInProgress": true,
	"isDedicated": true,
	"usesStats": false,
	"allowInvites": true,
	"usesPresence": false,
	"allowJoinViaPresence": true,
	"allowJoinViaPresenceFriendsOnly": false,
	"buildUniqueId": "256652735",
	"attributes":
	{
		"BEACONPORT_i": 7787,
		"GAMEMODE_s": "/Script/UnrealTournament.UTLobbyGameMode",
		"MAPNAME_s": "UT-Entry",
		"UT_SERVERNAME_s": "This is the ServerName string from Game.ini ",
		"UT_SERVERMOTD_s": "This is the ServerMOTD string from Game.ini ",
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
		"UT_SERVERINSTANCEGUID_s": "516E6C8C4595684421A2A3AB7BA704F6",
		"UT_GAMEINSTANCE_i": 0,
		"UT_SERVERFLAGS_i": 0,
		"UT_NUMMATCHES_i": 0,
		"UT_MAXPLAYERS_i": 200,
		"UT_MAXSPECTATORS_i": 7,
		"UT_MATCHSTATE_s": "WaitingToStart",
		"UT_SERVERTRUSTLEVEL_i": 2
	},
	"id": "4366368a402a4dd1adf3a9fd9fa15a36",
	"serverAddress": "1823259245",
	"serverPort": 7777,
	"openPrivatePlayers": 0,
	"openPublicPlayers": 10000,
	"sortWeight": 0,
	"publicPlayers": [],
	"privatePlayers": []
}
```

Same as the initial PUT, but with a new `UT_MATCHSTATE_s`. Also has added spaces to the end of 
`UT_SERVERNAME_s` and `UT_SERVERMOTD_s`

# Heartbeat

```
POST https://ut-public-service-prod10.ol.epicgames.com/ut/api/matchmaking/session/4366368a402a4dd1adf3a9fd9fa15a36/heartbeat HTTP/1.1
Host: ut-public-service-prod10.ol.epicgames.com
Accept: */*
Cookie: AWSELB=E9958FDB1448870EBFBE28377F3D10376F43CDC4B19CDE0B558DCA7837C13530AF8AEE356833F1E77089BA61742B66DC413B1809DF694FA70E14833E719C9EECB1D2256DBC; AWSELBCORS=E9958FDB1448870EBFBE28377F3D10376F43CDC4B19CDE0B558DCA7837C13530AF8AEE356833F1E77089BA61742B66DC413B1809DF694FA70E14833E719C9EECB1D2256DBC
Content-Type: application/json
User-Agent: game=UnrealTournament, engine=UE4, build=++UT+Release-Next-CL-3525360
X-Epic-Correlation-ID: UE4-1D75DB9D4D6F0E2C0A2DE88E11BC34D0
Authorization: bearer 47c88ffa188c46afa3807c1aefdcfcc8
Content-Length: 0
Pragma: no-cache
```

This is sent every 30 seconds. No data in or out, just a 204. Master server probably delists hubs a
short time after their last heartbeat.
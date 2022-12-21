## Client Joining Hub Flow

These are the calls specific to a client creating a hub instance.

# Matchmaking Request

```
POST https://ut-public-service-prod10.ol.epicgames.com/ut/api/matchmaking/session/matchMakingRequest HTTP/1.1
Host: ut-public-service-prod10.ol.epicgames.com
Accept: */*
Cookie: AWSELB=E9958FDB1448870EBFBE28377F3D10376F43CDC4B14B7412096B9B03EBCEE56C4051F02C0839B49802B60CF8413BEE9CF0EE52960FC45D37FCCE1689E8CE7AEE49558F7C5C; AWSELBCORS=E9958FDB1448870EBFBE28377F3D10376F43CDC4B14B7412096B9B03EBCEE56C4051F02C0839B49802B60CF8413BEE9CF0EE52960FC45D37FCCE1689E8CE7AEE49558F7C5C
Content-Type: application/json
User-Agent: game=UnrealTournament, engine=UE4, build=++UT+Release-Next-CL-3525360
X-Epic-Correlation-ID: UE4-31957B9A4DCE2C7F894131BC869FFBC9
Authorization: bearer 8999c1a8446549e38e621e2e6f7603ec
Content-Length: 280
Pragma: no-cache

{
	"criteria": [
		{
			"type": "EQUAL",
			"key": "UT_SERVERVERSION_s",
			"value": "3525360"
		},
		{
			"type": "EQUAL",
			"key": "UT_SERVERINSTANCEGUID_s",
			"value": "7C5F20C74F6C0DA060BDB3B6A7868EC7"
		}
	],
	"buildUniqueId": "256652735",
	"maxResults": 1
}
```

The request criteria is static, except for the key for `UT_SERVERINSTANCEGUID_s`.

## Response

```
[
    {
        "id": "e67b11a43927473ba1e89ca1e01a7140",
        "ownerId": "71B106B04C0E9AA05CD0D090D3996FB8",
        "ownerName": "[DS]DESKTOP-IQJVTED-25164",
        "serverName": "[DS]DESKTOP-IQJVTED-25164",
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
            "UT_MATCHSTATE_s": "WaitingToStart",
            "UT_SERVERINSTANCEGUID_s": "7C5F20C74F6C0DA060BDB3B6A7868EC7",
            "UT_SPECTATORSONLINE_i": 0,
            "UT_MAXPLAYERS_i": 10,
            "UT_MATCHELO_i": 0,
            "UT_SERVERNAME_s": "Capture the Flag on CTF-TitanPass ",
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
            "UT_SERVERMOTD_s": "This is the ServerMOTD string from Game.ini ",
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
        "allowJoinViaPresenceFriendsOnly": false,
        "buildUniqueId": "256652735",
        "lastUpdated": "2022-12-21T04:51:46.633Z",
        "started": true
    }
]
```

This is the same format as is typical for a hub or instance launch. Note that the key for
`UT_SERVERINSTANCEGUID_s` is what was sent in the request body. The `id` field is what is
used for a client to make a request to join an instance.

# Joining a Hub Instance

```
POST https://ut-public-service-prod10.ol.epicgames.com/ut/api/matchmaking/session/e67b11a43927473ba1e89ca1e01a7140/join?accountId=fd83abe496ca401497b5adf4e412bf2c HTTP/1.1
Host: ut-public-service-prod10.ol.epicgames.com
Accept: */*
Cookie: AWSELB=E9958FDB1448870EBFBE28377F3D10376F43CDC4B14B7412096B9B03EBCEE56C4051F02C0839B49802B60CF8413BEE9CF0EE52960FC45D37FCCE1689E8CE7AEE49558F7C5C; AWSELBCORS=E9958FDB1448870EBFBE28377F3D10376F43CDC4B14B7412096B9B03EBCEE56C4051F02C0839B49802B60CF8413BEE9CF0EE52960FC45D37FCCE1689E8CE7AEE49558F7C5C
Content-Type: application/json
User-Agent: game=UnrealTournament, engine=UE4, build=++UT+Release-Next-CL-3525360
X-Epic-Correlation-ID: UE4-D7FEC91146B93EDD251D66B733EC181C
Authorization: bearer 8999c1a8446549e38e621e2e6f7603ec
Content-Length: 0
Pragma: no-cache
```

This is the same request as made to join the hub itself. Just noting that the id used here is the id
returned in the previous request.
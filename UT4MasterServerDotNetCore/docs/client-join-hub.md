## Client Joining Hub Flow

These are the calls specific to joining a hub lobby.

# Query Profile

```
POST https://ut-public-service-prod10.ol.epicgames.com/ut/api/game/v2/profile/fd83abe496ca401497b5adf4e412bf2c/client/QueryProfile?profileId=profile0&rvn=3911 HTTP/1.1
Host: ut-public-service-prod10.ol.epicgames.com
Cookie: AWSELB=E9958FDB1448870EBFBE28377F3D10376F43CDC4B14B7412096B9B03EBCEE56C4051F02C0839B49802B60CF8413BEE9CF0EE52960FC45D37FCCE1689E8CE7AEE49558F7C5C; AWSELBCORS=E9958FDB1448870EBFBE28377F3D10376F43CDC4B14B7412096B9B03EBCEE56C4051F02C0839B49802B60CF8413BEE9CF0EE52960FC45D37FCCE1689E8CE7AEE49558F7C5C
Accept: */*
Content-Type: application/json
User-Agent: game=UnrealTournament, engine=UE4, build=++UT+Release-Next-CL-3525360
X-Epic-Correlation-ID: UE4-004C6347484951CB836BC2842F236F7B
Authorization: bearer 8999c1a8446549e38e621e2e6f7603ec
Content-Length: 4
Pragma: no-cache

{
}
```

Similar to login flow `QueryProfile`, but uses `rvn=3911` instead of `-1` and a different response
comes back.

## Response

```
{
    "profileRevision": 3911,
    "profileId": "profile0",
    "profileChangesBaseRevision": 3911,
    "profileChanges": [],
    "profileCommandRevision": 3910,
    "serverTime": "2022-12-21T04:39:41.662Z",
    "responseVersion": 1,
    "command": "QueryProfile"
}
```

# Get Hub Session Data

```
GET https://ut-public-service-prod10.ol.epicgames.com/ut/api/matchmaking/session/31a9890865de46b5af8b58105340cdc3 HTTP/1.1
Host: ut-public-service-prod10.ol.epicgames.com
Accept: */*
Cookie: AWSELB=E9958FDB1448870EBFBE28377F3D10376F43CDC4B14B7412096B9B03EBCEE56C4051F02C0839B49802B60CF8413BEE9CF0EE52960FC45D37FCCE1689E8CE7AEE49558F7C5C; AWSELBCORS=E9958FDB1448870EBFBE28377F3D10376F43CDC4B14B7412096B9B03EBCEE56C4051F02C0839B49802B60CF8413BEE9CF0EE52960FC45D37FCCE1689E8CE7AEE49558F7C5C
Content-Type: application/json
User-Agent: game=UnrealTournament, engine=UE4, build=++UT+Release-Next-CL-3525360
X-Epic-Correlation-ID: UE4-27E1898E4BCF1CB7570FF0A6EC3A2BDF
Authorization: bearer 8999c1a8446549e38e621e2e6f7603ec
Content-Length: 0
Pragma: no-cache
```

This gets all of the session data for the hub, in the same format as the responses to the PUT and
POST requests made by the hub.

# Join Hub

```
POST https://ut-public-service-prod10.ol.epicgames.com/ut/api/matchmaking/session/31a9890865de46b5af8b58105340cdc3/join?accountId=fd83abe496ca401497b5adf4e412bf2c HTTP/1.1
Host: ut-public-service-prod10.ol.epicgames.com
Accept: */*
Cookie: AWSELB=E9958FDB1448870EBFBE28377F3D10376F43CDC4B14B7412096B9B03EBCEE56C4051F02C0839B49802B60CF8413BEE9CF0EE52960FC45D37FCCE1689E8CE7AEE49558F7C5C; AWSELBCORS=E9958FDB1448870EBFBE28377F3D10376F43CDC4B14B7412096B9B03EBCEE56C4051F02C0839B49802B60CF8413BEE9CF0EE52960FC45D37FCCE1689E8CE7AEE49558F7C5C
Content-Type: application/json
User-Agent: game=UnrealTournament, engine=UE4, build=++UT+Release-Next-CL-3525360
X-Epic-Correlation-ID: UE4-3D8AB9C949DC5BF98C398FA62038BCD8
Authorization: bearer 8999c1a8446549e38e621e2e6f7603ec
Content-Length: 0
Pragma: no-cache

```

Server is likely checking that the `accountId` is valid and that responding appropriately. Will get
a 204 back on success.
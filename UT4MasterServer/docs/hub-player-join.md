# Player Join Hub Flow

These are the calls that are made specific to a player joining a hub.

## Join

```
POST https://ut-public-service-prod10.ol.epicgames.com/ut/api/game/v2/profile/fd83abe496ca401497b5adf4e412bf2c/dedicated_server/QueryProfile?profileId=profile0&rvn=-1 HTTP/1.1
Host: ut-public-service-prod10.ol.epicgames.com
Cookie: AWSELB=E9958FDB1448870EBFBE28377F3D10376F43CDC4B19CDE0B558DCA7837C13530AF8AEE356833F1E77089BA61742B66DC413B1809DF694FA70E14833E719C9EECB1D2256DBC; AWSELBCORS=E9958FDB1448870EBFBE28377F3D10376F43CDC4B19CDE0B558DCA7837C13530AF8AEE356833F1E77089BA61742B66DC413B1809DF694FA70E14833E719C9EECB1D2256DBC
Accept: */*
Content-Type: application/json
User-Agent: game=UnrealTournament, engine=UE4, build=++UT+Release-Next-CL-3525360
X-Epic-Correlation-ID: UE4-1BC8A26F436E4244ACDCE7ADA2138B61
Authorization: bearer 47c88ffa188c46afa3807c1aefdcfcc8
Content-Length: 4
Pragma: no-cache

{
}
```

POSTs a request to get the profile of the joined player.

### Server Response

```
{
  "profileRevision": 3911,
  "profileId": "profile0",
  "profileChangesBaseRevision": 3911,
  "profileChanges": [
	{
	  "changeType": "fullProfileUpdate",
	  "profile": {
		"_id": "dd8ba6fe24944718938b6b23a071ffa5",
		"created": "2017-01-29T03:21:48.822Z",
		"updated": "2022-12-15T00:25:57.941Z",
		"rvn": 3911,
		"wipeNumber": 4,
		"accountId": "fd83abe496ca401497b5adf4e412bf2c",
		"profileId": "profile0",
		"version": "ut_base",
		"items": {
		  "c3424820-7c39-44f6-a2f4-897d91f4eb7b": {
			"templateId": "Item.ThundercrashMale03",
			"attributes": {
			  "tradable": false
			},
			"quantity": 1
		  },
		  "6f686cc3-7a82-4764-a9e3-c5a9e49eb260": {
			"templateId": "Item.ThundercrashMale02",
			"attributes": {
			  "tradable": false
			},
			"quantity": 1
		  },
		  "b18ae8de-fc6b-4112-a593-6998333d73b3": {
			"templateId": "Item.Sunglasses",
			"attributes": {
			  "tradable": true
			},
			"quantity": 1
		  },
		  "a9079379-6c72-4c39-837e-bf93badf3ad6": {
			"templateId": "Item.HockeyMask",
			"attributes": {
			  "tradable": true
			},
			"quantity": 1
		  },
		  "e4712c64-bc7f-479f-9946-bf024969c626": {
			"templateId": "Item.HockeyMask02",
			"attributes": {
			  "tradable": true
			},
			"quantity": 1
		  },
		  "e6f6f3ad-b86d-4d25-8d48-79c85d586db8": {
			"templateId": "Item.ThundercrashBeanieGreen",
			"attributes": {
			  "tradable": true
			},
			"quantity": 1
		  },
		  "f4e8d9ab-8d71-4779-b70a-927d642f145e": {
			"templateId": "Item.NecrisMale04",
			"attributes": {
			  "tradable": false
			},
			"quantity": 1
		  },
		  "ff756c89-200d-4b12-b245-c420255d32a5": {
			"templateId": "Item.ThundercrashBeanieRed",
			"attributes": {
			  "tradable": true
			},
			"quantity": 1
		  },
		  "03adb035-6a79-417d-9768-344ea25b1ac2": {
			"templateId": "Item.BeanieWhite",
			"attributes": {
			  "tradable": true
			},
			"quantity": 1
		  },
		  "b92045b3-4361-45fc-a240-f1d74174677f": {
			"templateId": "Item.ThundercrashMale05",
			"attributes": {
			  "tradable": false
			},
			"quantity": 1
		  },
		  "a2abe02d-a43d-4c61-b502-527f1377b16b": {
			"templateId": "Item.NecrisHelm01",
			"attributes": {
			  "tradable": true
			},
			"quantity": 1
		  },
		  "db1287ba-158d-4e89-847c-8d5d4c8326ce": {
			"templateId": "Item.NecrisHelm02",
			"attributes": {
			  "tradable": true
			},
			"quantity": 1
		  },
		  "efecec94-c480-42c6-9ab7-0a913b87320c": {
			"templateId": "Item.SkaarjMale01",
			"attributes": {
			  "tradable": false
			},
			"quantity": 1
		  },
		  "64741a54-5831-4812-90ab-a80fb8f545dc": {
			"templateId": "Item.BeanieBlack",
			"attributes": {
			  "tradable": true
			},
			"quantity": 1
		  },
		  "f6258e09-9087-4b07-a0f2-20ea975dc97c": {
			"templateId": "Item.ThundercrashBeret",
			"attributes": {
			  "tradable": true
			},
			"quantity": 1
		  },
		  "58c238bb-b7ee-491b-848f-816b085ec75b": {
			"templateId": "Item.NecrisMale01",
			"attributes": {
			  "tradable": false
			},
			"quantity": 1
		  },
		  "1535b5e3-328a-4209-abfc-665471981387": {
			"templateId": "Item.Infiltrator",
			"attributes": {
			  "tradable": true
			},
			"quantity": 1
		  },
		  "d433c849-cdaf-434a-88f7-af7130a56dbf": {
			"templateId": "Item.NecrisFemale02",
			"attributes": {
			  "tradable": false
			},
			"quantity": 1
		  },
		  "d080b370-9e72-4964-bd9c-44e130e57b75": {
			"templateId": "Item.BeanieGrey",
			"attributes": {
			  "tradable": true
			},
			"quantity": 1
		  },
		  "51284c9e-f246-4147-88fd-b4045a3daca4": {
			"templateId": "Item.SkaarjMale02",
			"attributes": {
			  "tradable": false
			},
			"quantity": 1
		  }
		},
		"stats": {
		  "templateId": "profile_v2",
		  "attributes": {
			"CountryFlag": "Canada",
			"GoldStars": 20,
			"login_rewards": {
			  "nextClaimTime": null,
			  "level": 0,
			  "totalDays": 0
			},
			"Avatar": "UT.Avatar.2",
			"inventory_limit_bonus": 0,
			"daily_purchases": {},
			"in_app_purchases": {},
			"LastXPTime": 1670909243,
			"XP": 294056,
			"Level": 50,
			"BlueStars": 2,
			"RecentXP": 107,
			"boosts": [],
			"new_items": {}
		  }
		},
		"commandRevision": 3910
	  }
	}
  ],
  "profileCommandRevision": 3910,
  "serverTime": "2022-12-21T03:47:39.506Z",
  "responseVersion": 1,
  "command": "QueryProfile"
}
```

Similar data that is returned to the client when it makes a QueryProfile request.

## Update Players

```
POST https://ut-public-service-prod10.ol.epicgames.com/ut/api/matchmaking/session/4366368a402a4dd1adf3a9fd9fa15a36/players HTTP/1.1
Host: ut-public-service-prod10.ol.epicgames.com
Accept: */*
Cookie: AWSELB=E9958FDB1448870EBFBE28377F3D10376F43CDC4B19CDE0B558DCA7837C13530AF8AEE356833F1E77089BA61742B66DC413B1809DF694FA70E14833E719C9EECB1D2256DBC; AWSELBCORS=E9958FDB1448870EBFBE28377F3D10376F43CDC4B19CDE0B558DCA7837C13530AF8AEE356833F1E77089BA61742B66DC413B1809DF694FA70E14833E719C9EECB1D2256DBC
Content-Type: application/json
User-Agent: game=UnrealTournament, engine=UE4, build=++UT+Release-Next-CL-3525360
X-Epic-Correlation-ID: UE4-300D354C408D213AA3443EBEDA043C79
Authorization: bearer 47c88ffa188c46afa3807c1aefdcfcc8
Content-Length: 91
Pragma: no-cache

{
	"publicPlayers": [
		"fd83abe496ca401497b5adf4e412bf2c"
	],
	"privatePlayers": []
}
```

POSTs the new player list. Response is the usual hub update response format.

## Update Hub Session

Same as the PUT request and response in the initial hub launch sequence.
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace UT4MasterServer
{
	public class Server
	{
		private HttpListener listener;

		private UserStore users;
		private SessionStore sessions;

		public Server()
		{
			/*
			 
			Domains that need to be redirected:

			account-public-service-prod03.ol.epicgames.com
			cdn1.unrealengine.com
			content-controls-prod.ol.epicgames.net
			datarouter.ol.epicgames.com
			entitlement-public-service-prod08.ol.epicgames.com
			friends-public-service-prod06.ol.epicgames.com
			ut-public-service-prod10.ol.epicgames.com
			 
			*/


			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
			listener = new HttpListener();
			string domain = "*";//"*.epicgames.com";
			listener.Prefixes.Add($"http://{domain}:80/");
			listener.Prefixes.Add($"https://{domain}:443/");

			users = new UserStore();
			sessions = new SessionStore();
		}

		public void StartAsync()
		{
			listener.Start();
			listener.BeginGetContext(new AsyncCallback(NewConnection), null);

#if DEBUG
			// temporarily create users and valid auth codes
			sessions.CreateAuthCode(users.CreateUser("user1", "user1"));
			sessions.CreateAuthCode(users.CreateUser("user2", "user2"));
#endif
		}

		private async void NewConnection(IAsyncResult result)
		{
			HttpListenerContext context;
			try
			{
				context = listener.EndGetContext(result);
			}
			catch (ObjectDisposedException ex)
			{
				return;
			}

			listener.BeginGetContext(new AsyncCallback(NewConnection), null);

			Console.WriteLine(context.Request.RawUrl);


			var req = context.Request;
			var resp = context.Response;

			if (req.Url == null)
				return;

			using (var output = new StreamWriter(context.Response.OutputStream))
			{
				using (var input = new StreamReader(req.InputStream))
				{

					if (req.Url.AbsolutePath.StartsWith("/id/api/redirect"))
					{
						HandleEpicRedirect(req, input, resp, output);
					}
					else if (req.Url.AbsolutePath.StartsWith("/account/api/oauth/token"))
					{
						await HandleEpicAuthToken(req, input, resp, output);
					}
					else if (req.Url.AbsolutePath.StartsWith("/account/api/oauth/exchange"))
					{
						HandleEpicAuthExchange(req, input, resp, output);
					}
					else if (req.Url.AbsolutePath.StartsWith("/account/api/oauth/verify"))
					{
						HandleEpicAuthVerify(req, input, resp, output);
					}
					else if (req.Url.AbsolutePath.StartsWith("/account/api/oauth/sessions/kill"))
					{
						HandleEpicSessionsKill(req, input, resp, output);
					}
					else if (req.Url.AbsolutePath.StartsWith("/account/api/public/account"))
					{
						HandleEpicPublicAccount(req, input, resp, output);
					}
					else if (req.Url.AbsolutePath.StartsWith("/account/api/accounts"))
					{
						HandleEpicAccounts(req, input, resp, output);
					}
					else if (req.Url.AbsolutePath.StartsWith("/account/api/epicdomains/ssodomains"))
					{
						HandleEpicSSODomains(req, input, resp, output);
					}
					else if (req.Url.AbsolutePath.StartsWith("/datarouter/api/v1/public/data"))
					{
						HandleEpicDatarouter(req, input, resp, output);
					}
					else if (req.Url.AbsolutePath.StartsWith("/entitlement/api/account"))
					{
						HandleEpicEntitlements(req, input, resp, output);
					}
					else if (req.Url.AbsolutePath.StartsWith("/friends/api/public"))
					{
						HandleEpicFriends(req, input, resp, output);
					}
					else if (req.Url.AbsolutePath.StartsWith("/ut/api/game/v2/profile"))
					{
						HandleUTQueryProfile(req, input, resp, output);
					}
					else if (req.Url.AbsolutePath.StartsWith("/ut/api/game/v2/ratings"))
					{
						HandleUTGameRatings(req, input, resp, output);
					}
					else if (req.Url.AbsolutePath.StartsWith("/ut/api/cloudstorage"))
					{
						HandleEpicEntitlements(req, input, resp, output);
					}
					else if (req.Url.AbsolutePath.StartsWith("/entitlement/api/account"))
					{
						HandleEpicEntitlements(req, input, resp, output);
					}
					else if (req.Url.AbsolutePath.StartsWith("/ut/api/matchmaking/session/matchMakingRequest"))
					{
						HandleEpicEntitlements(req, input, resp, output);
					}
					else
					{
						HandleOtherMisc(req, input, resp, output);
					}
				}
			}
		}


		private void HandleEpicRedirect(HttpListenerRequest req, StreamReader input, HttpListenerResponse resp, StreamWriter output)
		{
			resp.StatusCode = 200;
			resp.ContentType = "text/plain; charset=UTF-8";
			output.Write("Your autorization code: 12345");
		}

		private async Task HandleEpicAuthToken(HttpListenerRequest req, StreamReader input, HttpListenerResponse resp, StreamWriter output)
		{
			var body = await input.ReadToEndAsync();
			if (req.HttpMethod == "POST" && req.ContentType == "application/x-www-form-urlencoded")
			{
				var decoded = HttpUtility.UrlDecode(body);
				var parameters = HttpUtility.ParseQueryString(decoded);

				var authorization = new HttpAuthorization(req);

				if (parameters["grant_type"] == "authorization_code")
				{
					// https://github.com/MixV2/EpicResearch/blob/master/docs/auth/grant_types/authorization_code.md

					var authCode = parameters["code"];

					if (!authorization.IsBasic)
						return;

					var code = parameters["code"];
					if (code == null)
						return;

					var session = sessions.CreateSessionWithAuthCode(code, new ClientIdentification(authorization.Value).ID);
					if (session == null)
						return;

					resp.StatusCode = 200;
					resp.ContentType = "application/json";
					output.Write(session.ToJson());
					return;
				}
				else if (parameters["grant_type"] == "client_credentials")
				{
					// https://github.com/MixV2/EpicResearch/blob/master/docs/auth/grant_types/client_credentials.md
					// this request only asks for publicly accessible stuff like cloud storage

					if (!authorization.IsBasic)
						return;

					var session = sessions.CreateSessionWithPublicAccess(new ClientIdentification(authorization.Value).ID);
					if (session == null)
						return;

					resp.StatusCode = 200;
					resp.ContentType = "application/json";
					output.Write(session.ToJson());
					return;
				}
				else if (parameters["grant_type"] == "exchange_code")
				{
					// https://github.com/MixV2/EpicResearch/blob/master/docs/auth/grant_types/exchange_code.md

					var code = parameters["exchange_code"];
					if (code == null)
						return;

					if (!authorization.IsBasic)
						return;

					var session = sessions.CreateSessionWithExchangeCode(code, new ClientIdentification(authorization.Value).ID);
					if (session == null)
						return;

					resp.StatusCode = 200;
					resp.ContentType = "application/json";
					output.Write(session.ToJson());
					return;
				}
			}
			resp.StatusCode = 400;
		}

		private void HandleEpicAuthExchange(HttpListenerRequest req, StreamReader input, HttpListenerResponse resp, StreamWriter output)
		{
			if (req.HttpMethod == "GET")
			{
				HttpAuthorization auth = new HttpAuthorization(req.Headers["Authorization"]);
				if (!auth.IsBearer)
					return;

				var session = sessions.GetSession(auth.Value);
				if (session == null)
					return;

				var exchangeCode = sessions.CreateExchangeCode(session.AccessToken.Value);
				if (exchangeCode == null)
					return;

				resp.StatusCode = 200;
				resp.ContentType = "application/json";
				output.Write(exchangeCode.ToJson());
				return;
			}

			resp.StatusCode = 400;
		}
		private void HandleEpicAuthVerify(HttpListenerRequest req, StreamReader input, HttpListenerResponse resp, StreamWriter output)
		{
			var authorization = new HttpAuthorization(req);
			if (authorization.IsBearer)
			{
				var session = sessions.GetSession(authorization.Value);
				if (session == null)
					return;


			}
		}
		private void HandleEpicSessionsKill(HttpListenerRequest req, StreamReader input, HttpListenerResponse resp, StreamWriter output)
		{
			if (req.HttpMethod == "DELETE")
			{
				var authorization = new HttpAuthorization(req);

				var killType = req.QueryString["killType"];
				if (killType == "OTHERS_ACCOUNT_CLIENT_SERVICE")
				{
					// kill all other sessions that exist for this client
					if (authorization.IsBearer)
					{
						sessions.KillOtherSessions(authorization.Value);
					}

				}
				else if (authorization.IsBearer)
				{
					sessions.KillSession(authorization.Value);
				}

				// URL: DELETE /account/api/oauth/sessions/kill/759877be30554dd09a8dfc2c232a0bd7
				// TODO

				resp.StatusCode = 204;
				return;
			}
			resp.StatusCode = 400;
		}

		private void HandleEpicPublicAccount(HttpListenerRequest req, StreamReader input, HttpListenerResponse resp, StreamWriter output)
		{
			// account id is in last part of url "/:accountID"
			// this has a few different forms:


			if (req.HttpMethod != "GET")
				return;
			string[] segments = req.Url.Segments;

			// URL: GET /account/api/public/account/0b0f09b400854b9b98932dd9e5abe7c5
			// OUTPUT:
			/*
			
			{
			  "id": "0b0f09b400854b9b98932dd9e5abe7c5",
			  "displayName": "timiimit",
			  "name": "...",
			  "email": "...",
			  "failedLoginAttempts": 0,
			  "lastLogin": "2022-10-25T20:45:26.258Z",
			  "numberOfDisplayNameChanges": 0,
			  "ageGroup": "UNKNOWN",
			  "headless": false,
			  "country": "SI",
			  "lastName": "...",
			  "preferredLanguage": "en",
			  "canUpdateDisplayName": true,
			  "tfaEnabled": true,
			  "emailVerified": true,
			  "minorVerified": false,
			  "minorExpected": false,
			  "minorStatus": "UNKNOWN",
			  "cabinedMode": false,
			  "hasHashedEmail": false
			}

			*/

			if (segments.Length == 6)
			{
				string accountID = segments[5];
				User? user = users.GetUserByID(new CommonID(accountID));
				if (user == null)
					return;

				JObject obj = new JObject();
				obj.Add("id", user.ID.ToString());
				obj.Add("displayName", user.Username);
				obj.Add("name", "actual_name");
				obj.Add("email", "actual_email");
				obj.Add("failedLoginAttempts", 0);
				obj.Add("lastLogin", "2022-10-25T20:45:26.258Z");
				obj.Add("numberOfDisplayNameChanges", 0);
				obj.Add("ageGroup", "UNKNOWN");
				obj.Add("headless", false);
				obj.Add("country", "SI"); // two letter country code
				obj.Add("lastName", "actual_last_name");
				obj.Add("preferredLanguage", "en");
				obj.Add("canUpdateDisplayName", true);
				obj.Add("tfaEnabled", true);
				obj.Add("emailVerified", true);
				obj.Add("minorExpected", true);
				obj.Add("minorStatus", "UNKNOWN");
				obj.Add("cabinedMode", false);
				obj.Add("hasHashedEmail", false);

				resp.StatusCode = 200;
				output.Write(obj.ToString());
				return;
			}


			// URL: GET /account/api/public/account/0b0f09b400854b9b98932dd9e5abe7c5/externalAuths
			// OUTPUT:
			/*
			
			[
			  {
				"accountId": "0b0f09b400854b9b98932dd9e5abe7c5",
				"type": "github",
				"externalAuthId": "timiimit",
				"externalDisplayName": "timiimit",
				"authIds": [
				  {
					"id": "timiimit",
					"type": "github_login"
				  }
				],
				"dateAdded": "2018-01-17T18:58:39.831Z"
			  }
			]

			*/

			if (segments.Length == 7 && segments[6] == "externalAuths")
			{
				JArray arr = new JArray();
				//// foreach {
				//JObject obj = new JObject();
				//obj.Add("accountId", "00000000000000000000000000000000");
				//obj.Add("type", "github");
				//obj.Add("externalAuthId", "username");
				//obj.Add("externalDisplayName", "username");
				//JArray authIds = new JArray();
				//// foreach {
				//JObject authId = new JObject();
				//authId.Add("id", "username");
				//authId.Add("type", "github_login");
				//authIds.Add(authId);
				//// }
				//obj.Add("authIds", authIds);
				//obj.Add("dateAdded", "00000000000000000000000000000000");
				////}
				//arr.Add(obj);

				resp.StatusCode = 200;
				output.Write(arr.ToString());
				return;
			}

			if (segments.Length == 5)
			{
				JArray arr = new JArray();
				string[] accountIDs = HttpUtility.UrlDecode(req.Url.Query).Substring(1).Split('&');
				// TODO: remove duplicates from `accountIDs`

				for (int i = 0; i < accountIDs.Length; i++)
				{
					string[] kvp = accountIDs[i].Split('=');
					if (kvp.Length == 2 && kvp[0] == "accountId")
					{
						string accountID = kvp[1];

						User? user = users.GetUserByID(new CommonID(accountID));
						if (user == null)
							continue;

						JObject obj = new JObject();
						obj.Add("id", accountID);
						obj.Add("displayName", user.Username);
						if (true)
						{
							// this is returned only when you ask about yourself
							obj.Add("minorVerified", false);
							obj.Add("minorStatus", "UNKNOWN");
							obj.Add("cabinedMode", false);
						}
						obj.Add("externalAuths", new JObject());
						arr.Add(obj);
					}
				}

				resp.StatusCode = 200;
				output.Write(arr.ToString());
				return;
			}

			// URL: GET /account/api/public/account?accountId=0b0f09b400854b9b98932dd9e5abe7c5
			// URL: GET /account/api/public/account?accountId=0b0f09b400854b9b98932dd9e5abe7c5&accountId=0b0f09b400854b9b98932dd9e5abe7c5
			// OUTPUT: output below is only produced when asked about your own profile. extenalAuths is useless for us.
			/*
			[
			  {
				"id": "0b0f09b400854b9b98932dd9e5abe7c5",
				"displayName": "timiimit",
				"minorVerified": false,
				"minorStatus": "UNKNOWN",
				"cabinedMode": false,
				"externalAuths": {
				  "github": {
					"accountId": "0b0f09b400854b9b98932dd9e5abe7c5",
					"type": "github",
					"externalAuthId": "timiimit",
					"externalAuthIdType": "github_login",
					"externalDisplayName": "timiimit",
					"authIds": [
					  {
						"id": "timiimit",
						"type": "github_login"
					  }
					]
				  }
				}
			  }
			]

			*/

			// url below contains up to 100 friend ids if player has
			// more than that, it chunks it up into multiple requests
			// URL: GET /account/api/public/account?accountId=09b8744abd524d879630f7c79365e2f8&accountId=00cecff34c124daf9c9f27b0f12347b8&accountId=03a9ed4fbb8a4ed2b379a88919d9d6f1&accountId=0143dd34031448e1a905ac91bfe72e26&accountId=082b13e1a45e4812ad398638e7954278&accountId=06f9ac25baa14471867f44bd5377b4d3&accountId=0031fba962dc4c7db885261ae5a26449&accountId=084ae3dbdfd9470abaccc96643c74aa1&accountId=09a904a43a8c47f5af24a003683e507d&accountId=058753b47cb24ea8998c037b92f3934f&accountId=0135796826204b61ae6c0a5b87766a59&accountId=02adecd712214d0cb6c7e62416763b93&accountId=06566169802f4f8bbd1122994e57d074&accountId=02841160ee184242b5f03e32a0750b01&accountId=0217c00cfba04892bc4ddce98c76256f&accountId=3c3b4966c47c4c258f07ac4d9a4e631b&accountId=50210f396e814311a851e10825bac84e&accountId=c67dea9f868f44e4a91d230662680441&accountId=f12fc487619841e69bbc05122f46276a&accountId=2a356d1a13aa44899863f25f7893dd2a&accountId=6ea07250fc994bcba6c082c8ac19e800&accountId=3f9b9d984bbd4d45882a3a59a5e7cfd7&accountId=b34156171a9943879f888ec136e1e9d2&accountId=5605b0e1d34d498fb7ee363fcd4ea9cc&accountId=b1435038b7c143a894e76fd133e7f3a9&accountId=71d9faf84c5d4ef188437676042b825a&accountId=2065cd8a20ca436888648e6e9cf369e5&accountId=2dae6e6edbd24ea1acdeda1001c81e47&accountId=6ec45bf5352b40b692b77ad7084eb091&accountId=534b7a323d8041d4b519cda8dc2a9e83&accountId=3d1a9af1e1ba4bb9b8b2c0668c157f62&accountId=5960fff9ef804c2788eb3ebcb488f8ce&accountId=f6145f2ec60b462085983ef7e15c0dcf&accountId=a9bb7567bf0a45d584c212f563e01847&accountId=830b0fb5a32c4a98acfc3b95fed5bdb7&accountId=409c425c337e44ea96825ccda3900a77&accountId=b63f96f636eb4a35b54813e7b58b6435&accountId=bf7f0e3c95a74be2a125bf95bd216a72&accountId=22436125a672409d91047aa40141cc65&accountId=26d30c18bcfc402c8b7ad0d03434b2d7&accountId=efd40e27ef9e444a8b664a8578f4379e&accountId=a730f6897408493f96388f63194133ba&accountId=80522f41f9554e45a9d830ed69e589be&accountId=b6cbcd26f6c44a389068d05b7b030aca&accountId=85d389a4ea7341c48ffd2effd272b5d5&accountId=33651bc8c5c4404aa2a916e4756862a8&accountId=f54ed0d628fb40dca8df3cdf791405f2&accountId=103e8a3edc31416292f81b1050f129f8&accountId=45d222d851794dbb8543976dcfe79f38&accountId=61067ec61a3649c7a9ba92524effba1c&accountId=d10012f8872441b483740217890f7599&accountId=155481189c214043928b84f61ee31d6c&accountId=a3b2d828afa8470893f506ecc1cedf82&accountId=79cd65d1c2d34df5901b1fdd2aa3ca7f&accountId=4b63e733081d43c1a4dc2c0a0b62b6e7&accountId=47611188b34a47219729a3865074b7a9&accountId=c245027671b04ce1a63e6ac15dcf8373&accountId=f841c0ebab5e4a4a80b7167480143684&accountId=f5c0dd7b5a8d4ad3b3621e647ac528e3&accountId=452ce36a1e19499b92d4fa96598a267b&accountId=d94b830d64764f3094babca7d583120a&accountId=3d9af50851cf4eee8d595cac8dd552d1&accountId=4407dd822e834bf1af130ed63e7ad3e5&accountId=74faf34870ba402fbd7f3135e052fdfe&accountId=ddcc10029d71432bbf287c3e339de400&accountId=fb7e3fb1180a4fd58ee4cc2fadc269cd&accountId=6ac40af99af94b01904a5627490e484c&accountId=b14503fd275b456f8766b75ec319b9ae&accountId=4bf8345d88204044bda503759b04d4b6&accountId=cf0ac9bb39944fc1904694c33ed13988&accountId=1642790656224a06828a9f1e5c9b0779&accountId=3d6a93720ea3498d9e64dad4c64f85b5&accountId=e4bf20e23d5444aea16447864d0f0896&accountId=26c87f543cfa4276b30adfd408f75a24&accountId=14e5beddb28b427e85311017cb3a0604&accountId=ba7e1baf84284cd9b900e5d016df52cb&accountId=3b8c6d3f473d44e0bca69c87134c070b&accountId=9c90c7be11bc4f81b81509c140abd0ee&accountId=f10b9bbd78924d808bf074663908a0e8&accountId=e6aa5dc56e98402d8f2b59c8a3b839d3&accountId=72d5035a6a48460e83da1153753cbfe3&accountId=4bf8a6411fbd474c894e1289cb645517&accountId=d17445e9b7e146379fe9a7d20c242d7b&accountId=437d64604d494a118d2cde240e074430&accountId=26affa78cf834b37a56b62f70fa6eb36&accountId=ed4f1edfef034041a78b075bc1011daa&accountId=75480cd6c4d14397ba5ccd1dffc53ca5&accountId=fc956dadb44846f18cb0355c3fd24dd4&accountId=6f3ab87b362f4c4680000d6bbd416941&accountId=31607ab9a41440a0880562194921ec4b&accountId=12fb2bfc835043b1940d6df97b245aab&accountId=baf2017bc8a44922bee589df64cfc5e2&accountId=192b7e5210b64c05851e62542fa954ba&accountId=3e1adbf4b7324381b8d661ab388d6f87&accountId=47c0d606abc946be9b0d1ab08128e6be&accountId=476ae5e72e1a4ecfb3e93ba2c3b7e5d8&accountId=c67e229bcc594b94bc8743eba7d974d7&accountId=2c88debef1564608bdb791de6f9785c6&accountId=28952c30152f4891990dc00619df2f14&accountId=1246551daaec4ca89e7f4020a1754d94
			// OUTPUT: output is in format specified below as an example. order of returned objects does not matter. i've omitted actual response because they include people's steam, psn, etc accounts. we don't need any info about extenalAuths as that is useless for us.
			/*
			  [
				{
				  "id": "9c90c7be11bc4f81b81509c140abd0ee",
				  "displayName": "AllTakenWTF",
				  "externalAuths": {}
				},
				{
				  "id": "ba7e1baf84284cd9b900e5d016df52cb",
				  "displayName": "ok_mistake",
				  "externalAuths": {}
				}
			  ]
			*/
		}

		private void HandleEpicAccounts(HttpListenerRequest req, StreamReader input, HttpListenerResponse resp, StreamWriter output)
		{
			// URL: GET /account/api/accounts/0b0f09b400854b9b98932dd9e5abe7c5/metadata
			// OUTPUT: {}

			resp.StatusCode = 200;
			output.Write(new JObject().ToString());
		}

		private void HandleEpicSSODomains(HttpListenerRequest req, StreamReader input, HttpListenerResponse resp, StreamWriter output)
		{
			// URL: GET /account/api/epicdomains/ssodomains
			// OUTPUT: ["unrealengine.com","unrealtournament.com","fortnite.com","epicgames.com"]

			resp.StatusCode = 200;
			output.Write(new JArray().ToString());
		}

		private void HandleEpicDatarouter(HttpListenerRequest req, StreamReader input, HttpListenerResponse resp, StreamWriter output)
		{
			// URL: POST /datarouter/api/v1/public/data??SessionID=%7B57D173A1-4BF7-5823-3A50-9B980FED67F0%7D&AppID=UnrealTournament.Dev&AppVersion=1.0.0.0%20-%20%2B%2BUT%2BMain-CL-3525109&UserID=9c808754479eaa36e7efe896f204da11%7C0b0f09b400854b9b98932dd9e5abe7c5%7C3d42b762-3e87-4d29-90d8-7f36f619c3ae%7C%7CReg&AppEnvironment=datacollector-binary&UploadType=eteventstream
			// HTTP Code: 240 No Content

			resp.StatusCode = 204;
		}

		private void HandleEpicEntitlements(HttpListenerRequest req, StreamReader input, HttpListenerResponse resp, StreamWriter output)
		{
			// URL: GET /entitlement/api/account/0b0f09b400854b9b98932dd9e5abe7c5/entitlements?namespace=ut
			// OUTPUT:
			/*
			
			[
			  {
				"id": "276d0455f7774cac9150e2d468da2620",
				"entitlementName": "UnrealTournament",
				"namespace": "ut",
				"catalogItemId": "b8538c739273426aa35a98220e258d55",
				"accountId": "0b0f09b400854b9b98932dd9e5abe7c5",
				"identityId": "0b0f09b400854b9b98932dd9e5abe7c5",
				"entitlementType": "EXECUTABLE",
				"grantDate": "2016-01-24T15:22:24.036Z",
				"consumable": false,
				"status": "ACTIVE",
				"active": true,
				"useCount": 0,
				"created": "2016-01-24T15:22:24.048Z",
				"updated": "2016-01-24T15:22:24.048Z",
				"groupEntitlement": false
			  },
			  {
				"id": "ebe5a90eaa934b7da849c77032152dd9",
				"entitlementName": "0d5e275ca99d4cf0b03c518a6b279y77",
				"namespace": "ut",
				"catalogItemId": "0d5e275ca99d4cf0b03c518a6b279e26",
				"accountId": "0b0f09b400854b9b98932dd9e5abe7c5",
				"identityId": "0b0f09b400854b9b98932dd9e5abe7c5",
				"entitlementType": "EXECUTABLE",
				"grantDate": "2016-06-06T08:26:32.662Z",
				"startDate": "2015-01-22T00:00:00.000Z",
				"consumable": false,
				"status": "ACTIVE",
				"active": true,
				"useCount": 0,
				"created": "2016-06-06T08:26:32.734Z",
				"updated": "2016-06-06T08:26:32.734Z",
				"groupEntitlement": false
			  },
			  {
				"id": "4e6f5c8f577a4c379ee2a6158ae3326d",
				"entitlementName": "48d281f487154bb29dd75bd7bb95ac8e",
				"namespace": "ut",
				"catalogItemId": "48d281f487154bb29dd75bd7bb95ac8e",
				"accountId": "0b0f09b400854b9b98932dd9e5abe7c5",
				"identityId": "0b0f09b400854b9b98932dd9e5abe7c5",
				"entitlementType": "EXECUTABLE",
				"grantDate": "2016-06-06T08:26:52.280Z",
				"startDate": "2015-09-09T00:00:00.000Z",
				"consumable": false,
				"status": "ACTIVE",
				"active": true,
				"useCount": 0,
				"created": "2016-06-06T08:26:52.304Z",
				"updated": "2016-06-06T08:26:52.304Z",
				"groupEntitlement": false
			  },
			  {
				"id": "87e81495602a42bfb8f11066886f7276",
				"entitlementName": "Unsaved",
				"namespace": "ut",
				"catalogItemId": "65fb5029cddb4de7b5fa155b6992e6a3",
				"accountId": "0b0f09b400854b9b98932dd9e5abe7c5",
				"identityId": "0b0f09b400854b9b98932dd9e5abe7c5",
				"entitlementType": "EXECUTABLE",
				"grantDate": "2017-10-12T16:20:24.736Z",
				"startDate": "2016-11-03T00:00:00.000Z",
				"consumable": false,
				"status": "ACTIVE",
				"active": true,
				"useCount": 0,
				"created": "2017-10-12T16:20:24.740Z",
				"updated": "2017-10-12T16:20:24.740Z",
				"groupEntitlement": false
			  },
			  {
				"id": "d0088647f4a040578fd6b34cc0f3c346",
				"entitlementName": "UnrealTournamentEditor",
				"namespace": "ut",
				"catalogItemId": "0cae1a97d47f4ee2ba4e112b9601637f",
				"accountId": "0b0f09b400854b9b98932dd9e5abe7c5",
				"identityId": "0b0f09b400854b9b98932dd9e5abe7c5",
				"entitlementType": "EXECUTABLE",
				"grantDate": "2017-10-19T10:14:31.737Z",
				"startDate": "2015-01-01T00:00:00.000Z",
				"consumable": false,
				"status": "ACTIVE",
				"active": true,
				"useCount": 0,
				"created": "2017-10-19T10:14:31.742Z",
				"updated": "2017-10-19T10:14:31.742Z",
				"groupEntitlement": false
			  }
			]

			*/

			var segments = req.Url.Segments;
			if (segments.Length == 6)
			{
				var accountID = segments[4];
				User? user = users.GetUserByID(new CommonID(accountID));
				if (user == null)
					return;

				JArray arr = new JArray();
				//foreach{
				JObject obj = new JObject();
				obj.Add("id", "d0088647f4a040578fd6b34cc0f3c346");
				obj.Add("entitlementName", "UnrealTournamentEditor");
				obj.Add("namespace", "ut");
				obj.Add("catalogItemId", "0cae1a97d47f4ee2ba4e112b9601637f");
				obj.Add("accountId", user.ID.ToString());
				obj.Add("identityId", user.ID.ToString());
				obj.Add("entitlementType", "EXECUTABLE"); // always?
				obj.Add("grantDate", "2017-10-19T10:14:31.737Z");
				obj.Add("startDate", "2015-01-01T00:00:00.000Z");
				obj.Add("consumable", false);
				obj.Add("status", "ACTIVE");
				obj.Add("active", true);
				obj.Add("useCount", 0);
				obj.Add("created", "2017-10-19T10:14:31.742Z");
				obj.Add("updated", "2017-10-19T10:14:31.742Z");
				obj.Add("groupEntitlement", false);
				arr.Add(obj);
				//}
				resp.StatusCode = 200;
				output.Write(arr.ToString());
			}
		}

		private void HandleEpicFriends(HttpListenerRequest req, StreamReader input, HttpListenerResponse resp, StreamWriter output)
		{
			string subUrl = req.Url.AbsolutePath.Substring("/friends/api/public".Length);

			// URL: GET /friends/api/public/friends/0b0f09b400854b9b98932dd9e5abe7c5?includePending=true
			// OUTPUT: [{"accountId":"09b8744abd524d879630f7c79365e2f8","status":"ACCEPTED","direction":"INBOUND","created":"2018-09-05T14:55:04.261Z","favorite":false},{"accountId":"00cecff34c124daf9c9f27b0f12347b8","status":"ACCEPTED","direction":"OUTBOUND","created":"2020-05-24T16:46:23.101Z","favorite":false},{"accountId":"03a9ed4fbb8a4ed2b379a88919d9d6f1","status":"ACCEPTED","direction":"OUTBOUND","created":"2018-06-06T13:44:54.765Z","favorite":false},{"accountId":"0143dd34031448e1a905ac91bfe72e26","status":"ACCEPTED","direction":"INBOUND","created":"2018-06-20T13:13:54.570Z","favorite":false},{"accountId":"082b13e1a45e4812ad398638e7954278","status":"ACCEPTED","direction":"INBOUND","created":"2018-11-17T10:25:05.078Z","favorite":false},{"accountId":"06f9ac25baa14471867f44bd5377b4d3","status":"ACCEPTED","direction":"OUTBOUND","created":"2018-05-26T11:41:22.726Z","favorite":false},{"accountId":"0031fba962dc4c7db885261ae5a26449","status":"ACCEPTED","direction":"INBOUND","created":"2018-01-19T16:19:56.325Z","favorite":false},{"accountId":"084ae3dbdfd9470abaccc96643c74aa1","status":"ACCEPTED","direction":"INBOUND","created":"2017-09-09T16:28:48.455Z","favorite":false},{"accountId":"09a904a43a8c47f5af24a003683e507d","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-10-06T07:56:07.482Z","favorite":false},{"accountId":"058753b47cb24ea8998c037b92f3934f","status":"ACCEPTED","direction":"INBOUND","created":"2019-02-12T09:31:38.040Z","favorite":false},{"accountId":"0135796826204b61ae6c0a5b87766a59","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-06-28T13:29:33.646Z","favorite":false},{"accountId":"02adecd712214d0cb6c7e62416763b93","status":"ACCEPTED","direction":"INBOUND","created":"2018-01-25T14:56:44.774Z","favorite":false},{"accountId":"06566169802f4f8bbd1122994e57d074","status":"ACCEPTED","direction":"OUTBOUND","created":"2021-08-14T17:36:53.770Z","favorite":false},{"accountId":"02841160ee184242b5f03e32a0750b01","status":"ACCEPTED","direction":"INBOUND","created":"2021-11-10T18:12:43.587Z","favorite":false},{"accountId":"0217c00cfba04892bc4ddce98c76256f","status":"ACCEPTED","direction":"INBOUND","created":"2018-02-01T15:59:23.104Z","favorite":false},{"accountId":"3c3b4966c47c4c258f07ac4d9a4e631b","status":"ACCEPTED","direction":"OUTBOUND","created":"2016-05-16T09:21:28.308Z","favorite":false},{"accountId":"50210f396e814311a851e10825bac84e","status":"ACCEPTED","direction":"INBOUND","created":"2016-06-01T16:31:49.446Z","favorite":false},{"accountId":"c67dea9f868f44e4a91d230662680441","status":"ACCEPTED","direction":"OUTBOUND","created":"2017-05-02T08:20:09.031Z","favorite":false},{"accountId":"f12fc487619841e69bbc05122f46276a","status":"ACCEPTED","direction":"INBOUND","created":"2017-09-27T16:02:01.348Z","favorite":false},{"accountId":"2a356d1a13aa44899863f25f7893dd2a","status":"ACCEPTED","direction":"INBOUND","created":"2017-09-14T13:35:48.311Z","favorite":false},{"accountId":"6ea07250fc994bcba6c082c8ac19e800","status":"ACCEPTED","direction":"INBOUND","created":"2017-09-22T18:57:08.378Z","favorite":false},{"accountId":"3f9b9d984bbd4d45882a3a59a5e7cfd7","status":"ACCEPTED","direction":"INBOUND","created":"2017-11-28T20:56:31.246Z","favorite":false},{"accountId":"b34156171a9943879f888ec136e1e9d2","status":"ACCEPTED","direction":"OUTBOUND","created":"2017-12-15T18:48:46.888Z","favorite":false},{"accountId":"5605b0e1d34d498fb7ee363fcd4ea9cc","status":"ACCEPTED","direction":"INBOUND","created":"2018-01-02T14:56:53.161Z","favorite":false},{"accountId":"b1435038b7c143a894e76fd133e7f3a9","status":"ACCEPTED","direction":"INBOUND","created":"2018-01-19T08:53:58.484Z","favorite":false},{"accountId":"71d9faf84c5d4ef188437676042b825a","status":"ACCEPTED","direction":"INBOUND","created":"2018-01-20T15:30:45.729Z","favorite":false},{"accountId":"2065cd8a20ca436888648e6e9cf369e5","status":"ACCEPTED","direction":"INBOUND","created":"2018-01-20T15:54:13.715Z","favorite":false},{"accountId":"2dae6e6edbd24ea1acdeda1001c81e47","status":"ACCEPTED","direction":"INBOUND","created":"2018-01-17T08:46:08.900Z","favorite":false},{"accountId":"6ec45bf5352b40b692b77ad7084eb091","status":"ACCEPTED","direction":"OUTBOUND","created":"2018-01-27T11:56:09.942Z","favorite":false},{"accountId":"534b7a323d8041d4b519cda8dc2a9e83","status":"ACCEPTED","direction":"INBOUND","created":"2018-01-25T14:04:17.567Z","favorite":false},{"accountId":"3d1a9af1e1ba4bb9b8b2c0668c157f62","status":"ACCEPTED","direction":"INBOUND","created":"2018-01-31T13:44:45.967Z","favorite":false},{"accountId":"5960fff9ef804c2788eb3ebcb488f8ce","status":"ACCEPTED","direction":"OUTBOUND","created":"2018-02-11T10:15:53.091Z","favorite":false},{"accountId":"f6145f2ec60b462085983ef7e15c0dcf","status":"ACCEPTED","direction":"INBOUND","created":"2018-02-09T11:30:06.976Z","favorite":false},{"accountId":"a9bb7567bf0a45d584c212f563e01847","status":"ACCEPTED","direction":"OUTBOUND","created":"2018-02-17T10:32:13.970Z","favorite":false},{"accountId":"830b0fb5a32c4a98acfc3b95fed5bdb7","status":"ACCEPTED","direction":"INBOUND","created":"2018-02-23T10:19:33.003Z","favorite":false},{"accountId":"409c425c337e44ea96825ccda3900a77","status":"ACCEPTED","direction":"INBOUND","created":"2018-02-20T09:38:08.731Z","favorite":false},{"accountId":"b63f96f636eb4a35b54813e7b58b6435","status":"ACCEPTED","direction":"INBOUND","created":"2018-04-29T08:37:04.591Z","favorite":false},{"accountId":"bf7f0e3c95a74be2a125bf95bd216a72","status":"ACCEPTED","direction":"INBOUND","created":"2018-05-01T08:09:17.075Z","favorite":false},{"accountId":"22436125a672409d91047aa40141cc65","status":"ACCEPTED","direction":"INBOUND","created":"2018-05-03T10:37:22.396Z","favorite":false},{"accountId":"26d30c18bcfc402c8b7ad0d03434b2d7","status":"ACCEPTED","direction":"INBOUND","created":"2018-05-20T16:31:39.423Z","favorite":false},{"accountId":"efd40e27ef9e444a8b664a8578f4379e","status":"ACCEPTED","direction":"INBOUND","created":"2018-05-23T05:58:36.279Z","favorite":false},{"accountId":"a730f6897408493f96388f63194133ba","status":"ACCEPTED","direction":"INBOUND","created":"2018-05-23T14:39:57.177Z","favorite":false},{"accountId":"80522f41f9554e45a9d830ed69e589be","status":"ACCEPTED","direction":"INBOUND","created":"2018-05-25T17:24:19.067Z","favorite":false},{"accountId":"b6cbcd26f6c44a389068d05b7b030aca","status":"ACCEPTED","direction":"INBOUND","created":"2018-05-31T15:01:38.241Z","favorite":false},{"accountId":"85d389a4ea7341c48ffd2effd272b5d5","status":"ACCEPTED","direction":"INBOUND","created":"2018-06-06T13:39:22.996Z","favorite":false},{"accountId":"33651bc8c5c4404aa2a916e4756862a8","status":"ACCEPTED","direction":"OUTBOUND","created":"2018-06-12T17:54:58.592Z","favorite":false},{"accountId":"f54ed0d628fb40dca8df3cdf791405f2","status":"ACCEPTED","direction":"INBOUND","created":"2018-06-12T16:22:36.142Z","favorite":false},{"accountId":"103e8a3edc31416292f81b1050f129f8","status":"ACCEPTED","direction":"OUTBOUND","created":"2018-06-12T14:09:35.441Z","favorite":false},{"accountId":"45d222d851794dbb8543976dcfe79f38","status":"ACCEPTED","direction":"OUTBOUND","created":"2018-06-13T16:42:17.161Z","favorite":false},{"accountId":"61067ec61a3649c7a9ba92524effba1c","status":"ACCEPTED","direction":"OUTBOUND","created":"2018-06-13T18:53:34.543Z","favorite":false},{"accountId":"d10012f8872441b483740217890f7599","status":"ACCEPTED","direction":"INBOUND","created":"2018-06-14T10:09:21.129Z","favorite":false},{"accountId":"155481189c214043928b84f61ee31d6c","status":"ACCEPTED","direction":"INBOUND","created":"2018-06-17T10:59:12.044Z","favorite":false},{"accountId":"a3b2d828afa8470893f506ecc1cedf82","status":"ACCEPTED","direction":"OUTBOUND","created":"2018-06-28T16:59:17.501Z","favorite":false},{"accountId":"79cd65d1c2d34df5901b1fdd2aa3ca7f","status":"ACCEPTED","direction":"INBOUND","created":"2018-06-28T16:41:18.863Z","favorite":false},{"accountId":"4b63e733081d43c1a4dc2c0a0b62b6e7","status":"ACCEPTED","direction":"OUTBOUND","created":"2018-07-01T11:04:21.845Z","favorite":false},{"accountId":"47611188b34a47219729a3865074b7a9","status":"ACCEPTED","direction":"INBOUND","created":"2018-07-03T16:07:40.357Z","favorite":false},{"accountId":"c245027671b04ce1a63e6ac15dcf8373","status":"ACCEPTED","direction":"INBOUND","created":"2018-07-05T13:43:00.957Z","favorite":false},{"accountId":"f841c0ebab5e4a4a80b7167480143684","status":"ACCEPTED","direction":"INBOUND","created":"2018-08-08T15:21:22.478Z","favorite":false},{"accountId":"f5c0dd7b5a8d4ad3b3621e647ac528e3","status":"ACCEPTED","direction":"INBOUND","created":"2018-08-11T11:46:04.819Z","favorite":false},{"accountId":"452ce36a1e19499b92d4fa96598a267b","status":"ACCEPTED","direction":"INBOUND","created":"2018-08-16T19:52:40.872Z","favorite":false},{"accountId":"d94b830d64764f3094babca7d583120a","status":"ACCEPTED","direction":"INBOUND","created":"2018-08-18T10:35:27.087Z","favorite":false},{"accountId":"3d9af50851cf4eee8d595cac8dd552d1","status":"ACCEPTED","direction":"INBOUND","created":"2018-08-19T07:41:10.712Z","favorite":false},{"accountId":"4407dd822e834bf1af130ed63e7ad3e5","status":"ACCEPTED","direction":"OUTBOUND","created":"2018-08-26T09:39:21.504Z","favorite":false},{"accountId":"74faf34870ba402fbd7f3135e052fdfe","status":"ACCEPTED","direction":"INBOUND","created":"2018-09-01T13:41:37.227Z","favorite":false},{"accountId":"ddcc10029d71432bbf287c3e339de400","status":"ACCEPTED","direction":"OUTBOUND","created":"2018-09-04T15:08:06.888Z","favorite":false},{"accountId":"fb7e3fb1180a4fd58ee4cc2fadc269cd","status":"ACCEPTED","direction":"INBOUND","created":"2018-09-08T08:51:46.189Z","favorite":false},{"accountId":"6ac40af99af94b01904a5627490e484c","status":"ACCEPTED","direction":"INBOUND","created":"2018-09-09T13:54:08.169Z","favorite":false},{"accountId":"b14503fd275b456f8766b75ec319b9ae","status":"ACCEPTED","direction":"INBOUND","created":"2018-09-25T15:38:44.139Z","favorite":false},{"accountId":"4bf8345d88204044bda503759b04d4b6","status":"ACCEPTED","direction":"OUTBOUND","created":"2018-10-04T13:55:48.203Z","favorite":false},{"accountId":"cf0ac9bb39944fc1904694c33ed13988","status":"ACCEPTED","direction":"INBOUND","created":"2018-10-05T17:34:40.742Z","favorite":false},{"accountId":"1642790656224a06828a9f1e5c9b0779","status":"ACCEPTED","direction":"INBOUND","created":"2018-10-08T14:33:03.448Z","favorite":false},{"accountId":"3d6a93720ea3498d9e64dad4c64f85b5","status":"ACCEPTED","direction":"OUTBOUND","created":"2018-10-13T12:26:32.931Z","favorite":false},{"accountId":"e4bf20e23d5444aea16447864d0f0896","status":"ACCEPTED","direction":"OUTBOUND","created":"2018-10-24T15:08:47.258Z","favorite":false},{"accountId":"26c87f543cfa4276b30adfd408f75a24","status":"ACCEPTED","direction":"INBOUND","created":"2018-11-04T15:28:17.018Z","favorite":false},{"accountId":"14e5beddb28b427e85311017cb3a0604","status":"ACCEPTED","direction":"INBOUND","created":"2018-12-29T20:57:08.803Z","favorite":false},{"accountId":"ba7e1baf84284cd9b900e5d016df52cb","status":"ACCEPTED","direction":"INBOUND","created":"2018-12-30T23:16:23.734Z","favorite":false},{"accountId":"3b8c6d3f473d44e0bca69c87134c070b","status":"ACCEPTED","direction":"INBOUND","created":"2019-01-11T16:41:23.798Z","favorite":false},{"accountId":"9c90c7be11bc4f81b81509c140abd0ee","status":"ACCEPTED","direction":"INBOUND","created":"2019-01-27T10:16:28.441Z","favorite":false},{"accountId":"f10b9bbd78924d808bf074663908a0e8","status":"ACCEPTED","direction":"INBOUND","created":"2019-02-01T14:15:42.359Z","favorite":false},{"accountId":"e6aa5dc56e98402d8f2b59c8a3b839d3","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-02-01T14:59:06.304Z","favorite":false},{"accountId":"72d5035a6a48460e83da1153753cbfe3","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-02-01T14:59:14.519Z","favorite":false},{"accountId":"4bf8a6411fbd474c894e1289cb645517","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-02-03T17:55:51.163Z","favorite":false},{"accountId":"d17445e9b7e146379fe9a7d20c242d7b","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-02-03T17:56:03.410Z","favorite":false},{"accountId":"437d64604d494a118d2cde240e074430","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-02-09T17:49:09.950Z","favorite":false},{"accountId":"26affa78cf834b37a56b62f70fa6eb36","status":"ACCEPTED","direction":"INBOUND","created":"2019-02-11T09:49:57.637Z","favorite":false},{"accountId":"ed4f1edfef034041a78b075bc1011daa","status":"ACCEPTED","direction":"INBOUND","created":"2019-02-12T19:04:00.397Z","favorite":false},{"accountId":"75480cd6c4d14397ba5ccd1dffc53ca5","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-02-13T16:41:15.458Z","favorite":false},{"accountId":"fc956dadb44846f18cb0355c3fd24dd4","status":"ACCEPTED","direction":"INBOUND","created":"2019-02-13T17:07:51.110Z","favorite":false},{"accountId":"6f3ab87b362f4c4680000d6bbd416941","status":"ACCEPTED","direction":"INBOUND","created":"2019-04-06T18:45:06.912Z","favorite":false},{"accountId":"31607ab9a41440a0880562194921ec4b","status":"ACCEPTED","direction":"INBOUND","created":"2019-04-21T09:36:14.382Z","favorite":false},{"accountId":"12fb2bfc835043b1940d6df97b245aab","status":"ACCEPTED","direction":"INBOUND","created":"2019-04-23T16:33:57.808Z","favorite":false},{"accountId":"baf2017bc8a44922bee589df64cfc5e2","status":"ACCEPTED","direction":"INBOUND","created":"2019-04-29T12:05:56.995Z","favorite":false},{"accountId":"192b7e5210b64c05851e62542fa954ba","status":"ACCEPTED","direction":"INBOUND","created":"2019-04-29T13:47:20.206Z","favorite":false},{"accountId":"3e1adbf4b7324381b8d661ab388d6f87","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-05-01T07:56:18.772Z","favorite":false},{"accountId":"47c0d606abc946be9b0d1ab08128e6be","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-05-07T13:11:23.100Z","favorite":false},{"accountId":"476ae5e72e1a4ecfb3e93ba2c3b7e5d8","status":"ACCEPTED","direction":"INBOUND","created":"2019-05-07T13:34:15.462Z","favorite":false},{"accountId":"c67e229bcc594b94bc8743eba7d974d7","status":"ACCEPTED","direction":"INBOUND","created":"2019-05-09T07:44:45.766Z","favorite":false},{"accountId":"2c88debef1564608bdb791de6f9785c6","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-05-10T10:38:16.338Z","favorite":false},{"accountId":"28952c30152f4891990dc00619df2f14","status":"ACCEPTED","direction":"INBOUND","created":"2019-05-11T09:47:34.257Z","favorite":false},{"accountId":"1246551daaec4ca89e7f4020a1754d94","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-05-11T19:40:01.843Z","favorite":false},{"accountId":"ba4e260a95214ed0994d02b88bd4c4ed","status":"ACCEPTED","direction":"INBOUND","created":"2019-05-12T07:22:50.240Z","favorite":false},{"accountId":"e269a71d65f646bcb7273d5307c31251","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-05-16T18:06:30.859Z","favorite":false},{"accountId":"571ebd86bbda49058981d8e7dfac175e","status":"ACCEPTED","direction":"INBOUND","created":"2019-05-19T15:15:45.354Z","favorite":false},{"accountId":"15f94f20df8a499f8f9e5b93552d1f21","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-05-19T16:02:26.797Z","favorite":false},{"accountId":"77ddda9d2ac042edbf50582a3e7bccd3","status":"ACCEPTED","direction":"INBOUND","created":"2019-05-20T17:38:34.453Z","favorite":false},{"accountId":"0fe318bc3a074b6788e05e1fae94c9bc","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-06-07T07:53:38.325Z","favorite":false},{"accountId":"b79f67d8bce848ff90fa62b28d35a48b","status":"ACCEPTED","direction":"INBOUND","created":"2019-06-27T14:47:42.210Z","favorite":false},{"accountId":"6dec9bf4bf1244008e066b3b39b87d47","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-07-09T14:04:47.073Z","favorite":false},{"accountId":"a8dd3d1b15ff4fa4a30bca9e5c8a676a","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-07-09T14:06:08.372Z","favorite":false},{"accountId":"f9122cb3c40b4f69b71d013c75bccf91","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-07-09T17:56:15.821Z","favorite":false},{"accountId":"e99c9661c78b4643b6a693097ad09512","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-07-09T17:56:42.601Z","favorite":false},{"accountId":"c4e345a000cc456d879f009d7383c3bf","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-07-09T17:56:45.013Z","favorite":false},{"accountId":"1f3d3fac4ed14c488a308b1c3d64aa35","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-07-11T15:11:54.301Z","favorite":false},{"accountId":"eb01c8d1e89a4901bd9a5d952f3942d5","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-07-11T15:11:56.698Z","favorite":false},{"accountId":"3d74738e7cb74d1389facd8e6193bcbb","status":"ACCEPTED","direction":"INBOUND","created":"2019-07-13T08:47:20.070Z","favorite":false},{"accountId":"c6d1b935cc664063aef2d2db32af3be2","status":"ACCEPTED","direction":"INBOUND","created":"2019-07-15T15:13:40.637Z","favorite":false},{"accountId":"6a3905fb16ef42fa8fdc4e513f109630","status":"ACCEPTED","direction":"INBOUND","created":"2019-08-05T18:18:34.594Z","favorite":false},{"accountId":"a6acbc5023824233a93737f36a55919c","status":"ACCEPTED","direction":"INBOUND","created":"2019-08-06T12:45:47.221Z","favorite":false},{"accountId":"47acd8f757dc44fcb217443c60558d5f","status":"ACCEPTED","direction":"INBOUND","created":"2019-08-06T22:07:18.778Z","favorite":false},{"accountId":"71e81aec8e80429f8511b253c73c0174","status":"ACCEPTED","direction":"INBOUND","created":"2019-08-08T15:43:31.470Z","favorite":false},{"accountId":"6ee9bf4b813648e0bb524fd9391f50a9","status":"ACCEPTED","direction":"INBOUND","created":"2019-08-12T15:00:45.168Z","favorite":false},{"accountId":"7783040fb96c47de933a7088c3f49142","status":"ACCEPTED","direction":"INBOUND","created":"2019-08-13T08:02:41.443Z","favorite":false},{"accountId":"2f189e77212744d39ff9797a905e3558","status":"ACCEPTED","direction":"INBOUND","created":"2019-08-17T11:56:12.063Z","favorite":false},{"accountId":"8182147740b64c89a57e280df9a79c7e","status":"ACCEPTED","direction":"INBOUND","created":"2019-08-24T14:59:54.448Z","favorite":false},{"accountId":"781433574c814cd4a53698f63a643809","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-10-05T14:39:13.565Z","favorite":false},{"accountId":"5759b45004324ababb16f375e3a208d9","status":"ACCEPTED","direction":"INBOUND","created":"2019-10-10T12:46:52.985Z","favorite":false},{"accountId":"4afdaa4cd2ce4ff798f3cfe2f02c32ef","status":"ACCEPTED","direction":"OUTBOUND","created":"2019-11-20T20:25:37.879Z","favorite":false},{"accountId":"cad816287f964dc196e0cee4beb8dce5","status":"ACCEPTED","direction":"INBOUND","created":"2019-11-21T08:51:35.002Z","favorite":false},{"accountId":"ce7e1c950d984ad1a8d3fdfa449b1160","status":"ACCEPTED","direction":"OUTBOUND","created":"2020-01-05T16:10:33.651Z","favorite":false},{"accountId":"5b7bb361ae764ee0be4fa1f6d805ac40","status":"ACCEPTED","direction":"OUTBOUND","created":"2020-01-11T21:22:49.679Z","favorite":false},{"accountId":"8a6084367d424611b7e6f2e5a3b80058","status":"ACCEPTED","direction":"INBOUND","created":"2020-02-01T20:58:29.418Z","favorite":false},{"accountId":"652ab9b45ede4f499a7a78ae09b6fa05","status":"ACCEPTED","direction":"OUTBOUND","created":"2020-02-01T20:58:43.360Z","favorite":false},{"accountId":"4b963f9120f9459e91dab7d12b1525d7","status":"ACCEPTED","direction":"OUTBOUND","created":"2020-02-01T20:58:49.053Z","favorite":false},{"accountId":"ad6ccde624674447974ab4836e0f6ba3","status":"ACCEPTED","direction":"OUTBOUND","created":"2020-02-01T20:58:51.027Z","favorite":false},{"accountId":"571d3da10e914231b36d78e208124e46","status":"ACCEPTED","direction":"OUTBOUND","created":"2020-02-01T20:58:53.353Z","favorite":false},{"accountId":"c84bb2eab19f47a4aa319de61f4ef59b","status":"ACCEPTED","direction":"OUTBOUND","created":"2020-02-01T21:35:03.142Z","favorite":false},{"accountId":"c26526feaf144c7ba435e3ed1c05068e","status":"ACCEPTED","direction":"OUTBOUND","created":"2020-02-02T10:43:36.957Z","favorite":false},{"accountId":"270df6193dea42cba1da7ee38902efc0","status":"ACCEPTED","direction":"OUTBOUND","created":"2020-02-04T15:12:53.807Z","favorite":false},{"accountId":"8eded79f56604a7ea17a5429a660ddb0","status":"ACCEPTED","direction":"OUTBOUND","created":"2020-02-04T15:12:57.057Z","favorite":false},{"accountId":"ee89db8296c74bd5a207ed29dbc406e4","status":"ACCEPTED","direction":"OUTBOUND","created":"2020-02-04T18:37:10.470Z","favorite":false},{"accountId":"265b3ce7ac454169b15b0166f4d59b3d","status":"ACCEPTED","direction":"INBOUND","created":"2020-02-06T10:59:12.369Z","favorite":false},{"accountId":"8fe861e91e8d4303913f23734d9636b7","status":"ACCEPTED","direction":"OUTBOUND","created":"2020-02-10T12:01:17.132Z","favorite":false},{"accountId":"819ab39fa4b74227bed471517839190e","status":"ACCEPTED","direction":"INBOUND","created":"2020-02-26T17:01:09.913Z","favorite":false},{"accountId":"a605b8c930ad4df795b086008d5077f9","status":"ACCEPTED","direction":"OUTBOUND","created":"2020-03-21T17:01:49.745Z","favorite":false},{"accountId":"8eb7081152bb4f6cb55b3fd4ebd0fb9b","status":"ACCEPTED","direction":"INBOUND","created":"2020-04-26T08:43:51.296Z","favorite":false},{"accountId":"7d2c27cfc090473ea7dd5153f5a671d3","status":"ACCEPTED","direction":"OUTBOUND","created":"2020-05-10T11:49:39.721Z","favorite":false},{"accountId":"6ae69ced39a94ade9b3ad7f0da8e766f","status":"ACCEPTED","direction":"INBOUND","created":"2020-05-21T15:21:00.441Z","favorite":false},{"accountId":"b6436cd2ba7d4f64829738d11bba51b0","status":"ACCEPTED","direction":"INBOUND","created":"2020-05-21T22:22:43.492Z","favorite":false},{"accountId":"4890eb1f42074b7f8c0f3763fae92e49","status":"ACCEPTED","direction":"INBOUND","created":"2020-05-30T10:07:26.861Z","favorite":false},{"accountId":"d3344092009e49c8b8ca33c5f4aaf090","status":"ACCEPTED","direction":"INBOUND","created":"2020-06-03T16:44:25.875Z","favorite":false},{"accountId":"45c717c8e237421c8866b676afd10cb4","status":"ACCEPTED","direction":"INBOUND","created":"2020-06-07T21:08:08.275Z","favorite":false},{"accountId":"ff37c167cb4a4595a02d0a1b59df8868","status":"ACCEPTED","direction":"INBOUND","created":"2020-06-08T17:24:47.014Z","favorite":false},{"accountId":"4714c4601061486b9736a7352d35cf3b","status":"ACCEPTED","direction":"OUTBOUND","created":"2020-06-12T15:27:43.321Z","favorite":false},{"accountId":"615a9047542e481e988c4dd3ed4fa3f7","status":"ACCEPTED","direction":"INBOUND","created":"2020-06-17T16:16:26.286Z","favorite":false},{"accountId":"7007a7ea38ba4422a4e554b6a59880c1","status":"ACCEPTED","direction":"INBOUND","created":"2020-06-19T17:37:43.487Z","favorite":false},{"accountId":"2b7cdff3355b42d6a8012cda9adf14cc","status":"ACCEPTED","direction":"OUTBOUND","created":"2020-06-25T10:39:35.732Z","favorite":false},{"accountId":"676d4fa3b8224385b76ee1f4148a8578","status":"ACCEPTED","direction":"INBOUND","created":"2020-12-31T17:55:46.923Z","favorite":false},{"accountId":"1f80b8ccc5fe4438b5bcbd83e5f746b3","status":"ACCEPTED","direction":"INBOUND","created":"2021-01-10T10:47:32.062Z","favorite":false},{"accountId":"6a113d67322148b285d198ccdc05a20c","status":"ACCEPTED","direction":"OUTBOUND","created":"2021-01-30T16:03:28.563Z","favorite":false},{"accountId":"5993aa7ab7534c8f89a64a4c18c5cf85","status":"ACCEPTED","direction":"INBOUND","created":"2021-02-19T02:01:57.462Z","favorite":false},{"accountId":"2131e12b34a640c0a0cd5d43e571efe3","status":"ACCEPTED","direction":"INBOUND","created":"2021-02-27T21:10:51.284Z","favorite":false},{"accountId":"e811b762e42f488482629b2a67c52bb5","status":"ACCEPTED","direction":"INBOUND","created":"2021-03-01T19:07:54.981Z","favorite":false},{"accountId":"74d031b2b2394bb69d568fe402336942","status":"ACCEPTED","direction":"INBOUND","created":"2021-03-04T20:51:05.174Z","favorite":false},{"accountId":"42ac65c9c4f9410d9a06cb506a4e434f","status":"ACCEPTED","direction":"INBOUND","created":"2021-03-05T19:15:25.317Z","favorite":false},{"accountId":"4750302f842c46609c8c4b55cb51245e","status":"ACCEPTED","direction":"INBOUND","created":"2021-05-20T13:53:43.696Z","favorite":false},{"accountId":"7ee7bcd8688b42ac9ddff15b4aee38f7","status":"ACCEPTED","direction":"INBOUND","created":"2021-07-11T09:43:09.140Z","favorite":false},{"accountId":"71586ef25ff34855b93aeb16cea37b8d","status":"ACCEPTED","direction":"OUTBOUND","created":"2021-07-22T17:58:59.787Z","favorite":false},{"accountId":"5c21aa0eb0ec46fead0d43a7ea7bf4f4","status":"ACCEPTED","direction":"OUTBOUND","created":"2021-08-14T17:10:36.290Z","favorite":false},{"accountId":"689b820a151f41faa15cdcbc8b25b191","status":"ACCEPTED","direction":"OUTBOUND","created":"2021-08-14T17:32:07.597Z","favorite":false},{"accountId":"2f6306ab81e8469eb05944134e07271b","status":"ACCEPTED","direction":"OUTBOUND","created":"2021-08-14T17:36:41.669Z","favorite":false},{"accountId":"3285d7e2cb384dc2bb2e5b2ef82c136d","status":"ACCEPTED","direction":"INBOUND","created":"2021-08-17T19:12:55.171Z","favorite":false},{"accountId":"827ccd56343440dca2f989700fbd4971","status":"ACCEPTED","direction":"OUTBOUND","created":"2021-08-24T17:25:22.832Z","favorite":false},{"accountId":"8d28e001f3a14dd3ba0726e071300251","status":"ACCEPTED","direction":"OUTBOUND","created":"2021-08-24T17:25:24.902Z","favorite":false},{"accountId":"33589eb2fe44459496623353428671fb","status":"ACCEPTED","direction":"INBOUND","created":"2021-08-26T19:26:08.779Z","favorite":false},{"accountId":"6492c09bd7e0481187c23e58e8eb1dc0","status":"ACCEPTED","direction":"OUTBOUND","created":"2021-08-29T13:56:15.888Z","favorite":false},{"accountId":"7e0b83579d62444fa1750f9c50580283","status":"ACCEPTED","direction":"INBOUND","created":"2021-09-01T13:26:15.719Z","favorite":false},{"accountId":"7e3bc2d8a1c947ef8db5298d89444a41","status":"ACCEPTED","direction":"INBOUND","created":"2021-09-11T22:28:42.728Z","favorite":false},{"accountId":"65fa1f1d5c4d412f93499b43bf91a4b3","status":"ACCEPTED","direction":"INBOUND","created":"2021-09-13T13:40:14.503Z","favorite":false},{"accountId":"688e41487c984ed2b6c6c1e867655c02","status":"ACCEPTED","direction":"OUTBOUND","created":"2021-09-13T22:16:04.407Z","favorite":false},{"accountId":"2f81e7990a76406db5cac98a9a4321ec","status":"ACCEPTED","direction":"OUTBOUND","created":"2021-09-13T22:16:05.497Z","favorite":false},{"accountId":"14d2996cc20a4f538e92d0768c6518aa","status":"ACCEPTED","direction":"OUTBOUND","created":"2021-09-13T22:16:06.925Z","favorite":false},{"accountId":"5e15bb00834649ab811f3b0cc20113a5","status":"ACCEPTED","direction":"OUTBOUND","created":"2021-09-18T21:23:06.247Z","favorite":false},{"accountId":"53a3264dd33b4634997c6173c0bfefce","status":"ACCEPTED","direction":"OUTBOUND","created":"2021-09-21T16:36:06.423Z","favorite":false},{"accountId":"ce0d061f28c84d13bb82274ec4c3a2a2","status":"ACCEPTED","direction":"INBOUND","created":"2021-10-09T12:41:40.162Z","favorite":false},{"accountId":"e39d0307599d4e9fa723ee8de278081b","status":"ACCEPTED","direction":"INBOUND","created":"2021-10-10T06:01:14.636Z","favorite":false},{"accountId":"7ecda7f5241e4ef59c990603fa7790f8","status":"ACCEPTED","direction":"INBOUND","created":"2021-10-13T18:17:49.622Z","favorite":false},{"accountId":"f4385c7a4d0942388e185b2c8183a646","status":"ACCEPTED","direction":"INBOUND","created":"2021-10-28T09:19:47.948Z","favorite":false},{"accountId":"6a747d1b1e7943028795e1146db0deef","status":"ACCEPTED","direction":"OUTBOUND","created":"2021-10-30T12:17:16.847Z","favorite":false},{"accountId":"ef8028b3f0534b2399f3fb294b89f154","status":"ACCEPTED","direction":"INBOUND","created":"2022-01-28T06:08:21.336Z","favorite":false},{"accountId":"955f079021d74da1a3b1a018baf39909","status":"ACCEPTED","direction":"INBOUND","created":"2022-07-05T17:58:44.936Z","favorite":false},{"accountId":"582b550757904e1283e9fdf57af4ba77","status":"ACCEPTED","direction":"INBOUND","created":"2022-10-10T12:52:13.482Z","favorite":false},{"accountId":"a67f1b2527c94add8c0ef6c84f8580c3","status":"ACCEPTED","direction":"OUTBOUND","created":"2022-10-17T17:51:30.667Z","favorite":false},{"accountId":"e5f4b9506c434a1e9b5141876aabb635","status":"ACCEPTED","direction":"INBOUND","created":"2022-10-18T18:08:03.959Z","favorite":false},{"accountId":"64bf8c6d81004e88823d577abe157373","status":"PENDING","direction":"OUTBOUND","created":"2022-10-16T15:14:32.356Z","favorite":false}]
			// OUTPUT PART
			/*
			
			  {
				"accountId": "09b8744abd524d879630f7c79365e2f8",
				"status": "ACCEPTED",
				"direction": "INBOUND",
				"created": "2018-09-05T14:55:04.261Z",
				"favorite": false
			  },

			*/



			// URL: GET /friends/api/public/blocklist/0b0f09b400854b9b98932dd9e5abe7c5
			// OUTPUT: {"blockedUsers":["4ab7f6139b35468080f8a7b8bb4334d5"]}

			bool wantsFriends = false;
			if (subUrl.StartsWith("/friends"))
			{
				wantsFriends = true;
			}
			else if (subUrl.StartsWith("/blocklist"))
			{
				wantsFriends = false;
			}

			string[] segments = req.Url.Segments;
			if (segments.Length == 6)
			{
				string accountID = segments[5];
				bool includePending = HttpUtility.ParseQueryString(req.Url.Query)["includePending"] == "true";
				if (wantsFriends)
				{
					JArray arr = new JArray();
					// foreach {
					JObject obj = new JObject();
					obj.Add("accountId", "09b8744abd524d879630f7c79365e2f8");
					obj.Add("status", "ACCEPTED"); // or "PENDING"
					obj.Add("direction", "INBOUND");
					obj.Add("created", "2018-09-05T14:55:04.261Z");
					obj.Add("favourite", false);
					arr.Add(obj);
					// }
					resp.StatusCode = 200;
					output.Write(arr.ToString());
					return;
				}
				else
				{
					JObject obj = new JObject();
					JArray arr = new JArray();
					// foreach {
					arr.Add("4ab7f6139b35468080f8a7b8bb4334d5");
					// }
					obj.Add("blockedUsers", arr);

					resp.StatusCode = 200;
					output.Write(obj.ToString());
					return;
				}
			}
		}

		private void HandleUTQueryProfile(HttpListenerRequest req, StreamReader input, HttpListenerResponse resp, StreamWriter output)
		{
			// URL: POST /ut/api/game/v2/profile/0b0f09b400854b9b98932dd9e5abe7c5/client/QueryProfile?profileId=profile0&rvn=-1
			// INPUT: {}
			// OUTPUT: {"profileRevision":7152,"profileId":"profile0","profileChangesBaseRevision":7152,"profileChanges":[{"changeType":"fullProfileUpdate","profile":{"_id":"45670543f0f24b47981c7bf38fb3ccbf","created":"2016-05-06T14:30:03.505Z","updated":"2022-12-05T17:45:56.027Z","rvn":7152,"wipeNumber":4,"accountId":"0b0f09b400854b9b98932dd9e5abe7c5","profileId":"profile0","version":"ut_base","items":{"e97df1f3-f7b6-41c2-a941-d3608df9b150":{"templateId":"Item.NecrisHelm01","attributes":{"tradable":true},"quantity":1},"ebd4482c-3259-4c23-bf7f-7b94c7bde70f":{"templateId":"Item.BeanieGrey","attributes":{"tradable":true},"quantity":1},"014386c5-a6b0-4e7c-81c1-e32a3ab7ce8e":{"templateId":"Item.ThundercrashMale05","attributes":{"tradable":false},"quantity":1},"60d63110-e262-4ce9-9381-ee435c0daf07":{"templateId":"Item.ThundercrashBeanieGreen","attributes":{"tradable":true},"quantity":1},"bca6407a-bd01-4586-9614-2c86f8320a1f":{"templateId":"Item.BeanieBlack","attributes":{"tradable":true},"quantity":1},"0a75d9bf-995d-4453-9aed-0ef390751d98":{"templateId":"Item.NecrisHelm02","attributes":{"tradable":true},"quantity":1},"38375537-1b83-4cec-9e5d-4cdcb9f1f824":{"templateId":"Item.NecrisMale04","attributes":{"tradable":false},"quantity":1},"b74f18d3-6a9e-47e2-8ef3-4922998addd8":{"templateId":"Item.ThundercrashBeret","attributes":{"tradable":true},"quantity":1},"7259ce8f-eb63-4751-94af-30bf1ac32a08":{"templateId":"Item.HockeyMask","attributes":{"tradable":true},"quantity":1},"dfd4382d-ac2b-4a45-95fe-354679ccfcb6":{"templateId":"Item.NecrisMale01","attributes":{"tradable":false},"quantity":1},"31ff0e28-db64-47b0-a7a5-33598fd73531":{"templateId":"Item.NecrisFemale02","attributes":{"tradable":false},"quantity":1},"f2ac1a98-d3d0-4253-855b-9f52466fbe00":{"templateId":"Item.HockeyMask02","attributes":{"tradable":true},"quantity":1},"430658de-b23d-470d-9505-79fa6bc7626e":{"templateId":"Item.BeanieWhite","attributes":{"tradable":true},"quantity":1},"44adb6f3-1328-488c-b0df-f43fcdbd6445":{"templateId":"Item.ThundercrashBeanieRed","attributes":{"tradable":true},"quantity":1},"1d8c1110-9606-4302-a22c-e516505f6d58":{"templateId":"Item.SkaarjMale02","attributes":{"tradable":false},"quantity":1},"15b19bf7-0b9f-49c3-98d7-bb1103838ddc":{"templateId":"Item.Sunglasses","attributes":{"tradable":true},"quantity":1},"1bbd85ba-0b45-4b1a-9a37-a285d9639a2a":{"templateId":"Item.ThundercrashMale03","attributes":{"tradable":false},"quantity":1},"198bc8ca-1746-430c-8a97-792dbbe5d343":{"templateId":"Item.SkaarjMale01","attributes":{"tradable":false},"quantity":1},"0c465e5c-ea64-4569-a6b3-02e2dbe0f5db":{"templateId":"Item.ThundercrashMale02","attributes":{"tradable":false},"quantity":1}},"stats":{"templateId":"profile_v2","attributes":{"CountryFlag":"Slovenia","GoldStars":35,"login_rewards":{"nextClaimTime":null,"level":0,"totalDays":0},"Avatar":"UT.Avatar.1","inventory_limit_bonus":0,"daily_purchases":{},"in_app_purchases":{},"LastXPTime":1670262356,"XP":790988,"Level":50,"BlueStars":3,"RecentXP":105,"boosts":[],"new_items":{}}},"commandRevision":7043}}],"profileCommandRevision":7043,"serverTime":"2022-12-14T20:28:29.359Z","responseVersion":1,"command":"QueryProfile"}

			string[] segments = req.Url.Segments;
			if (segments.Length >= 7)
			{
				string accountID = segments[6];
				accountID = accountID.Substring(0, accountID.Length - 1);
				User? user = users.GetUserByID(new CommonID(accountID));
				if (user == null)
					return;
				// TODO: extra checks to make sure QueryProfile request was made

				JObject obj = new JObject();
				obj.Add("profileRevision", 7152);
				obj.Add("profileId", "profile0");
				obj.Add("profileChangesBaseRevision", 7152);
				JArray profileChanges = new JArray();
				// foreach {
				JObject profileChange = new JObject();
				profileChange.Add("changeType", "fullProfileUpdate");
				JObject profile = new JObject();
				profile.Add("_id", "45670543f0f24b47981c7bf38fb3ccbf");
				profile.Add("created", "2016-05-06T14:30:03.505Z");
				profile.Add("updated", "2022-12-05T17:45:56.027Z");
				profile.Add("rvn", 7152);
				profile.Add("wipeNumber", 4);
				profile.Add("accountId", user.ID.ToString());
				profile.Add("profileId", "profile0");
				profile.Add("version", "ut_base");
				JObject items = new JObject();
				// TODO !!!
				profile.Add("items", items);
				JObject stats = new JObject();
				stats.Add("CountryFlag", "Slovenia");
				stats.Add("GoldStars", 35);
				JObject login_rewards = new JObject();
				login_rewards.Add("nextClaimTime", null);
				login_rewards.Add("level", 0);
				login_rewards.Add("totalDays", 0);
				stats.Add("login_rewards", login_rewards);
				stats.Add("Avatar", "UT.Avatar.1");
				stats.Add("inventory_limit_bonus", 0);
				stats.Add("daily_purchases", new JObject());
				stats.Add("in_app_purchases", new JObject());
				stats.Add("LastXPTime", 1670262356); // is this a unix timestamp?
				stats.Add("XP", 790988);
				stats.Add("Level", "50"); // :o we can start bumping this number up
				stats.Add("BlueStars", 3);
				stats.Add("RecentXP", "105"); // probably xp from last match
				stats.Add("boosts", new JArray());
				stats.Add("new_items", new JObject());
				profile.Add("stats", stats);
				profileChange.Add("profile", profile);
				profileChange.Add("commandRevision", 7043);
				// }
				profileChanges.Add(profileChange);
				obj.Add("profileChanges", profileChanges);
				obj.Add("profileCommandRevision", 7043);
				obj.Add("serverTime", "2022-12-14T20:28:29.359Z");
				obj.Add("responseVersion", 1);
				obj.Add("command", "QueryProfile");


				resp.StatusCode = 200;
				output.Write(obj.ToString(Newtonsoft.Json.Formatting.None));
				return;
			}
		}

		private void HandleUTGameRatings(HttpListenerRequest req, StreamReader input, HttpListenerResponse resp, StreamWriter output)
		{
			var segments = req.Url.Segments;

			string accountID;
			if (segments.Length >= 8)
			{
				accountID = segments[7];
				accountID = accountID.Substring(0, accountID.Length - 1);
			}



			// URL: POST /ut/api/game/v2/ratings/account/0b0f09b400854b9b98932dd9e5abe7c5/mmrbulk
			// INPUT: {"ratingTypes": ["SkillRating","TDMSkillRating","DMSkillRating","CTFSkillRating","ShowdownSkillRating","FlagRunSkillRating","RankedDuelSkillRating","RankedCTFSkillRating","RankedShowdownSkillRating","RankedFlagRunSkillRating"]}
			// OUTPUT: {"ratingTypes":["SkillRating","TDMSkillRating","DMSkillRating","CTFSkillRating","ShowdownSkillRating","FlagRunSkillRating","RankedDuelSkillRating","RankedCTFSkillRating","RankedShowdownSkillRating","RankedFlagRunSkillRating"],"ratings":[1655,1862,2038,1827,1626,2630,1666,1500,1500,1500],"numGamesPlayed":[546,1029,2073,181,17,2264,29,0,0,0]}

			if (segments.Length == 9)
			{
				if (segments[8] == "mmrbulk")
				{
					// TODO: parse input

					JObject obj = new JObject();
					JArray ratingTypes = new JArray();
					ratingTypes.Add("SkillRating");
					ratingTypes.Add("TDMSkillRating");
					ratingTypes.Add("DMSkillRating");
					ratingTypes.Add("CTFSkillRating");
					ratingTypes.Add("ShowdownSkillRating");
					ratingTypes.Add("FlagRunSkillRating");
					ratingTypes.Add("RankedDuelSkillRating");
					ratingTypes.Add("RankedDuelSkillRating");
					ratingTypes.Add("RankedCTFSkillRating");
					ratingTypes.Add("RankedShowdownSkillRating");
					ratingTypes.Add("RankedFlagRunSkillRating");
					obj.Add("ratingTypes", ratingTypes);

					JArray ratings = new JArray();
					ratings.Add(0);
					ratings.Add(0);
					ratings.Add(0);
					ratings.Add(0);
					ratings.Add(0);
					ratings.Add(0);
					ratings.Add(0);
					ratings.Add(0);
					ratings.Add(0);
					ratings.Add(0);
					ratings.Add(0);
					obj.Add("ratings", ratings);

					JArray numGamesPlayed = new JArray();
					numGamesPlayed.Add(0);
					numGamesPlayed.Add(0);
					numGamesPlayed.Add(0);
					numGamesPlayed.Add(0);
					numGamesPlayed.Add(0);
					numGamesPlayed.Add(0);
					numGamesPlayed.Add(0);
					numGamesPlayed.Add(0);
					numGamesPlayed.Add(0);
					numGamesPlayed.Add(0);
					numGamesPlayed.Add(0);
					obj.Add("numGamesPlayed", numGamesPlayed);

					resp.StatusCode = 200;
					output.Write(obj.ToString(Newtonsoft.Json.Formatting.None));
					return;
				}
			}

			// URL: GET /ut/api/game/v2/ratings/account/0b0f09b400854b9b98932dd9e5abe7c5/league/RankedFlagRunSkillRating
			// OUTPUT: {"tier":0,"division":0,"points":0,"isInPromotionSeries":false,"promotionMatchesAttempted":0,"promotionMatchesWon":0,"placementMatchesAttempted":0}

			// URL: GET /ut/api/game/v2/ratings/account/0b0f09b400854b9b98932dd9e5abe7c5/league/RankedShowdownSkillRating
			// OUTPUT: {"tier":0,"division":0,"points":0,"isInPromotionSeries":false,"promotionMatchesAttempted":0,"promotionMatchesWon":0,"placementMatchesAttempted":0}

			// URL: GET /ut/api/game/v2/ratings/account/0b0f09b400854b9b98932dd9e5abe7c5/league/RankedCTFSkillRating
			// OUTPUT: {"tier":0,"division":0,"points":0,"isInPromotionSeries":false,"promotionMatchesAttempted":0,"promotionMatchesWon":0,"placementMatchesAttempted":0}

			// URL: GET /ut/api/game/v2/ratings/account/0b0f09b400854b9b98932dd9e5abe7c5/league/RankedDuelSkillRating
			// OUTPUT: {"tier":2,"division":0,"points":13,"isInPromotionSeries":false,"promotionMatchesAttempted":3,"promotionMatchesWon":3,"placementMatchesAttempted":10}

			if (segments.Length == 10)
			{
				JObject obj = new JObject();
				obj.Add("tier", 2);
				obj.Add("division", 0);
				obj.Add("points", 13);
				obj.Add("isInPromotionSeries", false);
				obj.Add("promotionMatchesAttempted", 3);
				obj.Add("promotionMatchesWon", 3);
				obj.Add("placementMatchesAttempted", 10);

				resp.StatusCode = 200;
				output.Write(obj.ToString());
				return;
			}
		}

		private void HandleUTCloudStorage(HttpListenerRequest req, StreamReader input, HttpListenerResponse resp, StreamWriter output)
		{
			var segments = req.Url.Segments;
			// URL: GET /ut/api/cloudstorage/user/0b0f09b400854b9b98932dd9e5abe7c5
			// OUTPUT: [{"uniqueFilename":"user_progression_1","filename":"user_progression_1","hash":"2e2d2c3d3e65bc2156edea5324ba2d28d4e2ced2","hash256":"2576474a5c0a77452e4e3d480f01203963ba639384723e5d4f07c172b6e50b42","length":4946,"contentType":"text/plain","uploaded":"2021-02-09T17:22:51.311Z","storageType":"S3","accountId":"0b0f09b400854b9b98932dd9e5abe7c5"},{"uniqueFilename":"stats.json","filename":"stats.json","hash":"57181e2e97dc09fd7f2b58dd3debff89955fa5c3","hash256":"fcc870dce478f07340e0c97d2417b056b17c816196939b138a5ae9c06b1bdc2f","length":5406,"contentType":"text/plain","uploaded":"2022-12-05T17:45:56.222Z","storageType":"S3","accountId":"0b0f09b400854b9b98932dd9e5abe7c5"},{"uniqueFilename":"user_profile_2","filename":"user_profile_2","hash":"1b420f535b762e866b55767945aea60c289a6101","hash256":"8af53380c8e233ce7b3a136c26b9bf7128cd4344418d98d2a1164dee37af872b","length":98615,"contentType":"text/plain","uploaded":"2022-11-20T11:46:52.431Z","storageType":"S3","accountId":"0b0f09b400854b9b98932dd9e5abe7c5"}]

			if (segments.Length == 6)
			{
				var accountID = segments[5];
				accountID = accountID.Substring(0, accountID.Length - 1);
				JArray arr = new JArray();
				// foreach {
				{
					JObject obj = new JObject();
					obj.Add("uniqueFilename", "user_progression_1");
					obj.Add("filename", "user_progression_1");
					obj.Add("hash", "2e2d2c3d3e65bc2156edea5324ba2d28d4e2ced2");
					obj.Add("hash256", "2576474a5c0a77452e4e3d480f01203963ba639384723e5d4f07c172b6e50b42");
					obj.Add("length", 4946);
					obj.Add("contentType", "text/plain");
					obj.Add("uploaded", "2021-02-09T17:22:51.311Z");
					obj.Add("storageType", "S3");
					obj.Add("accountId", accountID);
					arr.Add(obj);
				}
				{
					JObject obj = new JObject();
					obj.Add("uniqueFilename", "user_profile_2");
					obj.Add("filename", "user_profile_2");
					obj.Add("hash", "1b420f535b762e866b55767945aea60c289a6101");
					obj.Add("hash256", "8af53380c8e233ce7b3a136c26b9bf7128cd4344418d98d2a1164dee37af872b");
					obj.Add("length", 98615);
					obj.Add("contentType", "text/plain");
					obj.Add("uploaded", "2022-11-20T11:46:52.431Z");
					obj.Add("storageType", "S3");
					obj.Add("accountId", accountID);
					arr.Add(obj);
				}
				//}
				resp.StatusCode = 200;
				output.Write(arr.ToString());
				return;
			}

			// URL: GET /ut/api/cloudstorage/user/0b0f09b400854b9b98932dd9e5abe7c5/user_profile_2
			// URL: GET /ut/api/cloudstorage/user/0b0f09b400854b9b98932dd9e5abe7c5/user_progression_1
			// OUTPUT: <<actual file content of specified file>>

			if (segments.Length == 7)
			{
				var accountID = segments[5];
				var filename = segments[6];

				resp.StatusCode = 200;
				byte[] content = File.ReadAllBytes("Cloud/User/" + filename); // TODO: this is insecure
				output.BaseStream.Write(content);
				return;
			}

			// URL: GET /ut/api/cloudstorage/system
			// OUTPUT: [{"uniqueFilename":"UnrealTournmentMCPGameRulesets.json","filename":"UnrealTournmentMCPGameRulesets.json","hash":"04d909734ebb0bce8010a81a7b3828212aa518ba","hash256":"ad08c0a3e626fd0e33710e3d84c73d7de9465e6807b10289516f59ff1079629b","length":1736,"contentType":"text/plain","uploaded":"2017-05-18T19:34:52.579Z","storageType":"S3","doNotCache":false},{"uniqueFilename":"UnrealTournamentPlaylists.json","filename":"UnrealTournamentPlaylists.json","hash":"625670ef1938d6e3b154a99a95e94762ebed6ccc","hash256":"bd765ce4dd05c1e8daf3fa939562108f1546812488296e1709c659ace20a1272","length":1327,"contentType":"text/plain","uploaded":"2017-04-17T17:59:32.698Z","storageType":"S3","doNotCache":false},{"uniqueFilename":"UTMCPPlaylists.json","filename":"UTMCPPlaylists.json","hash":"1ee9753108b6d89dfc3c157f49531c67cb559217","hash256":"bbc3d03d0a99289a66475313281dfb972933ef43d136cefe52017f43f458bc08","length":2097,"contentType":"text/plain","uploaded":"2017-06-19T19:46:37.476Z","storageType":"S3","doNotCache":false},{"uniqueFilename":"UnrealTournamentRankedPlay.json","filename":"UnrealTournamentRankedPlay.json","hash":"68d6b0188f4cc80182e4b4307a7e3d8326c5b912","hash256":"940a8f2d3c416bdb8d135c8a485b2a4fc7a923a17f565950003f5a61b728f8cb","length":41,"contentType":"text/plain","uploaded":"2017-02-13T22:50:32.644Z","storageType":"S3","doNotCache":false},{"uniqueFilename":"UnrealTournmentMCPStorage.json","filename":"UnrealTournmentMCPStorage.json","hash":"97d00d83589bfcdf3f0994c401b40c0f56842a75","hash256":"8a7e492baab389690efd44f5cc9ac29aab0f2f98e5b589db5561a43d7b313a1a","length":3135,"contentType":"text/plain","uploaded":"2017-07-12T14:47:49.270Z","storageType":"S3","doNotCache":false},{"uniqueFilename":"UnrealTournmentMCPAnnouncement.json","filename":"UnrealTournmentMCPAnnouncement.json","hash":"28b24fff42790e74506690606f1551e421a4084c","hash256":"23657300e828ffcac2a6bc88dca1cb5ea6162350fefbc2c38f184bc359ca2b2a","length":503,"contentType":"text/plain","uploaded":"2017-03-16T19:53:49.713Z","storageType":"S3","doNotCache":false},{"uniqueFilename":"UnrealTournamentOnlineSettings.json","filename":"UnrealTournamentOnlineSettings.json","hash":"47cd9c3e37c6a1f0cf34cee62a1ca6568f5cf8c1","hash256":"904f88619ceee94b28e0fff2b09096d65b7a2fac4ff9cf5141815956d3122aa3","length":226,"contentType":"text/plain","uploaded":"2017-02-22T18:48:57.165Z","storageType":"S3","doNotCache":false}]
			if (segments.Length == 5)
			{
				string staticReturn = @"[{""uniqueFilename"":""UnrealTournmentMCPGameRulesets.json"",""filename"":""UnrealTournmentMCPGameRulesets.json"",""hash"":""04d909734ebb0bce8010a81a7b3828212aa518ba"",""hash256"":""ad08c0a3e626fd0e33710e3d84c73d7de9465e6807b10289516f59ff1079629b"",""length"":1736,""contentType"":""text/plain"",""uploaded"":""2017-05-18T19:34:52.579Z"",""storageType"":""S3"",""doNotCache"":false},{""uniqueFilename"":""UnrealTournamentPlaylists.json"",""filename"":""UnrealTournamentPlaylists.json"",""hash"":""625670ef1938d6e3b154a99a95e94762ebed6ccc"",""hash256"":""bd765ce4dd05c1e8daf3fa939562108f1546812488296e1709c659ace20a1272"",""length"":1327,""contentType"":""text/plain"",""uploaded"":""2017-04-17T17:59:32.698Z"",""storageType"":""S3"",""doNotCache"":false},{""uniqueFilename"":""UTMCPPlaylists.json"",""filename"":""UTMCPPlaylists.json"",""hash"":""1ee9753108b6d89dfc3c157f49531c67cb559217"",""hash256"":""bbc3d03d0a99289a66475313281dfb972933ef43d136cefe52017f43f458bc08"",""length"":2097,""contentType"":""text/plain"",""uploaded"":""2017-06-19T19:46:37.476Z"",""storageType"":""S3"",""doNotCache"":false},{""uniqueFilename"":""UnrealTournamentRankedPlay.json"",""filename"":""UnrealTournamentRankedPlay.json"",""hash"":""68d6b0188f4cc80182e4b4307a7e3d8326c5b912"",""hash256"":""940a8f2d3c416bdb8d135c8a485b2a4fc7a923a17f565950003f5a61b728f8cb"",""length"":41,""contentType"":""text/plain"",""uploaded"":""2017-02-13T22:50:32.644Z"",""storageType"":""S3"",""doNotCache"":false},{""uniqueFilename"":""UnrealTournmentMCPStorage.json"",""filename"":""UnrealTournmentMCPStorage.json"",""hash"":""97d00d83589bfcdf3f0994c401b40c0f56842a75"",""hash256"":""8a7e492baab389690efd44f5cc9ac29aab0f2f98e5b589db5561a43d7b313a1a"",""length"":3135,""contentType"":""text/plain"",""uploaded"":""2017-07-12T14:47:49.270Z"",""storageType"":""S3"",""doNotCache"":false},{""uniqueFilename"":""UnrealTournmentMCPAnnouncement.json"",""filename"":""UnrealTournmentMCPAnnouncement.json"",""hash"":""28b24fff42790e74506690606f1551e421a4084c"",""hash256"":""23657300e828ffcac2a6bc88dca1cb5ea6162350fefbc2c38f184bc359ca2b2a"",""length"":503,""contentType"":""text/plain"",""uploaded"":""2017-03-16T19:53:49.713Z"",""storageType"":""S3"",""doNotCache"":false},{""uniqueFilename"":""UnrealTournamentOnlineSettings.json"",""filename"":""UnrealTournamentOnlineSettings.json"",""hash"":""47cd9c3e37c6a1f0cf34cee62a1ca6568f5cf8c1"",""hash256"":""904f88619ceee94b28e0fff2b09096d65b7a2fac4ff9cf5141815956d3122aa3"",""length"":226,""contentType"":""text/plain"",""uploaded"":""2017-02-22T18:48:57.165Z"",""storageType"":""S3"",""doNotCache"":false}]";

				resp.StatusCode = 200;
				output.Write(staticReturn);
				return;
			}

		}


		private void HandleUTMatchmaking(HttpListenerRequest req, StreamReader input, HttpListenerResponse resp, StreamWriter output)
		{
			// USE: handles listing hubs
			// URL: POST /ut/api/matchmaking/session/matchMakingRequest
			// INPUT:
			/*
			
			{
				"criteria": [
					{
						"type": "NOT_EQUAL",
						"key": "UT_GAMEINSTANCE_i",
						"value": 1
					},
					{
						"type": "NOT_EQUAL",
						"key": "UT_RANKED_i",
						"value": 1
					}
				],
				"buildUniqueId": "256652735",
				"maxResults": 10000
			}

			*/

			// OUTPUT: <<too big, its in HubMenuResponse.json file>>
		}


		private void HandleOtherMisc(HttpListenerRequest req, StreamReader input, HttpListenerResponse resp, StreamWriter output)
		{
			// URL: cdn1.unrealengine.com/launcher-resources/0.1_b76b28ed708e4efcbb6d0e843fcc6456/launcher/icon.png
			// URL: cdn1.unrealengine.com/launcher-resources/0.1_b76b28ed708e4efcbb6d0e843fcc6456/default/icon.png
			// OUTPUT: a file... duhhh


			//context.Response.StatusCode = 200;
			//	using (var writer = new StreamWriter(output, Encoding.UTF8))
			//	{
			//		writer.WriteLine(@$"{{ ""msg"":"":Welcome to {context.Request.RawUrl}""}}");
			//	}

		}


		
	}
}

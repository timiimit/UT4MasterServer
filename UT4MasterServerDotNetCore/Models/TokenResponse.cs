namespace UT4MasterServer.Models
{
	// TODO: This is a temp static class until we get a real auth/token exchange server set up
	public class TokenResponse
	{
		public string access_token { get; set; } = "187deaf65f3a46d180ff01317f7fb4ed";
		public int expires_in { get; set; } = 28800;
		public string expires_at { get; set; } = "2022-12-30t11:42:30.271z";
		public string refresh_token { get; set; } = "58132fae31f0414383d04506148cf42a";
		public int refresh_expires { get; set; } = 86400;
		public string refresh_expires_at { get; set; } = "2022-12-31t03:42:30.219z";
		public string account_id { get; set; } = "CHANGE ME TO AN ID!!!";
		public string client_id { get; set; } = "1252412dc7704a9690f6ea4611bc81ee";
		public bool internal_client { get; set; } = false;
		public string client_service { get; set; } = "ut";
		public string display_name { get; set; } = "dc!";
		public string app { get; set; } = "ut";
		public string in_app_id { get; set; } = "CHANGE ME TO AN ID!!!";
		public string device_id { get; set; } = "58bf9d89433df78f9cb79ab6d7e78886";
    }
}

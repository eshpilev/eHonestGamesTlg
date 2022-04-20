namespace eHonestGamesTlg
{
	internal static class Setting
	{
		public static string Token { get; private set; } = "TOKEN";
		public static string PhoneNumber { get; private set; } = "+79998889999";
		public static string FirstName { get; private set; } = "NAME";
		public static string LastName { get; private set; } = "LASTNAME";
		public static string URL { get; private set; } = "https://google.ru";

		public static int FODMaxTries { get; private set; } = 5;
		public static int GNMaxTries { get; private set; } = 6;
		public static int GNMaxNumber { get; private set; } = 100;
	}
}

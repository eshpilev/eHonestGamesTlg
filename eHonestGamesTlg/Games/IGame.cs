namespace eHonestGamesTlg.Games
{
	public interface IGame
	{
		public delegate Task<string> SendAndWaitMessage(string label);
		public delegate Task SendResult(string result);
		public event SendAndWaitMessage? NotifySendAndWaitMessage;
		public event SendResult? NotifySendResult;

		GameStatus Status { get; set; }

		void StartAsync();
		string rulesInfo();
	}
}

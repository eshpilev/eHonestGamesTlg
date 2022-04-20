using eHonestGamesTlg.Games;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace eHonestGamesTlg
{
	internal class GameHandler
	{
		public event Action<long>? EndGame;

		private readonly ITelegramBotClient botClient;
		private readonly CancellationToken cancellationToken;	
		private readonly IGame game;
		private readonly long chatId;
		private string response = "";

		AutoResetEvent sync = new AutoResetEvent(false); 

		private GameHandler(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken, IGame game)
		{		
			this.botClient = botClient;		
			this.cancellationToken = cancellationToken;
			this.chatId = chatId;
			this.game = game;			
		}

		public static GameHandler GameFieldOfDreams(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
		{
			return new GameHandler(botClient, chatId, cancellationToken, new FieldOfDreams());			
		}

		public static GameHandler GameGuessNumber(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
		{
			return new GameHandler(botClient, chatId, cancellationToken, new GuessNumber());
		}

		public void RunGame()
		{
			if (!this.HasActiveGame())
			{
				game.NotifySendResult += SendResult;
				game.NotifySendAndWaitMessage += SendAndWaitMessage;
				game.StartAsync();
			}
		}

		public void Response(string response)
		{			
			this.response = response;
			sync.Set();
		}

		public async void CancelGame()
		{
			if (this.HasActiveGame())
			{
				game.Status = GameStatus.Interrupted;
				sync.Set();

				await this.SendResult("Игра завершена");
			}
			else
			{
				await this.SendResult("Игра отменена");
			}						
		}

		public bool HasActiveGame()
		{
			switch (game.Status)
			{			
				case GameStatus.Started:
					return true;

				default:
					return false;
			}
		}

		public async void Preview()
		{
			InlineKeyboardMarkup inlineKeyboard = new(new[]
			{
				new []
				{
					InlineKeyboardButton.WithCallbackData(text: "Старт", callbackData: "/game_start"),
					InlineKeyboardButton.WithCallbackData(text: "Отмена", callbackData: "/game_cancel"),
				},
			});

			await botClient.SendTextMessageAsync(
				chatId: chatId,
				text: $" <strong>Правила игры:</strong> {game.rulesInfo()}",
				parseMode: ParseMode.Html,
				replyMarkup: inlineKeyboard,
				cancellationToken: cancellationToken);
		}

		public async Task SendResult(string result)
		{	
			 await botClient.SendTextMessageAsync(
				chatId: chatId,
				text: result,
				replyMarkup: MenuHandler.inlineKeyboardMenuGames(),
				cancellationToken: cancellationToken);

			EndGame?.Invoke(chatId);
		}

		public async Task<string> SendAndWaitMessage(string label)
		{
			await botClient.SendTextMessageAsync(
				chatId: chatId,
				text: label,
				cancellationToken: cancellationToken);

			sync.WaitOne();
			return response;
		}
	}
}

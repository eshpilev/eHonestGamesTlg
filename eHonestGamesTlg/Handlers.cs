using eHonestGamesTlg;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;


namespace Telegram.Bot.Examples.Polling
{
	public class Handlers
	{		
		private Dictionary<long, GameHandler> cachedChatGame;	

		public Handlers()
		{
			cachedChatGame = new Dictionary<long, GameHandler>();
		}		

		public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
		{
			var ErrorMessage = exception switch
			{
				ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
				_ => exception.ToString()
			};

			Console.WriteLine(ErrorMessage);
			return Task.CompletedTask;			
		}

		public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
		{			
			var handler = update.Type switch
			{				
				UpdateType.Message => BotOnMessageReceived(botClient, update.Message!, cancellationToken),
				UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!, cancellationToken),
				UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!, cancellationToken),				
				_ => UnknownUpdateHandlerAsync(botClient, update)
			};

			try
			{				
				await handler;				
			}
			catch (Exception exception)
			{
				await HandleErrorAsync(botClient, exception, cancellationToken);
			}
		}

		private async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
		{
			if (message?.Type != MessageType.Text)
				return;

			long chatId = message.Chat.Id;
			string? text = message?.Text?.ToLower().Trim();
			GameHandler? gameHandler = GetCacheGameHandler(chatId);
			bool hasActiveGame = gameHandler?.HasActiveGame() ?? false;

			switch (text)
			{
				case "/game_cancel":
					gameHandler?.CancelGame();
					break;

				case "/start":
					if (!hasActiveGame)
						await MenuHandler.MainMenu(botClient, message.Chat.Id, cancellationToken);
					break;

				case "/games":
					if (!hasActiveGame)
						await MenuHandler.GamesMenu(botClient, message.Chat.Id, cancellationToken);
					break;

				default:
					if (hasActiveGame)
						gameHandler?.Response(message?.Text!);
					break;
			}
		}

		private async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
		{
			long chatId = callbackQuery.Message.Chat.Id;			
			GameHandler? gameHandler = GetCacheGameHandler(chatId);
			bool hasActiveGame = gameHandler?.HasActiveGame() ?? false;

			if (callbackQuery?.Data == "/game_cancel")
				gameHandler?.CancelGame();
			
			if (!hasActiveGame)
			{
				switch (callbackQuery?.Data)
				{
					case "/start":
						await MenuHandler.MainMenu(botClient, chatId, cancellationToken);
						break;

					case "/contact":
						await MenuHandler.Contact(botClient, chatId, cancellationToken);
						break;						

					case "/games":
						await MenuHandler.GamesMenu(botClient, callbackQuery.Message.Chat.Id, cancellationToken);
						break;

					case "/game_fieldOfDreams":
						var gameFieldOfDreams = GameHandler.GameFieldOfDreams(botClient, chatId, cancellationToken);
						gameFieldOfDreams.Preview();
						gameFieldOfDreams.EndGame += EndGame;
						AddCacheGameHandler(chatId, gameFieldOfDreams);
						break;

					case "/game_guessNumber":
						var gameGuessNumber = GameHandler.GameGuessNumber(botClient, chatId, cancellationToken);
						gameGuessNumber.Preview();
						gameGuessNumber.EndGame += EndGame;
						AddCacheGameHandler(chatId, gameGuessNumber);
						break;

					case "/game_start":
						gameHandler?.RunGame();
						break;
				}								
			}			
		}

		private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
		{
			Console.WriteLine($"Unknown update type: {update.Type}");
			return Task.CompletedTask;
		}		

		private void EndGame(long chatId)
		{			
			cachedChatGame.Remove(chatId);
		}		

		private GameHandler? GetCacheGameHandler(long chatId)
		{
			return cachedChatGame.ContainsKey(chatId) ? cachedChatGame[chatId] : null;
		}

		private void AddCacheGameHandler(long chatId, GameHandler gameHandler)
		{
			if (!cachedChatGame.ContainsKey(chatId))
				cachedChatGame.Add(chatId, gameHandler);			
		}
	}
}
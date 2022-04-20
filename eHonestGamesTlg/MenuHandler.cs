using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace eHonestGamesTlg
{
	public static class MenuHandler
	{
		public async static Task MainMenu(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
		{
			InlineKeyboardMarkup inlineKeyboard = new(new[]
			{
				new []
				{
					InlineKeyboardButton.WithCallbackData(text: "Выбрать игру", callbackData: "/games"),
					InlineKeyboardButton.WithCallbackData(text: "Написать автору", callbackData: "/contact"),
				},
				new []
				{
					InlineKeyboardButton.WithSwitchInlineQuery(text: "Поделиться ботом"),
					InlineKeyboardButton.WithUrl(text: "Об авторе", url: Setting.URL),
				},
			});

			await botClient.SendTextMessageAsync(
				chatId: chatId,
				text: "Главное меню",
				replyMarkup: inlineKeyboard,
				cancellationToken: cancellationToken);
		}

		public async static Task GamesMenu(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
		{			

			await botClient.SendTextMessageAsync(
				chatId: chatId,
				text: "Выберите игру",
				replyMarkup: MenuHandler.inlineKeyboardMenuGames(),
				cancellationToken: cancellationToken);
		}

		public async static Task Contact(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
		{
			await botClient.SendContactAsync(
				chatId: chatId,
				phoneNumber: Setting.PhoneNumber,
				firstName: Setting.FirstName,
				lastName: Setting.LastName,
				cancellationToken: cancellationToken);
		}

		public static InlineKeyboardMarkup inlineKeyboardMenuGames()
		{
			return new(new[]
			{
				new []
				{
					InlineKeyboardButton.WithCallbackData(text: "Поле чудес", callbackData: "/game_fieldOfDreams"),
					InlineKeyboardButton.WithCallbackData(text: "Угадай число", callbackData: "/game_guessNumber"),
				},
				new []
				{
					InlineKeyboardButton.WithCallbackData(text: "Главное меню",  callbackData: "/start"),
				},
			});
		}
	}
}

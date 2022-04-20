using eHonestGamesTlg; 
using Telegram.Bot;
using Telegram.Bot.Examples.Polling;
using Telegram.Bot.Extensions.Polling;


var botClient = new TelegramBotClient(Setting.Token);

using var cts = new CancellationTokenSource();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = { } // receive all update types
};

var Handlers = new Handlers();

botClient.StartReceiving(
    Handlers.HandleUpdateAsync,
    Handlers.HandleErrorAsync,
    receiverOptions,
    cancellationToken: cts.Token);


Console.WriteLine($"Запущен бот {botClient.GetMeAsync().Result.FirstName}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();
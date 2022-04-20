namespace eHonestGamesTlg.Games 
{
	public class GuessNumber : IGame
	{
		public event IGame.SendAndWaitMessage? NotifySendAndWaitMessage;
		public event IGame.SendResult? NotifySendResult;
		
		private int maxTries;
		private int maxNumber;		
		public GameStatus Status { get; set; }

		public GuessNumber()
		{
			this.Status = GameStatus.NonStarted;			
		}

		public async void StartAsync()
		{
			try
			{
				this.Status = GameStatus.Started;

				await InitParametersAsync();
				await RunAsync();				
			}
			catch (Exception ex)
			{
				if (NotifySendResult != null)
					await NotifySendResult(ex.Message);
			}
		}

		private async Task InitParametersAsync()
		{
			var tokenSource = new CancellationTokenSource();
			var token = tokenSource.Token;

			string response = string.Empty;

			if (NotifySendAndWaitMessage != null)
				response = await NotifySendAndWaitMessage($"Введите максимальное количество попыток (по умолчанию {Setting.GNMaxTries}):");

			if (this.Status == GameStatus.Interrupted)			
				return;			

			int.TryParse(response, out this.maxTries);

			if (this.maxTries == 0)
				this.maxTries = Setting.GNMaxTries;

			response = string.Empty;

			if (NotifySendAndWaitMessage != null)
				response = await NotifySendAndWaitMessage($"Введите максимальное число (по умолчанию {Setting.GNMaxNumber}):");

			if (this.Status == GameStatus.Interrupted)
				return;

			int.TryParse(response, out this.maxNumber);

			if (this.maxNumber == 0)
				this.maxNumber = Setting.GNMaxNumber;			
		}

		private async Task RunAsync()
		{
			int guessesNumber = new Random().Next(this.maxNumber);
			int remainTries = this.maxTries;
			string resultTries = "";
			int number = 0;

		
			while (this.Status == GameStatus.Started)
			{
				string response = string.Empty;				

				if (NotifySendAndWaitMessage != null)
					response = await NotifySendAndWaitMessage($"{resultTries} Оставшееся число попыток: {remainTries}. Введите число:");

				if (this.Status == GameStatus.Interrupted)
				{
					break;
				}

				int.TryParse(response, out number);

				while (number < 0 || number > this.maxNumber)
				{
					if (NotifySendAndWaitMessage != null)
						response = await NotifySendAndWaitMessage($"Введеное число должно быть в диапозоне от 0 до {this.maxNumber}. Повторите попытку");
				
					int.TryParse(response, out number);
				}

				remainTries--;
				if (number == guessesNumber)
				{
					this.Status = GameStatus.Won;
					break;
				}
				else if (number < guessesNumber)
				{
					resultTries = "Введеное число меньше загаданного! ";				
				}
				else
				{
					resultTries = "Введеное число больше загаданного! ";					
				}

				if (remainTries <= 0)
				{
					this.Status = GameStatus.Lost;
				}				
			}

			await ResultAsync(guessesNumber);
		}

		private async Task ResultAsync(int guessesNumber)
		{
			switch (this.Status)
			{
				case GameStatus.Won:
					if (NotifySendResult != null)
						await NotifySendResult($"Вы отгадали загаданное число");
					break;

				case GameStatus.Lost:
					if (NotifySendResult != null)
						await NotifySendResult($"Вы проиграли. Загаданное число: {guessesNumber}");
					break;
			}			
		}

		public string rulesInfo()
		{
			return $"Бот загадывает число от 0 до {Setting.GNMaxNumber} (по умолчанию), а игрок пытается угадать за ограниченное число попыток ({Setting.GNMaxTries} по умолчанию). Когда игрок делает предположение о загаданном числе, бот сообщает о том угадано ли число, меньше ли оно загаданного, или больше. Если угадано - игра завершена. Если меньше или больше загаданного, то игрок продолжает пытаться угадать. Так происходит до тех пор пока либо число не угадано, либо исчерпано кол-во попыток. Команда /game_cancel завершает игру.";
		}
	}
}

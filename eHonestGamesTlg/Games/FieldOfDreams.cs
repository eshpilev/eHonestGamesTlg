namespace eHonestGamesTlg.Games
{	
	public class FieldOfDreams : IGame
	{			
		public event IGame.SendAndWaitMessage? NotifySendAndWaitMessage;
		public event IGame.SendResult? NotifySendResult;	

		private const int maxTriesDefault = 6;		
		private int maxTries; 
		private readonly string hiddenWord;
		public GameStatus Status { get; set; }		

		public FieldOfDreams()
		{			
			this.hiddenWord = this.GenerateWord();
			this.Status = GameStatus.NonStarted;			
		}	
		
		public async void StartAsync()
		{
			try
			{
				this.Status = GameStatus.Started;

				await InitParametersAsync();
				await RunAsync();
				await ResultAsync();
			}					
			catch (Exception ex)
			{
				if (NotifySendResult != null)
					await NotifySendResult(ex.Message);
			}
		}

		private async Task InitParametersAsync()
		{
			string? response = string.Empty;

			if (NotifySendAndWaitMessage != null)
				response = await NotifySendAndWaitMessage.Invoke($"Введите максимальное количество попыток (по умолчанию {Setting.FODMaxTries}):");

			if (this.Status == GameStatus.Interrupted)
				return;

			int.TryParse(response, out this.maxTries);

			if (this.maxTries == 0)
				this.maxTries = Setting.FODMaxTries;
		}

		private async Task RunAsync()
		{
			int remainTries = this.maxTries;
			SortedSet<char> userChars = new SortedSet<char>();
			string visibleWord = this.VisibleWord(userChars);
	
			while (this.Status == GameStatus.Started)
			{
				string response = string.Empty;

				if (NotifySendAndWaitMessage != null)				
					response = await NotifySendAndWaitMessage(LabelStep(visibleWord, userChars, remainTries));				

				if (this.Status == GameStatus.Interrupted)				
					break;				

				char? letter = response != string.Empty ? response.ToLower().Trim().ToCharArray()[0] : null;

				if (letter == null)
				{
					remainTries--;
					continue;
				}

				if (!hiddenWord.Contains((char)letter))				
					remainTries--;				

				userChars.Add((char)letter);

				visibleWord = this.VisibleWord(userChars);

				if (visibleWord == this.hiddenWord)				
					this.Status = GameStatus.Won;				

				if (remainTries <= 0)				
					this.Status = GameStatus.Lost;				
			}
		}				

		private async Task ResultAsync()
		{
			switch (this.Status)
			{
				case GameStatus.Won:									
					if (NotifySendResult != null)					
						await NotifySendResult($"Вы отгадали слово: {hiddenWord}");		
					break;

				case GameStatus.Lost:
					if (NotifySendResult != null)
						await NotifySendResult($"Вы проиграли, загаданное слово: {hiddenWord}");
					break;
			}
		}

		private string LabelStep(string visibleWord, SortedSet<char> userChars, int remainTries)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.AppendLine($"Загаданное слово: {visibleWord}");
			sb.AppendLine($"Оставшееся число попыток: {remainTries}");
			if (userChars.Count > 0)
			{
				sb.AppendLine($"Использованные буквы: {String.Join(", ", userChars.ToArray())}");
			}

			sb.Append("Введите букву:");

			return sb.ToString();
		}

		private string VisibleWord(SortedSet<char> userChars)
		{
			string word = string.Empty;

			for (int i = 0; i < hiddenWord.Length; i++)
			{
				if (userChars.Contains(hiddenWord[i]))
					word += hiddenWord[i];
				else
					word += "- ";
			}

			return word;
		}

		private string GenerateWord()
		{
			string[] words = File.ReadAllLines("WordsStockRus.txt");
			int index = new Random().Next(words.Length - 1);

			return words[index];
		}

		public string rulesInfo()
		{
			return $"Компьютер загадывает любое слово. Игрок, называя буквы, пытается угадать слово. Если буква есть в слове, компьютер вскрывает отгаданные буквы. Неотгаданные буквы не вскрываются, а выводятся прочерками (дефисами). Есть ограниченное кол-во попыток (по умолчанию, максимум {Setting.FODMaxTries}). Если попытки исчерпаны, то игрок проиграл. Команда /game_cancel завершает игру.";
		}
	}
}

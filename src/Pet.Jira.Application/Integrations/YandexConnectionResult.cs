namespace Pet.Jira.Application.Integrations
{
	public class YandexConnectionResult
	{
		public bool IsSuccess { get; set; }
		public string Error { get; set; }

		public static YandexConnectionResult Success()
		{
			return new YandexConnectionResult { IsSuccess = true };
		}

		public static YandexConnectionResult Failure(string error)
		{
			return new YandexConnectionResult
			{
				IsSuccess = false,
				Error = error
			};
		}
	}
}

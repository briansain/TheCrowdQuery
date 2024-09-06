namespace CrowdQuery.Sagas.QuestionSaga
{
	public class QuestionSagaConfiguration
	{
		public int DebouceTimerMilliseconds { get; set; }
		public QuestionSagaConfiguration(int debounceTimerSeconds = 5000)
		{
			DebouceTimerMilliseconds = debounceTimerSeconds;
		}
	}
}

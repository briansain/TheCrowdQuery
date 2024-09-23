namespace CrowdQuery.AS.Sagas.PromptSaga
{
	public class PromptSagaConfiguration
	{
		public int DebouceTimerMilliseconds { get; set; }
		public PromptSagaConfiguration(int debounceTimerSeconds = 5000)
		{
			DebouceTimerMilliseconds = debounceTimerSeconds;
		}
	}
}

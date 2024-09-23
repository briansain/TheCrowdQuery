namespace CrowdQuery.AS.Sagas.PromptSaga.ResponseModels
{
	public class PromptStateUpdated
	{
		public string Prompt { get; set; }
		public Dictionary<string, long> Answers { get; set; }
		public int SubscriberCount { get; set; }

		public PromptStateUpdated(string Prompt, Dictionary<string, long> answers, int subscriberCount)
		{
			Prompt = Prompt;
			Answers = answers;
			SubscriberCount = subscriberCount;
		}
	}
}

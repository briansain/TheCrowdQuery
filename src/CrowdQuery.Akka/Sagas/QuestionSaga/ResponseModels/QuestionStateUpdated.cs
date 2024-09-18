namespace CrowdQuery.Akka.Sagas.QuestionSaga.ResponseModels
{
	public class QuestionStateUpdated
	{
		public string Question { get; set; }
		public Dictionary<string, long> Answers { get; set; }
		public int SubscriberCount { get; set; }

		public QuestionStateUpdated(string question, Dictionary<string, long> answers, int subscriberCount)
		{
			Question = question;
			Answers = answers;
			SubscriberCount = subscriberCount;
		}
	}
}
